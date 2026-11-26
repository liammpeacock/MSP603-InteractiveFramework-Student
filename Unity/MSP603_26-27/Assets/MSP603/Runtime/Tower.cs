using System;
using UnityEngine;
using UnityEngine.Events;

namespace MSP603.TowerDefense
{
    public sealed class Tower : MonoBehaviour
    {
        public enum AudioActivity { Idle, Firing, Targeting }

        [Header("Student audio authoring hooks")]
        [SerializeField] private UnityEvent _onPlaced = new();
        [SerializeField] private UnityEvent _onAttack = new();
        [SerializeField] private UnityEvent _onUpgrade = new();
        [SerializeField] private UnityEvent _onDestroyed = new();

        private TowerStats _stats;
        private float _baseRangeBonus;
        private float _cooldown;
        private int _level = 1;
        private Transform _turret;
        private TowerPresentation _presentation;
        private InfernoFlame _activeFlame;

        public int Level => _level;
        public float EffectiveRange => (_stats.Range + _baseRangeBonus + 0.35f * (_level - 1)) * 1.15f;
        public TowerType Type { get; private set; }
        public bool IsDestroyed { get; private set; }
        public int TotalGoldSpent { get; private set; }
        public AudioActivity CurrentAudioActivity { get; private set; } = AudioActivity.Idle;
        public event Action<int> UpgradeLevelChanged;
        public event Action<AudioActivity> AudioActivityChanged;
        public event Action TerminalStateEntered;
        public bool IsCombatEnabled { get; private set; } = true;

        public void Configure(TowerType type, float baseRangeBonus = 0f)
        {
            Type = type;
            _stats = TowerCatalog.Get(type);
            _baseRangeBonus = baseRangeBonus;
            TotalGoldSpent = _stats.Cost;
            BuildAppearance();
            _onPlaced?.Invoke();
        }

        public int UpgradeCost => Mathf.RoundToInt(_stats.Cost * (0.65f + 0.25f * _level));

        public static float CalculateDamageForLevel(float baseDamage, int level)
        {
            int upgradeStage = Mathf.Clamp(level - 1, 0, 2);
            float existingUpgradeDamage = baseDamage * (1f + 0.35f * upgradeStage);
            return existingUpgradeDamage * (1f + 0.10f * upgradeStage);
        }

        public bool TryUpgrade()
        {
            if (!IsCombatEnabled || IsDestroyed || _level >= 3)
            {
                return false;
            }

            int cost = UpgradeCost;
            if (!TowerDefenseGame.Instance.TrySpend(cost))
            {
                GameplayAudioEvents.RequestOneShot("TowerUpgradeFailed", transform.position);
                return false;
            }

            _level++;
            TotalGoldSpent += cost;
            _turret.localScale *= 1.12f;
            _presentation?.SetLevel(_level);
            UpgradeLevelChanged?.Invoke(_level);
            _onUpgrade?.Invoke();
            GameplayAudioEvents.RequestOneShot("TowerUpgraded", transform.position);
            return true;
        }

        /// <summary>
        /// Applies the tower's existing destroyed lifecycle without adding a separate
        /// health or damage system. Future gameplay that determines tower destruction
        /// should call this once when that condition is reached.
        /// </summary>
        public void DestroyTower(bool immediatePresentation = false)
        {
            if (IsDestroyed)
            {
                return;
            }

            IsDestroyed = true;
            SetAudioActivity(AudioActivity.Idle);
            _onDestroyed?.Invoke();
            if (_activeFlame != null)
            {
                _activeFlame.StopFlame();
                _activeFlame = null;
            }
            if (_presentation != null)
            {
                _presentation.PlayDestroyed(immediatePresentation);
            }
            else
            {
                // Towers without supplied destruction sprites still retain a simple,
                // permanent ruined presentation rather than disappearing or attacking.
                _turret.localRotation = Quaternion.Euler(18f, 0f, 28f);
                foreach (Renderer towerRenderer in GetComponentsInChildren<Renderer>())
                {
                    towerRenderer.material.color *= 0.35f;
                }
            }
            GameplayAudioEvents.RequestOneShot("TowerDestroyed", transform.position);
            if (Type == TowerType.Inferno)
                GameplayAudioEvents.RequestOneShot("InfernoDestroyed", transform.position);
        }

        private void Update()
        {
            if (!IsCombatEnabled || IsDestroyed)
            {
                return;
            }

            _cooldown -= Time.deltaTime;
            EnemyUnit target = FindTarget();
            if (target == null)
            {
                SetAudioActivity(AudioActivity.Idle);
                return;
            }

            SetAudioActivity(AudioActivity.Targeting);

            Vector3 direction = target.transform.position - _turret.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
            {
                _turret.forward = Vector3.Lerp(_turret.forward, direction.normalized, Time.deltaTime * 8f);
                _presentation?.SetAimDirection(direction);
            }

            if (_cooldown <= 0f)
            {
                Fire(target);
                _cooldown = _stats.Cooldown / (1f + 0.18f * (_level - 1));
            }
        }

        private EnemyUnit FindTarget()
        {
            EnemyUnit best = null;
            float bestProgress = -1f;
            float range = EffectiveRange;
            foreach (EnemyUnit enemy in TowerDefenseGame.Instance.ActiveEnemies)
            {
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                Vector3 offset = enemy.transform.position - transform.position;
                offset.y = 0f;
                if (offset.sqrMagnitude <= range * range && enemy.Progress > bestProgress)
                {
                    best = enemy;
                    bestProgress = enemy.Progress;
                }
            }

            return best;
        }

        private void Fire(EnemyUnit target)
        {
            SetAudioActivity(AudioActivity.Firing);
            float damage = CalculateDamageForLevel(_stats.Damage, _level);
            float cooldown = _stats.Cooldown / (1f + 0.18f * (_level - 1));
            GameplayAudioEvents.RequestTowerFire(Type, transform.position);
            if (Type == TowerType.Inferno)
            {
                _onAttack?.Invoke();
                GameObject flameObject = StudentAuthoringTemplates.InstantiatePrefab(
                    "Projectiles/InfernoFlame", "InfernoFlame") ?? new GameObject("InfernoFlame");
                InfernoFlame flame = flameObject.GetComponent<InfernoFlame>() ?? flameObject.AddComponent<InfernoFlame>();
                flame.Configure(_turret, target, damage, Mathf.Min(0.85f, cooldown * 0.72f));
                _activeFlame = flame;
                _presentation?.PlayFire(cooldown);
                GameplayAudioEvents.RequestOneShot("InfernoFlameStarted", transform.position);
                return;
            }

            PrimitiveType projectileShape = Type == TowerType.Archer ? PrimitiveType.Cube : PrimitiveType.Sphere;
            _onAttack?.Invoke();
            GameObject projectileObject = StudentAuthoringTemplates.InstantiatePrefab(
                $"Projectiles/{_stats.Name}Projectile", $"{_stats.Name}Projectile")
                ?? GameObject.CreatePrimitive(projectileShape);
            projectileObject.name = $"{_stats.Name}Projectile";
            Vector3 shotDirection = target.transform.position - _turret.position;
            shotDirection.y = 0f;
            projectileObject.transform.position = _turret.position
                + shotDirection.normalized * 0.7f
                + Vector3.up * 0.25f;
            Destroy(projectileObject.GetComponent<Collider>());
            Projectile projectile = projectileObject.GetComponent<Projectile>() ?? projectileObject.AddComponent<Projectile>();
            projectile.Configure(target, damage, _stats.ProjectileSpeed, _stats.SplashRadius, Type);
            _presentation?.PlayFire(cooldown);
            GameplayAudioEvents.RequestOneShot($"{_stats.Name}Fired", transform.position);
        }

        private void SetAudioActivity(AudioActivity activity)
        {
            if (CurrentAudioActivity == activity) return;
            CurrentAudioActivity = activity;
            AudioActivityChanged?.Invoke(activity);
        }

        public void EnterTerminalState(bool destroyed)
        {
            if (!IsCombatEnabled) return;
            IsCombatEnabled = false;
            SetAudioActivity(AudioActivity.Idle);
            if (_activeFlame != null)
            {
                _activeFlame.StopFlame();
                _activeFlame = null;
            }
            TerminalStateEntered?.Invoke();
            if (destroyed) DestroyTower(true);
        }

        private void BuildAppearance()
        {
            GameObject baseObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseObject.name = "TowerBase";
            baseObject.transform.SetParent(transform, false);
            baseObject.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            baseObject.transform.localScale = new Vector3(0.65f, 0.25f, 0.65f);
            baseObject.GetComponent<Renderer>().material = TowerDefenseGame.CreateMaterial(new Color(0.28f, 0.25f, 0.22f));
            Destroy(baseObject.GetComponent<Collider>());

            GameObject turretObject = GameObject.CreatePrimitive(Type == TowerType.Cannon ? PrimitiveType.Cube : PrimitiveType.Cylinder);
            turretObject.name = "Turret";
            turretObject.transform.SetParent(transform, false);
            turretObject.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            turretObject.transform.localScale = Type == TowerType.Cannon ? new Vector3(0.7f, 0.45f, 0.9f) : new Vector3(0.45f, 0.55f, 0.45f);
            turretObject.GetComponent<Renderer>().material = TowerDefenseGame.CreateMaterial(_stats.Color);
            Destroy(turretObject.GetComponent<Collider>());
            _turret = turretObject.transform;

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "DirectionMarker";
            marker.transform.SetParent(_turret, false);
            marker.transform.localPosition = new Vector3(0f, 0f, 0.65f);
            marker.transform.localScale = new Vector3(0.18f, 0.18f, 0.8f);
            marker.GetComponent<Renderer>().material = TowerDefenseGame.CreateMaterial(_stats.Color * 1.2f);
            Destroy(marker.GetComponent<Collider>());

            // Production sprites are the visible tower presentation for every type;
            // primitive geometry remains only as an internal construction aid.
            _presentation = gameObject.AddComponent<TowerPresentation>();
            _presentation.Configure(Type);
            if (_presentation.HasSpriteArtwork)
            {
                foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
                {
                    if (renderer != _presentation.SpriteRenderer) renderer.enabled = false;
                }
            }
            _presentation.SetLevel(_level);
        }
    }
}

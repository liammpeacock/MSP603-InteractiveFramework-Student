using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MSP603.TowerDefense
{
    public sealed class EnemyUnit : MonoBehaviour
    {
        public const float GlobalMovementSpeedMultiplier = 0.80f;

        [Header("Student audio authoring hooks")]
        [Tooltip("Invoked once when this spawned enemy begins moving. Connect an official audio component here for reusable movement audio.")]
        [SerializeField] private UnityEvent _onMovementStarted = new();
        [Tooltip("Invoked whenever this enemy receives damage.")]
        [SerializeField] private UnityEvent _onDamaged = new();
        [Tooltip("Invoked once when this enemy is defeated.")]
        [SerializeField] private UnityEvent _onDied = new();

        private IReadOnlyList<Vector3> _path;
        private int _pathIndex;
        private float _health;
        private float _maxHealth;
        private float _speed;
        private int _reward;
        private bool _resolved;
        private Transform _healthFill;
        private EnemyPresentation _presentation;
        private bool _movementAudioActive;

        public float Progress { get; private set; }
        public bool IsAlive => !_resolved && _health > 0f;
        public EnemyType Type { get; private set; }
        public bool IsTerminallyStopped { get; private set; }

        public void Configure(EnemyType type, IReadOnlyList<Vector3> path, float healthMultiplier, float speedMultiplier)
        {
            Type = type;
            _path = path;
            _pathIndex = 1;
            transform.position = path[0];

            switch (type)
            {
                case EnemyType.GoblinRaider:
                    _maxHealth = 42f * healthMultiplier;
                    _reward = 12;
                    SetBody(new Color(0.95f, 0.72f, 0.18f), new Vector3(0.55f, 0.75f, 0.55f));
                    break;
                case EnemyType.RuinKnight:
                    _maxHealth = 155f * healthMultiplier;
                    _reward = 28;
                    SetBody(new Color(0.35f, 0.38f, 0.44f), new Vector3(0.9f, 1.15f, 0.9f));
                    break;
                default:
                    _maxHealth = 75f * healthMultiplier;
                    _reward = 16;
                    SetBody(new Color(0.67f, 0.20f, 0.16f), new Vector3(0.72f, 0.9f, 0.72f));
                    break;
            }

            _speed = CalculateMovementSpeed(type, speedMultiplier);

            _health = _maxHealth;
            _presentation = GetComponent<EnemyPresentation>() ?? gameObject.AddComponent<EnemyPresentation>();
            _presentation.Configure(type);
            CreateHealthBar();
            _onMovementStarted?.Invoke();
            _movementAudioActive = true;
            GameplayAudioEvents.SetEnemyMovement(Type, transform, true);
            GameplayAudioEvents.RequestOneShot("EnemySpawned", transform.position);
        }

        public static float CalculateMovementSpeed(EnemyType type, float levelSpeedMultiplier)
        {
            float baseSpeed = type switch
            {
                EnemyType.GoblinRaider => 2.25f,
                EnemyType.RuinKnight => 0.72f,
                _ => 1.15f
            };
            return baseSpeed * levelSpeedMultiplier * GlobalMovementSpeedMultiplier;
        }

        public void ApplyDamage(float amount)
        {
            if (_resolved || IsTerminallyStopped || amount <= 0f)
            {
                return;
            }

            _health = Mathf.Max(0f, _health - amount);
            _onDamaged?.Invoke();
            GameplayAudioEvents.RequestEnemyOneShot("EnemyHit", Type, transform.position);
            UpdateHealthBar();
            _presentation?.PlayHit();
            if (_health <= 0f)
            {
                _resolved = true;
                StopMovementAudio();
                _onDied?.Invoke();
                GameplayAudioEvents.RequestEnemyOneShot("EnemyDefeated", Type, transform.position);
                TowerDefenseGame.Instance.ResolveEnemy(this, true, _reward);
                SetDefeatedPresentationState();
                if (_presentation == null || !_presentation.PlayDeath(() => Destroy(gameObject)))
                {
                    Destroy(gameObject);
                }
            }
        }

        private void Update()
        {
            if (_resolved || IsTerminallyStopped || _path == null || _pathIndex >= _path.Count)
            {
                return;
            }

            Vector3 target = _path[_pathIndex];
            Vector3 movement = target - transform.position;
            movement.y = 0f;
            float step = _speed * Time.deltaTime;
            if (movement.sqrMagnitude <= step * step)
            {
                transform.position = target;
                _pathIndex++;
                Progress = (float)_pathIndex / _path.Count;
                if (_pathIndex >= _path.Count)
                {
                    ReachGoal();
                }
            }
            else
            {
                transform.position += movement.normalized * step;
                transform.forward = movement.normalized;
                _presentation?.SetMoveDirection(movement);
            }
        }

        private void ReachGoal()
        {
            _resolved = true;
            StopMovementAudio();
            GameplayAudioEvents.RequestOneShot("EnemyEscaped", transform.position);
            TowerDefenseGame.Instance.ResolveEnemy(this, false, 0);
            Destroy(gameObject);
        }

        public void EnterTerminalState()
        {
            if (IsTerminallyStopped || _resolved) return;
            IsTerminallyStopped = true;
            StopMovementAudio();
            enabled = false;
        }

        private void OnDestroy()
        {
            StopMovementAudio();
        }

        private void StopMovementAudio()
        {
            if (!_movementAudioActive) return;
            _movementAudioActive = false;
            GameplayAudioEvents.SetEnemyMovement(Type, transform, false);
        }

        private void SetBody(Color color, Vector3 scale)
        {
            transform.localScale = scale;
            Renderer renderer = GetComponent<Renderer>();
            renderer.material = TowerDefenseGame.CreateMaterial(color);
        }

        private void CreateHealthBar()
        {
            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
            background.name = "HealthBarBackground";
            background.transform.SetParent(transform, false);
            background.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            background.transform.localScale = new Vector3(1.1f, 0.09f, 0.08f);
            background.GetComponent<Renderer>().material = TowerDefenseGame.CreateMaterial(new Color(0.12f, 0.12f, 0.12f));
            Destroy(background.GetComponent<Collider>());

            GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fill.name = "HealthBarFill";
            fill.transform.SetParent(background.transform, false);
            fill.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            fill.transform.localScale = new Vector3(0.94f, 0.62f, 1f);
            fill.GetComponent<Renderer>().material = TowerDefenseGame.CreateMaterial(new Color(0.25f, 0.85f, 0.28f));
            Destroy(fill.GetComponent<Collider>());
            _healthFill = fill.transform;
        }

        private void UpdateHealthBar()
        {
            float ratio = _health / _maxHealth;
            _healthFill.localScale = new Vector3(0.94f * ratio, 0.62f, 1f);
            _healthFill.localPosition = new Vector3(-0.47f * (1f - ratio), 0f, -0.02f);
        }

        private void SetDefeatedPresentationState()
        {
            if (_healthFill != null)
            {
                _healthFill.parent.gameObject.SetActive(false);
            }

            foreach (Collider enemyCollider in GetComponents<Collider>())
            {
                enemyCollider.enabled = false;
            }
        }
    }
}

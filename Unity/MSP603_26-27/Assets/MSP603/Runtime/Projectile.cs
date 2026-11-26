using UnityEngine;
using UnityEngine.Events;

namespace MSP603.TowerDefense
{
    public sealed class Projectile : MonoBehaviour
    {
        [Header("Student audio authoring hooks")]
        [SerializeField] private UnityEvent _onImpact = new();

        private EnemyUnit _target;
        private float _damage;
        private float _speed;
        private float _splashRadius;
        private float _lifetime = 5f;
        private TowerType _sourceTowerType;

        public TowerType SourceTowerType => _sourceTowerType;

        public void Configure(EnemyUnit target, float damage, float speed, float splashRadius, TowerType sourceTowerType)
        {
            _target = target;
            _damage = damage;
            _speed = speed;
            _splashRadius = splashRadius;
            _sourceTowerType = sourceTowerType;
            ConfigureAppearance();
        }

        private void Update()
        {
            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f || _target == null || !_target.IsAlive)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 destination = _target.transform.position + Vector3.up * 0.5f;
            if (_sourceTowerType == TowerType.Archer)
            {
                transform.rotation = Quaternion.LookRotation(destination - transform.position);
            }

            transform.position = Vector3.MoveTowards(transform.position, destination, _speed * Time.deltaTime);
            if ((transform.position - destination).sqrMagnitude < 0.04f)
            {
                Impact();
            }
        }

        private void Impact()
        {
            _onImpact?.Invoke();
            if (_splashRadius <= 0f)
            {
                _target.ApplyDamage(_damage);
            }
            else
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, _splashRadius);
                foreach (Collider hit in hits)
                {
                    if (hit.TryGetComponent(out EnemyUnit enemy))
                    {
                        enemy.ApplyDamage(_damage);
                    }
                }
            }

            GameplayAudioEvents.RequestOneShot("ProjectileImpact", transform.position);
            Destroy(gameObject);
        }

        private void ConfigureAppearance()
        {
            Renderer projectileRenderer = GetComponent<Renderer>();
            Color projectileColor;

            switch (_sourceTowerType)
            {
                case TowerType.Archer:
                    // A long, narrow silhouette remains readable as an arrow at gameplay scale.
                    transform.localScale = new Vector3(0.09f, 0.09f, 0.65f);
                    projectileColor = new Color(0.92f, 0.78f, 0.42f);
                    break;
                case TowerType.Cannon:
                    transform.localScale = Vector3.one * 0.34f;
                    projectileColor = new Color(0.12f, 0.12f, 0.14f);
                    break;
                default:
                    // Mage and future non-physical towers retain a simple coloured orb.
                    transform.localScale = Vector3.one * 0.24f;
                    projectileColor = TowerCatalog.Get(_sourceTowerType).Color;
                    break;
            }

            projectileRenderer.material = TowerDefenseGame.CreateMaterial(projectileColor);
        }
    }
}

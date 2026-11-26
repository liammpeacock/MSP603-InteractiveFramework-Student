using UnityEngine;
using UnityEngine.Events;

namespace MSP603.TowerDefense
{
    /// <summary>A short sustained flame that applies one configured attack over safe damage ticks.</summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class InfernoFlame : MonoBehaviour
    {
        [Header("Student audio authoring hooks")]
        [SerializeField] private UnityEvent _onFlameStarted = new();
        [SerializeField] private UnityEvent _onBurn = new();
        [SerializeField] private UnityEvent _onFlameStopped = new();

        [Header("Visual fallback")]
        [Tooltip("Enable only when the tower has no approved fire sprite containing its own flame stream.")]
        [SerializeField] private bool _showRuntimeLine;

        private const float TickInterval = 0.2f;

        private LineRenderer _line;
        private Transform _origin;
        private EnemyUnit _target;
        private float _damagePerSecond;
        private float _damageRemaining;
        private float _remainingDuration;
        private float _tickTimer;
        private bool _reportedImpact;
        private bool _stopped;

        public void Configure(Transform origin, EnemyUnit target, float totalDamage, float duration)
        {
            _origin = origin;
            _target = target;
            _remainingDuration = Mathf.Max(TickInterval, duration);
            _damagePerSecond = totalDamage / _remainingDuration;
            _damageRemaining = totalDamage;
            _tickTimer = TickInterval;
            ConfigureLine();
            UpdateLine();
            _onFlameStarted?.Invoke();
            // The projectile towers apply damage as soon as their impact condition is met.
            // Apply Inferno's first bounded tick while the target passed by Tower is known
            // to be valid, then distribute only the remaining damage in Update.
            ApplyDamageTick(Mathf.Min(_damagePerSecond * TickInterval, _damageRemaining));
        }

        public void StopFlame()
        {
            if (_stopped) return;
            _stopped = true;
            if (_line != null) _line.enabled = false;
            _onFlameStopped?.Invoke();
            GameplayAudioEvents.RequestOneShot("InfernoFlameStopped", _origin != null ? _origin.position : transform.position);
            Destroy(gameObject);
        }

        private void Update()
        {
            if (_origin == null || _target == null || !_target.IsAlive)
            {
                StopFlame();
                return;
            }

            _remainingDuration -= Time.deltaTime;
            _tickTimer -= Time.deltaTime;
            UpdateLine();
            if (_tickTimer <= 0f)
            {
                _tickTimer += TickInterval;
                ApplyDamageTick(Mathf.Min(_damagePerSecond * TickInterval, _damageRemaining));
            }

            if (_remainingDuration <= 0f)
            {
                ApplyDamageTick(_damageRemaining);
                StopFlame();
            }
        }

        private void ApplyDamageTick(float damage)
        {
            if (damage <= 0f || _target == null || !_target.IsAlive) return;
            _target.ApplyDamage(damage);
            _damageRemaining -= damage;
            _onBurn?.Invoke();
            if (!_reportedImpact)
            {
                _reportedImpact = true;
                GameplayAudioEvents.RequestOneShot("InfernoBurn", _target.transform.position);
            }
        }

        private void ConfigureLine()
        {
            // UnityEngine.Object uses a custom null state for missing/destroyed components;
            // null-coalescing does not honour it and can retain a fake-null LineRenderer.
            _line = GetComponent<LineRenderer>();
            if (_line == null)
            {
                _line = gameObject.AddComponent<LineRenderer>();
            }
            _line.positionCount = 2;
            _line.useWorldSpace = true;
            _line.startWidth = 0.28f;
            _line.endWidth = 0.08f;
            _line.numCapVertices = 4;
            _line.startColor = new Color(1f, 0.25f, 0.02f, 0.95f);
            _line.endColor = new Color(1f, 0.82f, 0.08f, 0.25f);
            _line.material = TowerDefenseGame.CreateMaterial(Color.white);
            _line.enabled = _showRuntimeLine;
        }

        private void UpdateLine()
        {
            if (_line == null)
            {
                ConfigureLine();
            }

            Vector3 targetPosition = _target.transform.position + Vector3.up * 0.5f;
            Vector3 direction = (targetPosition - _origin.position).normalized;
            _line.SetPosition(0, _origin.position + direction * 0.7f + Vector3.up * 0.25f);
            _line.SetPosition(1, targetPosition);
        }
    }
}

using System.Collections;
using FMODUnity;
using UnityEngine;

namespace MSP603.TowerDefense.FMODIntegration
{
    [AddComponentMenu("MSP603/Audio/FMOD Tower Parameters")]
    [RequireComponent(typeof(Tower), typeof(StudioEventEmitter))]
    public sealed class MSP603FMODTowerParameters : MonoBehaviour
    {
        private const string UpgradeLevelParameter = "UpgradeLevel";
        private const string ActivityParameter = "Activity";
        private const string ProximityParameter = "Proximity";
        private const float ProximityRangeMultiplier = 1.25f;

        private Tower _tower;
        private StudioEventEmitter _emitter;
        private Camera _worldCamera;
        private bool _reportedProximityError;
        public bool TerminalStopRequested { get; private set; }
        public float LastProximityValue { get; private set; }

        private void Awake()
        {
            _tower = GetComponent<Tower>();
            _emitter = GetComponent<StudioEventEmitter>();
            _worldCamera = Camera.main;
        }

        private void OnEnable()
        {
            _tower.UpgradeLevelChanged += SetUpgradeLevel;
            _tower.AudioActivityChanged += SetActivity;
            _tower.TerminalStateEntered += StopForTerminalState;
            StartCoroutine(PublishInitialValues());
        }

        private void OnDisable()
        {
            _tower.UpgradeLevelChanged -= SetUpgradeLevel;
            _tower.AudioActivityChanged -= SetActivity;
            _tower.TerminalStateEntered -= StopForTerminalState;
        }

        private IEnumerator PublishInitialValues()
        {
            yield return null;
            SetUpgradeLevel(_tower.Level);
            SetActivity(_tower.CurrentAudioActivity);
            PublishProximity();
        }

        private void Update()
        {
            PublishProximity();
        }

        private void SetUpgradeLevel(int level)
        {
            SetLabel(UpgradeLevelParameter, Mathf.Clamp(level, 1, 3).ToString());
        }

        private void SetActivity(Tower.AudioActivity activity)
        {
            SetLabel(ActivityParameter, activity.ToString());
        }

        private void SetLabel(string parameterName, string label)
        {
            if (_emitter.EventInstance.isValid())
            {
                _emitter.EventInstance.setParameterByNameWithLabel(parameterName, label);
            }
        }

        public static float CalculateProximity(Vector3 cursorWorldPosition, Vector3 towerWorldPosition, float effectiveRange)
        {
            if (effectiveRange <= 0f || float.IsNaN(effectiveRange) || float.IsInfinity(effectiveRange)) return 0f;

            Vector3 offset = cursorWorldPosition - towerWorldPosition;
            offset.y = 0f;
            float proximityRadius = effectiveRange * ProximityRangeMultiplier;
            return Mathf.Clamp01(1f - offset.magnitude / proximityRadius);
        }

        private void PublishProximity()
        {
            float proximity = TryGetCursorWorldPosition(out Vector3 cursorWorldPosition)
                ? CalculateProximity(cursorWorldPosition, transform.position, _tower.EffectiveRange)
                : 0f;
            LastProximityValue = proximity;

            if (!_emitter.EventInstance.isValid()) return;

            FMOD.RESULT result = _emitter.EventInstance.setParameterByName(ProximityParameter, proximity);
            if (result != FMOD.RESULT.OK && !_reportedProximityError)
            {
                _reportedProximityError = true;
                Debug.LogWarning(
                    $"MSP603 FMOD: Could not set local parameter '{ProximityParameter}' on this tower's " +
                    $"Tower Ambience EventInstance ({result}). Confirm the event parameter contract.", this);
            }
        }

        private bool TryGetCursorWorldPosition(out Vector3 cursorWorldPosition)
        {
            cursorWorldPosition = default;
            if (!Input.mousePresent) return false;

            Vector3 screenPosition = Input.mousePosition;
            if (screenPosition.x < 0f || screenPosition.y < 0f ||
                screenPosition.x > Screen.width || screenPosition.y > Screen.height)
            {
                return false;
            }

            if (_worldCamera == null) _worldCamera = Camera.main;
            if (_worldCamera == null) return false;

            Ray cursorRay = _worldCamera.ScreenPointToRay(screenPosition);
            Plane gameplayPlane = new(Vector3.up, transform.position);
            if (!gameplayPlane.Raycast(cursorRay, out float distance)) return false;

            cursorWorldPosition = cursorRay.GetPoint(distance);
            return IsFinite(cursorWorldPosition.x) &&
                   IsFinite(cursorWorldPosition.y) &&
                   IsFinite(cursorWorldPosition.z);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private void StopForTerminalState()
        {
            TerminalStopRequested = true;
            StopAllCoroutines();
            _emitter.Stop();
            enabled = false;
        }
    }
}

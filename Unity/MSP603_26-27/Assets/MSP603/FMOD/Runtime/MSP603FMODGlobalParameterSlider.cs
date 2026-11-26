using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

namespace MSP603.TowerDefense.FMODIntegration
{
    [AddComponentMenu("MSP603/Audio/FMOD Global Parameter Slider")]
    [RequireComponent(typeof(Slider))]
    public sealed class MSP603FMODGlobalParameterSlider : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private string _globalParameterName;

        private bool _subscribed;
        private bool _reportedFailure;

        public Slider Slider => _slider;
        public string GlobalParameterName => _globalParameterName;
        public float LastAttemptedValue { get; private set; }

        public void Configure(Slider slider, string globalParameterName)
        {
            _slider = slider;
            _globalParameterName = globalParameterName;
        }

        private void Reset()
        {
            _slider = GetComponent<Slider>();
        }

        private void Start()
        {
            Subscribe();
            SendCurrentStateValue();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_subscribed) return;
            if (_slider == null) _slider = GetComponent<Slider>();
            if (_slider == null || string.IsNullOrWhiteSpace(_globalParameterName))
            {
                Debug.LogError("FMOD Global Parameter Slider requires a Slider and an exact global parameter name.", this);
                return;
            }

            GameplayAudioEvents.ParameterChanged += HandleGameplayParameter;
            _slider.onValueChanged.AddListener(SetGlobalParameter);
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            GameplayAudioEvents.ParameterChanged -= HandleGameplayParameter;
            if (_slider != null) _slider.onValueChanged.RemoveListener(SetGlobalParameter);
            _subscribed = false;
        }

        private void SendCurrentStateValue()
        {
            switch (_globalParameterName)
            {
                case "RTPC_Master_Volume": SetGlobalParameter(AudioControlState.MasterVolume * 100f); break;
                case "RTPC_Music_Volume": SetGlobalParameter(AudioControlState.MusicVolume * 100f); break;
                case "RTPC_SFX_Volume": SetGlobalParameter(AudioControlState.SfxVolume * 100f); break;
            }
        }

        private void HandleGameplayParameter(string parameterName, float value)
        {
            bool matches = (_globalParameterName == "RTPC_Master_Volume" && parameterName == "MasterVolume") ||
                           (_globalParameterName == "RTPC_Music_Volume" && parameterName == "MusicVolume") ||
                           (_globalParameterName == "RTPC_SFX_Volume" && parameterName == "SfxVolume");
            if (matches) SetGlobalParameter(value * 100f);
        }

        private void SetGlobalParameter(float value)
        {
            LastAttemptedValue = Mathf.Clamp(value, 0f, 100f);
            FMOD.RESULT result = RuntimeManager.StudioSystem.setParameterByName(_globalParameterName, LastAttemptedValue);
            if (result == FMOD.RESULT.OK)
            {
                _reportedFailure = false;
                return;
            }

            if (_reportedFailure) return;
            _reportedFailure = true;
            Debug.LogWarning(
                $"FMOD global parameter '{_globalParameterName}' could not be set ({result}). " +
                "Create it as a Global Continuous parameter with range 0-100, build banks, and confirm the spelling.", this);
        }
    }
}

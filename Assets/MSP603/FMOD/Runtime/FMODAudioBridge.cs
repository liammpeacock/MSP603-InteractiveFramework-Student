using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace MSP603.TowerDefense.FMODIntegration
{
    [AddComponentMenu("MSP603/Audio/FMOD Audio Bridge")]
    public sealed class FMODAudioBridge : MonoBehaviour
    {
        private const string WaveCounterGameplayParameter = "WaveCounter";
        private const string WaveCounterFmodParameter = "RTPC_WaveCounter";
        private const string LivesFmodParameter = "Lives";
        private const string EnemyHitGameplayEvent = "EnemyHit";
        private const string EnemyKilledGameplayEvent = "EnemyDefeated";
        private const string PlayerLifeLostGameplayEvent = "PlayerLifeLost";
        private const string PlayerLifeLostAuthoringSource = "Player Life Lost";
        private const string TowerBuiltGameplayEvent = "TowerBuilt";
        private const string TowerPlacedAuthoringSource = "Tower Placed";
        private const string TowerUpgradedGameplayEvent = "TowerUpgraded";
        private const string TowerUpgradedAuthoringSource = "Tower Upgraded";
        private const string TowerUpgradeFailedGameplayEvent = "TowerUpgradeFailed";
        private const string TowerUpgradeFailedAuthoringSource = "Tower Upgrade Failed";
        private const string TowerFireAuthoringSource = "Tower Fire";
        private const string TowerTypeParameter = "TowerType";
        private const string EnemyMovementAuthoringRoot = "Enemy Movement";
        private const string PausedParameter = "Paused";
        private const string PausedLabel = "Paused";
        private const string UnpausedLabel = "Unpaused";
        private const string PauseMenuAmbienceAuthoringSource = "Pause Menu Ambience";

        [Header("Teaching setup")]
        [Tooltip("Enable only after the FMOD project, banks, events, parameters, and VCAs have been configured.")]
        [SerializeField] private bool _enableBackendBridge;

        [Serializable]
        private sealed class OneShotBinding
        {
            [Tooltip("Semantic gameplay hook, for example TowerBuilt or ArcherFired.")]
            public string GameplayEvent;

            [Tooltip("Drag an event from the FMOD Event Browser into this field.")]
            public EventReference FmodEvent;
        }

        [Header("One-shot gameplay events")]
        [SerializeField] private List<OneShotBinding> _oneShots = new();

        [Header("Mixer controls")]
        [Tooltip("The nested Music and SFX buses preserve independent gameplay mixer controls while UI remains outside Gameplay.")]
        [SerializeField] private string _masterVca = "vca:/Master";
        [SerializeField] private string _musicBus = "bus:/Gameplay/Music";
        [SerializeField] private string _sfxBus = "bus:/Gameplay/SFX";

        private bool _reportedWaveCounterError;
        private bool _reportedPausedParameterError;
        private bool _pauseMenuAmbiencePlaying;
        private readonly Dictionary<Transform, EventInstance> _enemyMovementInstances = new();

        public string LastWaveCounterLabel { get; private set; }
        public float LastLivesValue { get; private set; }
        public string LastTerminalState { get; private set; }
        public string LastPlayedAuthoringEmitter { get; private set; }
        public int AuthoringEmitterPlayCount { get; private set; }
        public int TowerFireEmitterPlayCount { get; private set; }
        public int LastTowerFireTypeValue { get; private set; } = -1;
        public string LastTowerFireTypeLabel { get; private set; }
        public Vector3 LastTowerFirePosition { get; private set; }
        public float LastMusicControlValue { get; private set; } = 1f;
        public float LastSfxControlValue { get; private set; } = 1f;
        public string LastPauseStateLabel { get; private set; }
        public int PauseMenuAmbiencePlayCount { get; private set; }
        public int PauseMenuAmbienceStopCount { get; private set; }
        public string LastStoppedAuthoringEmitter { get; private set; }
        public string LastStartedEnemyMovementEvent { get; private set; }
        public int ActiveEnemyMovementCount => _enemyMovementInstances.Count;

        private void OnEnable()
        {
            GameplayAudioEvents.OneShotRequested += HandleOneShot;
            GameplayAudioEvents.TowerFireRequested += HandleTowerFire;
            GameplayAudioEvents.EnemyOneShotRequested += HandleEnemyOneShot;
            GameplayAudioEvents.EnemyMovementChanged += HandleEnemyMovement;
            GameplayAudioEvents.ParameterChanged += HandleParameter;
            GameplayAudioEvents.PauseStateChanged += HandlePauseStateChanged;
        }

        private void Start()
        {
            HandlePauseStateChanged(false);
            AudioControlState.PublishCurrentState();
        }

        private void OnDisable()
        {
            GameplayAudioEvents.OneShotRequested -= HandleOneShot;
            GameplayAudioEvents.TowerFireRequested -= HandleTowerFire;
            GameplayAudioEvents.EnemyOneShotRequested -= HandleEnemyOneShot;
            GameplayAudioEvents.EnemyMovementChanged -= HandleEnemyMovement;
            GameplayAudioEvents.ParameterChanged -= HandleParameter;
            GameplayAudioEvents.PauseStateChanged -= HandlePauseStateChanged;
            HandlePauseStateChanged(false);
            StopAllEnemyMovement();
        }

        private void HandlePauseStateChanged(bool paused)
        {
            string label = paused ? PausedLabel : UnpausedLabel;
            LastPauseStateLabel = label;
            FMOD.RESULT result = RuntimeManager.StudioSystem.setParameterByNameWithLabel(PausedParameter, label);
            if (result != FMOD.RESULT.OK && !_reportedPausedParameterError)
            {
                _reportedPausedParameterError = true;
                Debug.LogWarning(
                    $"MSP603 FMOD: Could not set Global labelled parameter '{PausedParameter}' " +
                    $"to '{label}' ({result}). Confirm the parameter and labels match the integration contract.", this);
            }

            if (paused)
            {
                if (_pauseMenuAmbiencePlaying) return;
                StudioEventEmitter emitter = FindAuthoringEmitter(PauseMenuAmbienceAuthoringSource);
                if (emitter == null) return;
                _pauseMenuAmbiencePlaying = true;
                PauseMenuAmbiencePlayCount++;
                emitter.Play();
                return;
            }

            if (!_pauseMenuAmbiencePlaying) return;
            _pauseMenuAmbiencePlaying = false;
            PauseMenuAmbienceStopCount++;
            StopAuthoringEmitter(PauseMenuAmbienceAuthoringSource);
        }

        private void HandleOneShot(string gameplayEvent, Vector3 position)
        {
            if (string.Equals(gameplayEvent, PlayerLifeLostGameplayEvent, StringComparison.Ordinal))
            {
                PlayAuthoringEmitter(PlayerLifeLostAuthoringSource);
                return;
            }

            if (string.Equals(gameplayEvent, TowerBuiltGameplayEvent, StringComparison.Ordinal))
            {
                PlayAuthoringEmitter(TowerPlacedAuthoringSource);
                return;
            }

            if (string.Equals(gameplayEvent, TowerUpgradedGameplayEvent, StringComparison.Ordinal))
            {
                PlayAuthoringEmitter(TowerUpgradedAuthoringSource);
                return;
            }

            if (string.Equals(gameplayEvent, TowerUpgradeFailedGameplayEvent, StringComparison.Ordinal))
            {
                PlayAuthoringEmitter(TowerUpgradeFailedAuthoringSource);
                return;
            }

            if (!_enableBackendBridge || !AudioControlState.SfxEnabled)
            {
                return;
            }

            foreach (OneShotBinding binding in _oneShots)
            {
                if (string.Equals(binding.GameplayEvent, gameplayEvent, StringComparison.Ordinal) && !binding.FmodEvent.IsNull)
                {
                    RuntimeManager.PlayOneShot(binding.FmodEvent, position);
                    return;
                }
            }
        }

        private void HandleEnemyOneShot(string gameplayEvent, EnemyType enemyType, Vector3 position)
        {
            string action = string.Equals(gameplayEvent, EnemyHitGameplayEvent, StringComparison.Ordinal)
                ? "Hit"
                : string.Equals(gameplayEvent, EnemyKilledGameplayEvent, StringComparison.Ordinal) ? "Death" : null;
            if (action == null)
            {
                return;
            }

            string enemyName = enemyType switch
            {
                EnemyType.GoblinRaider => "Goblin Raider",
                EnemyType.GoblinBrute => "Goblin Brute",
                EnemyType.RuinKnight => "Ruin Knight",
                _ => null
            };
            if (enemyName != null)
            {
                PlayAuthoringEmitter($"Enemy Damage and Death/{enemyName}/{action}");
            }
        }

        private void HandleTowerFire(TowerType towerType, Vector3 position)
        {
            StudioEventEmitter emitter = transform.Find(TowerFireAuthoringSource)?.GetComponent<StudioEventEmitter>();
            if (emitter == null) return;

            int towerTypeValue = GameplayAudioEvents.TowerTypeParameterValue(towerType);
            string towerTypeLabel = GameplayAudioEvents.TowerTypeParameterLabel(towerType);
            emitter.transform.position = position;
            foreach (ParamRef parameter in emitter.Params)
            {
                if (string.Equals(parameter.Name, TowerTypeParameter, StringComparison.Ordinal))
                    parameter.Value = towerTypeValue;
            }

            LastTowerFireTypeValue = towerTypeValue;
            LastTowerFireTypeLabel = towerTypeLabel;
            LastTowerFirePosition = position;
            TowerFireEmitterPlayCount++;
            LastPlayedAuthoringEmitter = TowerFireAuthoringSource;
            AuthoringEmitterPlayCount++;
            emitter.Play();
            if (emitter.EventInstance.isValid())
                emitter.EventInstance.setParameterByNameWithLabel(TowerTypeParameter, towerTypeLabel);
        }

        private void HandleEnemyMovement(EnemyType enemyType, Transform enemy, bool moving)
        {
            if (enemy == null) return;
            if (!moving)
            {
                StopEnemyMovement(enemy);
                return;
            }

            StopEnemyMovement(enemy);
            string sourceName = enemyType switch
            {
                EnemyType.GoblinRaider => "Goblin Raider Movement",
                EnemyType.GoblinBrute => "Goblin Brute Movement",
                EnemyType.RuinKnight => "Ruin Knight Movement",
                _ => null
            };
            if (sourceName == null) return;

            StudioEventEmitter source = transform
                .Find($"{EnemyMovementAuthoringRoot}/{sourceName}")
                ?.GetComponent<StudioEventEmitter>();
            if (source == null || source.EventReference.IsNull) return;

            EventInstance instance = RuntimeManager.CreateInstance(source.EventReference);
            RuntimeManager.AttachInstanceToGameObject(instance, enemy.gameObject);
            instance.start();
            _enemyMovementInstances.Add(enemy, instance);
            LastStartedEnemyMovementEvent = source.EventReference.ToString();
        }

        private void StopEnemyMovement(Transform enemy)
        {
            if (!_enemyMovementInstances.Remove(enemy, out EventInstance instance)) return;
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            RuntimeManager.DetachInstanceFromGameObject(instance);
            instance.release();
        }

        private void StopAllEnemyMovement()
        {
            foreach (EventInstance instance in _enemyMovementInstances.Values)
            {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                RuntimeManager.DetachInstanceFromGameObject(instance);
                instance.release();
            }
            _enemyMovementInstances.Clear();
        }

        private void HandleParameter(string parameter, float value)
        {
            if (string.Equals(parameter, WaveCounterGameplayParameter, StringComparison.Ordinal))
            {
                SetWaveCounter(value);
                return;
            }

            if (string.Equals(parameter, LivesFmodParameter, StringComparison.Ordinal))
            {
                LastLivesValue = Mathf.Clamp(value, 0f, 20f);
                RuntimeManager.StudioSystem.setParameterByName(LivesFmodParameter, LastLivesValue);
                return;
            }

            // Settings use the project's authored VCAs even while the optional generic
            // backend teaching bridge is disabled.
            switch (parameter)
            {
                case "MusicVolume":
                    SetMusicBus(AudioControlState.MusicEnabled ? value : 0f);
                    return;
                case "SfxVolume":
                    SetSfxBus(AudioControlState.SfxEnabled ? value : 0f);
                    return;
                case "MusicEnabled":
                    SetMusicBus(value > 0.5f ? AudioControlState.MusicVolume : 0f);
                    return;
                case "SfxEnabled":
                    SetSfxBus(value > 0.5f ? AudioControlState.SfxVolume : 0f);
                    return;
            }

            if (!_enableBackendBridge)
            {
                return;
            }

            RuntimeManager.StudioSystem.setParameterByName(parameter, value);
        }

        private void SetMusicBus(float value)
        {
            LastMusicControlValue = Mathf.Clamp01(value);
            SetBus(_musicBus, LastMusicControlValue);
        }

        private void SetSfxBus(float value)
        {
            LastSfxControlValue = Mathf.Clamp01(value);
            SetBus(_sfxBus, LastSfxControlValue);
        }

        private void SetWaveCounter(float value)
        {
            int wave = Mathf.RoundToInt(value);
            string label = wave switch
            {
                0 => "Wave0",
                1 => "Wave1",
                2 => "Wave2",
                3 => "Wave3",
                4 => "Wave4",
                5 => "Victory",
                6 => "Defeat",
                _ => null
            };

            if (label == null)
            {
                Debug.LogError($"MSP603 FMOD: WaveCounter received unsupported gameplay value {value}.", this);
                return;
            }

            LastWaveCounterLabel = label;
            FMOD.RESULT result = RuntimeManager.StudioSystem.setParameterByNameWithLabel(
                WaveCounterFmodParameter, label);
            if (result != FMOD.RESULT.OK && !_reportedWaveCounterError)
            {
                _reportedWaveCounterError = true;
                Debug.LogWarning(
                    $"MSP603 FMOD: Could not set Global labelled parameter '{WaveCounterFmodParameter}' " +
                    $"to '{label}' ({result}). Confirm the parameter and labels match the integration contract.", this);
            }

            if (wave is 5 or 6)
            {
                LastTerminalState = label;
                StopAuthoringEmitter("Level Ambience");
            }
        }

        private static void SetVca(string path, float value)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            FMOD.RESULT result = RuntimeManager.StudioSystem.getVCA(path, out VCA vca);
            if (result == FMOD.RESULT.OK)
            {
                vca.setVolume(Mathf.Clamp01(value));
            }
        }

        private static void SetBus(string path, float value)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            FMOD.RESULT result = RuntimeManager.StudioSystem.getBus(path, out Bus bus);
            if (result == FMOD.RESULT.OK)
            {
                bus.setVolume(Mathf.Clamp01(value));
            }
        }

        private void StopAuthoringEmitter(string sourceName)
        {
            StudioEventEmitter emitter = FindAuthoringEmitter(sourceName);
            if (emitter != null)
            {
                LastStoppedAuthoringEmitter = sourceName;
                emitter.Stop();
            }
        }

        private void PlayAuthoringEmitter(string sourceName)
        {
            StudioEventEmitter emitter = FindAuthoringEmitter(sourceName);
            if (emitter != null)
            {
                LastPlayedAuthoringEmitter = sourceName;
                AuthoringEmitterPlayCount++;
                emitter.Play();
            }
        }

        private StudioEventEmitter FindAuthoringEmitter(string sourceName)
        {
            return transform.Find(sourceName)?.GetComponent<StudioEventEmitter>();
        }

    }
}

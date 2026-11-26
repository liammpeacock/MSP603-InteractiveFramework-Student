using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using FMODUnity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MSP603.TowerDefense.FMODIntegration.Tests
{
    public sealed class FMODTeachingRigPlayModeTests
    {
        private float[] _savedVolumes;
        private bool[] _savedEnabled;
        private bool[] _preferenceExisted;
        private int[] _savedPreferences;
        private static readonly string[] PreferenceKeys = { AudioControlState.MusicEnabledPreference, AudioControlState.SfxEnabledPreference, "MSP603.HighestUnlockedLevel", "MSP603.GameCompleted" };

        [SetUp]
        public void PreserveUserState()
        {
            _savedVolumes = new[] { AudioControlState.MasterVolume, AudioControlState.MusicVolume, AudioControlState.SfxVolume };
            _savedEnabled = new[] { AudioControlState.MusicEnabled, AudioControlState.SfxEnabled };
            _preferenceExisted = new bool[PreferenceKeys.Length];
            _savedPreferences = new int[PreferenceKeys.Length];
            for (int i = 0; i < PreferenceKeys.Length; i++)
            {
                _preferenceExisted[i] = PlayerPrefs.HasKey(PreferenceKeys[i]);
                _savedPreferences[i] = PlayerPrefs.GetInt(PreferenceKeys[i]);
            }
            Time.timeScale = 1f;
            MainMenuController.OpenLevelSelectOnLoad = false;
            LogAssert.ignoreFailingMessages = false;
        }

        [TearDown]
        public void RestoreUserState()
        {
            Time.timeScale = 1f;
            MainMenuController.OpenLevelSelectOnLoad = false;
            AudioControlState.SetMasterVolume(_savedVolumes[0]);
            AudioControlState.SetMusicVolume(_savedVolumes[1]);
            AudioControlState.SetSfxVolume(_savedVolumes[2]);
            AudioControlState.SetMusicEnabled(_savedEnabled[0]);
            AudioControlState.SetSfxEnabled(_savedEnabled[1]);
            for (int i = 0; i < PreferenceKeys.Length; i++)
            {
                if (_preferenceExisted[i]) PlayerPrefs.SetInt(PreferenceKeys[i], _savedPreferences[i]);
                else PlayerPrefs.DeleteKey(PreferenceKeys[i]);
            }
            PlayerPrefs.Save();
            LogAssert.ignoreFailingMessages = false;
        }

        private static GameObject FindProductionObject(string name)
        {
            foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (canvas.name != "Main Menu Canvas" && canvas.name != "Gameplay UI Canvas") continue;
                foreach (Transform item in canvas.GetComponentsInChildren<Transform>(false))
                    if (item.name == name) return item.gameObject;
            }
            return null;
        }


        [TestCase(0f, 10f, 1f)]
        [TestCase(5f, 10f, 0.6f)]
        [TestCase(10f, 10f, 0.2f)]
        [TestCase(12.5f, 10f, 0f)]
        [TestCase(15f, 10f, 0f)]
        [TestCase(-5f, 10f, 0.6f)]
        public void TowerProximityUsesExtendedAudioInteractionRadiusAndClamps(
            float cursorOffset, float effectiveRange, float expected)
        {
            float proximity = MSP603FMODTowerParameters.CalculateProximity(
                new Vector3(cursorOffset, 0f, 0f), Vector3.zero, effectiveRange);

            Assert.That(proximity, Is.EqualTo(expected).Within(0.0001f));
            Assert.That(proximity, Is.InRange(0f, 1f));
        }

        [Test]
        public void TowerProximityUsesEachTowersCurrentEffectiveRangeIndependently()
        {
            Vector3 cursor = new(3f, 0f, 0f);
            float shortRangeTower = MSP603FMODTowerParameters.CalculateProximity(cursor, Vector3.zero, 4f);
            float upgradedRangeTower = MSP603FMODTowerParameters.CalculateProximity(cursor, Vector3.zero, 6f);

            Assert.That(shortRangeTower, Is.EqualTo(0.4f).Within(0.0001f));
            Assert.That(upgradedRangeTower, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(shortRangeTower, Is.Not.EqualTo(upgradedRangeTower));
        }

        [UnityTest]
        public IEnumerator TowerAmbienceBankExposesLocalContinuousProximityContract()
        {
            LogAssert.ignoreFailingMessages = true;
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;

            Assert.That(RuntimeManager.StudioSystem.getEvent(
                "event:/Towers/Tower_Ambience", out FMOD.Studio.EventDescription description),
                Is.EqualTo(FMOD.RESULT.OK));
            Assert.That(description.getParameterDescriptionByName(
                "Proximity", out FMOD.Studio.PARAMETER_DESCRIPTION proximity),
                Is.EqualTo(FMOD.RESULT.OK));
            Assert.That(proximity.minimum, Is.EqualTo(0f));
            Assert.That(proximity.maximum, Is.EqualTo(1f));
            Assert.That(proximity.defaultvalue, Is.EqualTo(0f));
            Assert.That(proximity.type, Is.EqualTo(FMOD.Studio.PARAMETER_TYPE.GAME_CONTROLLED));
            Assert.That(proximity.flags & FMOD.Studio.PARAMETER_FLAGS.GLOBAL,
                Is.EqualTo((FMOD.Studio.PARAMETER_FLAGS)0));
            Assert.That(proximity.flags & FMOD.Studio.PARAMETER_FLAGS.DISCRETE,
                Is.EqualTo((FMOD.Studio.PARAMETER_FLAGS)0));
            Assert.That(proximity.flags & FMOD.Studio.PARAMETER_FLAGS.LABELED,
                Is.EqualTo((FMOD.Studio.PARAMETER_FLAGS)0));

            Assert.That(description.createInstance(out FMOD.Studio.EventInstance instance), Is.EqualTo(FMOD.RESULT.OK));
            Assert.That(instance.setParameterByName("Proximity", 0.42f), Is.EqualTo(FMOD.RESULT.OK));
            Assert.That(instance.getParameterByName("Proximity", out float value, out _), Is.EqualTo(FMOD.RESULT.OK));
            Assert.That(value, Is.EqualTo(0.42f).Within(0.0001f));
            instance.release();
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator MainMenuProvidesContinuousVolumeBridgesAndNativeEmitters()
        {
            LogAssert.ignoreFailingMessages = true;
            SceneManager.LoadScene("MainMenu");
            yield return null;
            yield return null;

            GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
            Assert.That(authoring, Is.Not.Null);
            Assert.That(authoring.GetComponent<FMODAudioBridge>(), Is.Not.Null);
            Assert.That(authoring.transform.Find("Main Menu Music").GetComponent<StudioEventEmitter>().EventReference.Path, Is.EqualTo("event:/Music/MainMenu"));
            Assert.That(authoring.transform.Find("Main Menu Ambience").GetComponent<StudioEventEmitter>(), Is.Not.Null);
            Assert.That(Camera.main.GetComponent<StudioListener>(), Is.Not.Null);

            FindProductionObject("Settings").GetComponent<Button>().onClick.Invoke();
            yield return null;
            yield return null;
            AssertSlider("MASTER VOLUME Slider", "RTPC_Master_Volume", 75f);
            AssertSlider("MUSIC VOLUME Slider", "RTPC_Music_Volume", 63.7f);
            AssertSlider("SFX VOLUME Slider", "RTPC_SFX_Volume", 25f);
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator SettingsTogglesDriveIndependentVcasAndPersistAcrossScenes()
        {
            LogAssert.ignoreFailingMessages = true;
            bool originalMusic = AudioControlState.MusicEnabled;
            bool originalSfx = AudioControlState.SfxEnabled;
            AudioControlState.SetMusicEnabled(true);
            AudioControlState.SetSfxEnabled(true);

            SceneManager.LoadScene("MainMenu");
            yield return null;
            yield return null;

            FMODAudioBridge menuBridge = GameObject.Find(StudentAuthoringTemplates.RootName).GetComponent<FMODAudioBridge>();
            FindProductionObject("Settings").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Toggle musicToggle = FindProductionObject("MUSIC ON").GetComponent<Toggle>();
            Toggle sfxToggle = FindProductionObject("SFX ON").GetComponent<Toggle>();
            Assert.That(musicToggle.isOn, Is.True);
            Assert.That(sfxToggle.isOn, Is.True);

            musicToggle.isOn = false;
            Assert.That(menuBridge.LastMusicControlValue, Is.Zero);
            Assert.That(menuBridge.LastSfxControlValue, Is.EqualTo(AudioControlState.SfxVolume));
            Assert.That(PlayerPrefs.GetInt(AudioControlState.MusicEnabledPreference), Is.Zero);

            sfxToggle.isOn = false;
            Assert.That(menuBridge.LastSfxControlValue, Is.Zero);
            Assert.That(menuBridge.LastMusicControlValue, Is.Zero);
            Assert.That(PlayerPrefs.GetInt(AudioControlState.SfxEnabledPreference), Is.Zero);

            musicToggle.isOn = true;
            Assert.That(menuBridge.LastMusicControlValue, Is.EqualTo(AudioControlState.MusicVolume));
            Assert.That(menuBridge.LastSfxControlValue, Is.Zero);

            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;
            FMODAudioBridge levelBridge = GameObject.Find(StudentAuthoringTemplates.RootName).GetComponent<FMODAudioBridge>();
            Assert.That(levelBridge.LastMusicControlValue, Is.EqualTo(AudioControlState.MusicVolume));
            Assert.That(levelBridge.LastSfxControlValue, Is.Zero);

            AudioControlState.SetMusicEnabled(originalMusic);
            AudioControlState.SetSfxEnabled(originalSfx);
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator EveryLevelProvidesPauseAmbienceWithRecoveredCanonicalIdentity()
        {
            LogAssert.ignoreFailingMessages = true;
            try
            {
                foreach (string scene in new[] { "Level01", "Level02", "Level03" })
                {
                    SceneManager.LoadScene(scene);
                    yield return null;
                    yield return null;
                    GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
                    Assert.That(authoring.transform.Find("Pause Menu Ambience"), Is.Not.Null, scene);
                    StudioEventEmitter emitter = authoring.transform.Find("Pause Menu Ambience").GetComponent<StudioEventEmitter>();
                    Assert.That(emitter.EventReference.IsNull, Is.False, scene);
                    Assert.That(emitter.EventReference.Path, Is.EqualTo("event:/UI/Pause_Menu_Ambience"), scene);
                    Assert.That(emitter.EventReference.Guid, Is.EqualTo(FMOD.GUID.Parse("{7b2e1694-b72e-47f2-8bbc-75959f198897}")), scene);
                    Assert.That(emitter.EventPlayTrigger, Is.EqualTo(EmitterGameEvent.None));
                    Assert.That(emitter.EventStopTrigger, Is.EqualTo(EmitterGameEvent.None));
                }
            }
            finally { LogAssert.ignoreFailingMessages = false; }
        }

        [UnityTest]
        public IEnumerator LifeLossPublishesLivesBeforeEventAcrossAllLevels()
        {
            List<string> observed = new();
            void Parameter(string name, float value) { if (name == "Lives") observed.Add("Lives:" + value); }
            void OneShot(string name, Vector3 position) { if (name == "PlayerLifeLost") observed.Add(name); }
            GameplayAudioEvents.ParameterChanged += Parameter;
            GameplayAudioEvents.OneShotRequested += OneShot;
            LogAssert.ignoreFailingMessages = true;
            try
            {
                foreach (string scene in new[] { "Level01", "Level02", "Level03" })
                {
                    observed.Clear();
                    SceneManager.LoadScene(scene);
                    yield return null;
                    yield return null;
                    Assert.That(observed, Does.Contain("Lives:20"), scene);
                    EnemyUnit enemy = CreateEnemy(EnemyType.GoblinRaider, "Life loss ordering test");
                    observed.Clear();
                    TowerDefenseGame.Instance.ResolveEnemy(enemy, false, 0);
                    CollectionAssert.AreEqual(new[] { "Lives:19", "PlayerLifeLost" }, observed, scene);
                    Object.Destroy(enemy.gameObject);
                    TowerDefenseGame.Instance.SetPaused(true);
                    TowerDefenseGame.Instance.SetPaused(false);
                    Assert.That(TowerDefenseGame.Instance.Lives, Is.EqualTo(19), scene);
                }
            }
            finally
            {
                GameplayAudioEvents.ParameterChanged -= Parameter;
                GameplayAudioEvents.OneShotRequested -= OneShot;
                LogAssert.ignoreFailingMessages = false;
                Time.timeScale = 1f;
            }
        }

        [UnityTest]
        public IEnumerator PausePublicationIsIdempotentAndTerminalStateResetsIt()
        {
            List<bool> observed = new();
            void Capture(bool paused) { observed.Add(paused); }
            GameplayAudioEvents.PauseStateChanged += Capture;
            LogAssert.ignoreFailingMessages = true;
            try
            {
                foreach (string scene in new[] { "Level01", "Level02", "Level03" })
                {
                    SceneManager.LoadScene(scene);
                    yield return null;
                    yield return null;
                    observed.Clear();
                    TowerDefenseGame game = TowerDefenseGame.Instance;
                    FMODAudioBridge bridge = GameObject.Find(StudentAuthoringTemplates.RootName).GetComponent<FMODAudioBridge>();
                    game.SetPaused(true);
                    game.SetPaused(true);
                    Assert.That(bridge.PauseMenuAmbiencePlayCount, Is.EqualTo(1), scene);
                    game.SetPaused(false);
                    Assert.That(bridge.PauseMenuAmbienceStopCount, Is.EqualTo(1), scene);
                    game.SetPaused(true);
                    Assert.That(game.TryEnterTerminalState(true), Is.True);
                    CollectionAssert.AreEqual(new[] { true, false, true, false }, observed, scene);
                    Assert.That(bridge.LastPauseStateLabel, Is.EqualTo("Unpaused"));
                    Assert.That(bridge.PauseMenuAmbienceStopCount, Is.EqualTo(2));
                    game.ReloadLevel();
                    yield return null;
                    yield return null;
                    Assert.That(TowerDefenseGame.Instance.Lives, Is.EqualTo(20));
                    Assert.That(TowerDefenseGame.Instance.IsPaused, Is.False);
                }
            }
            finally
            {
                GameplayAudioEvents.PauseStateChanged -= Capture;
                LogAssert.ignoreFailingMessages = false;
                Time.timeScale = 1f;
            }
        }

        [UnityTest, Category("RequiresV2FMODAuthoring")]
        public IEnumerator EnemyCountsPublishIndependentCapsAndResetAfterVictory()
        {
            LogAssert.ignoreFailingMessages = true;
            try
            {
                SceneManager.LoadScene("Level01");
                yield return null;
                yield return null;
                foreach (string name in new[] { "EnemyCount", "RunnerCount", "GruntCount", "ArmouredCount" })
                {
                    if (RuntimeManager.StudioSystem.getParameterDescriptionByName(name, out _) != FMOD.RESULT.OK)
                        Assert.Ignore("Pending manual FMOD authoring and rebuilt banks: missing " + name);
                }
                TowerDefenseGame game = TowerDefenseGame.Instance;
                game.SetPaused(true);
                MethodInfo spawn = typeof(TowerDefenseGame).GetMethod("SpawnEnemy", BindingFlags.Instance | BindingFlags.NonPublic);
                for (int i = 0; i < 61; i++) spawn.Invoke(game, new object[] { EnemyType.GoblinRaider, 1f, 1f });
                spawn.Invoke(game, new object[] { EnemyType.GoblinBrute, 1f, 1f });
                spawn.Invoke(game, new object[] { EnemyType.RuinKnight, 1f, 1f });
                AssertCount("EnemyCount", 60);
                AssertCount("RunnerCount", 60);
                AssertCount("GruntCount", 1);
                AssertCount("ArmouredCount", 1);
                foreach (EnemyUnit enemy in new List<EnemyUnit>(game.ActiveEnemies)) enemy.ApplyDamage(float.MaxValue);
                foreach (string name in new[] { "EnemyCount", "RunnerCount", "GruntCount", "ArmouredCount" }) AssertCount(name, 0);
                spawn.Invoke(game, new object[] { EnemyType.RuinKnight, 1f, 1f });
                game.TryEnterTerminalState(true);
                foreach (string name in new[] { "EnemyCount", "RunnerCount", "GruntCount", "ArmouredCount" }) AssertCount(name, 0);
            }
            finally { LogAssert.ignoreFailingMessages = false; Time.timeScale = 1f; }
        }

        private static void AssertCount(string name, float expected)
        {
            Assert.That(RuntimeManager.StudioSystem.getParameterByName(name, out float value, out _), Is.EqualTo(FMOD.RESULT.OK));
            Assert.That(value, Is.EqualTo(expected), name);
        }

        [UnityTest]
        public IEnumerator EnemyHitAndKilledHooksFollowDamageSemanticsInEveryLevel()
        {
            LogAssert.ignoreFailingMessages = true;
            foreach (string sceneName in new[] { "Level01", "Level02", "Level03" })
            {
                SceneManager.LoadScene(sceneName);
                yield return null;

                GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
                Transform enemySources = authoring.transform.Find("Enemy Damage and Death");
                FMODAudioBridge bridge = authoring.GetComponent<FMODAudioBridge>();
                TowerDefenseGame game = TowerDefenseGame.Instance;

                AssertEnemySources(enemySources, sceneName);
                Assert.That(bridge.AuthoringEmitterPlayCount, Is.Zero, sceneName);

                foreach (EnemyType type in new[] { EnemyType.GoblinRaider, EnemyType.GoblinBrute, EnemyType.RuinKnight })
                {
                    string source = EnemySourceName(type);
                    EnemyUnit enemy = CreateEnemy(type, source + " Damage Test Enemy");
                    int playsBefore = bridge.AuthoringEmitterPlayCount;
                    enemy.ApplyDamage(0f);
                    Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(playsBefore), sceneName);
                    enemy.ApplyDamage(1f);
                    Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(playsBefore + 1), sceneName);
                    Assert.That(bridge.LastPlayedAuthoringEmitter, Is.EqualTo($"Enemy Damage and Death/{source}/Hit"), sceneName);
                    enemy.ApplyDamage(1000f);
                    Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(playsBefore + 3), sceneName);
                    Assert.That(bridge.LastPlayedAuthoringEmitter, Is.EqualTo($"Enemy Damage and Death/{source}/Death"), sceneName);
                    enemy.ApplyDamage(1f);
                    Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(playsBefore + 3), sceneName);

                    EnemyUnit escapedEnemy = CreateEnemy(type, source + " Escaped Enemy");
                    game.ResolveEnemy(escapedEnemy, false, 0);
                    Assert.That(bridge.LastPlayedAuthoringEmitter, Is.EqualTo("Player Life Lost"), sceneName);

                    EnemyUnit terminalEnemy = CreateEnemy(type, source + " Terminal Enemy");
                    int playsBeforeTerminalStop = bridge.AuthoringEmitterPlayCount;
                    terminalEnemy.EnterTerminalState();
                    terminalEnemy.ApplyDamage(1000f);
                    Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(playsBeforeTerminalStop), sceneName);
                }
            }
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator EnemyMovementUsesOneTypedPersistentInstanceAndStopsSafelyInEveryLevel()
        {
            LogAssert.ignoreFailingMessages = true;
            foreach (string sceneName in new[] { "Level01", "Level02", "Level03" })
            {
                SceneManager.LoadScene(sceneName);
                yield return null;
                GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
                Transform movementSources = authoring.transform.Find("Enemy Movement");
                FMODAudioBridge bridge = authoring.GetComponent<FMODAudioBridge>();

                Assert.That(movementSources, Is.Not.Null, sceneName);
                Assert.That(movementSources.childCount, Is.EqualTo(3), sceneName);

                foreach (EnemyType type in new[] { EnemyType.GoblinRaider, EnemyType.GoblinBrute, EnemyType.RuinKnight })
                {
                    StudioEventEmitter source = movementSources
                        .Find(EnemyMovementSourceName(type))
                        ?.GetComponent<StudioEventEmitter>();
                    Assert.That(source, Is.Not.Null, sceneName);
                    Assert.That(source.EventReference.IsNull, Is.False, sceneName);

                    EnemyUnit enemy = CreateEnemy(type, type + " Movement Test");
                    Assert.That(bridge.ActiveEnemyMovementCount, Is.EqualTo(1), sceneName);
                    Assert.That(bridge.LastStartedEnemyMovementEvent, Is.EqualTo(source.EventReference.ToString()), sceneName);
                    enemy.EnterTerminalState();
                    Assert.That(bridge.ActiveEnemyMovementCount, Is.Zero, sceneName);

                    enemy = CreateEnemy(type, type + " Death Movement Test");
                    enemy.ApplyDamage(1000f);
                    Assert.That(bridge.ActiveEnemyMovementCount, Is.Zero, sceneName);

                    enemy = CreateEnemy(type, type + " Endpoint Movement Test", new[] { Vector3.zero, Vector3.zero });
                    yield return null;
                    Assert.That(bridge.ActiveEnemyMovementCount, Is.Zero, sceneName);

                    enemy = CreateEnemy(type, type + " Destroy Movement Test");
                    Object.Destroy(enemy.gameObject);
                    yield return null;
                    Assert.That(bridge.ActiveEnemyMovementCount, Is.Zero, sceneName);
                }
            }
            LogAssert.ignoreFailingMessages = false;
        }

        private static string EnemyMovementSourceName(EnemyType type) => type switch
        {
            EnemyType.GoblinRaider => "Goblin Raider Movement",
            EnemyType.GoblinBrute => "Goblin Brute Movement",
            EnemyType.RuinKnight => "Ruin Knight Movement",
            _ => throw new System.ArgumentOutOfRangeException(nameof(type))
        };

        private static void AssertEnemySources(Transform parent, string sceneName)
        {
            Assert.That(parent.Find("Enemy Hit"), Is.Null, sceneName);
            Assert.That(parent.Find("Enemy Killed"), Is.Null, sceneName);
            foreach (string enemyName in new[] { "Goblin Raider", "Goblin Brute", "Ruin Knight" })
            {
                Transform enemy = parent.Find(enemyName);
                Assert.That(enemy, Is.Not.Null, sceneName);
                foreach (string action in new[] { "Hit", "Death" })
                {
                    Transform source = enemy.Find(action);
                    Assert.That(source, Is.Not.Null, sceneName);
                    Assert.That(source.GetComponents<StudioEventEmitter>(), Has.Length.EqualTo(1), sceneName);
                }
            }
        }

        private static string EnemySourceName(EnemyType type) => type switch
        {
            EnemyType.GoblinRaider => "Goblin Raider",
            EnemyType.GoblinBrute => "Goblin Brute",
            EnemyType.RuinKnight => "Ruin Knight",
            _ => throw new System.ArgumentOutOfRangeException(nameof(type))
        };

        private static EnemyUnit CreateEnemy(EnemyType type, string name, Vector3[] path = null)
        {
            GameObject enemyObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemyObject.name = name;
            EnemyUnit enemy = enemyObject.AddComponent<EnemyUnit>();
            enemy.Configure(type, path ?? new[] { Vector3.zero, Vector3.right }, 1f, 1f);
            return enemy;
        }

        [UnityTest]
        public IEnumerator PlayerLifeLossPlaysAuthoringEmitterOnceAndUpdatesLivesInEveryLevel()
        {
            LogAssert.ignoreFailingMessages = true;
            foreach (string sceneName in new[] { "Level01", "Level02", "Level03" })
            {
                SceneManager.LoadScene(sceneName);
                yield return null;

                GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
                StudioEventEmitter emitter = authoring.transform.Find("Player Life Lost").GetComponent<StudioEventEmitter>();
                FMODAudioBridge bridge = authoring.GetComponent<FMODAudioBridge>();
                TowerDefenseGame game = TowerDefenseGame.Instance;

                Assert.That(emitter, Is.Not.Null, sceneName);
                Assert.That(bridge.AuthoringEmitterPlayCount, Is.Zero, sceneName);
                Assert.That(game.Lives, Is.EqualTo(20), sceneName);

                EnemyUnit firstEnemy = new GameObject("Escaped Enemy 1").AddComponent<EnemyUnit>();
                game.ResolveEnemy(firstEnemy, false, 0);

                Assert.That(game.Lives, Is.EqualTo(19), sceneName);
                Assert.That(bridge.LastLivesValue, Is.EqualTo(19f), sceneName);
                Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(1), sceneName);
                Assert.That(bridge.LastPlayedAuthoringEmitter, Is.EqualTo("Player Life Lost"), sceneName);

                EnemyUnit secondEnemy = new GameObject("Escaped Enemy 2").AddComponent<EnemyUnit>();
                game.ResolveEnemy(secondEnemy, false, 0);

                Assert.That(game.Lives, Is.EqualTo(18), sceneName);
                Assert.That(bridge.LastLivesValue, Is.EqualTo(18f), sceneName);
                Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(2), sceneName);

                for (int loss = 3; loss <= 20; loss++)
                {
                    EnemyUnit escapedEnemy = new GameObject($"Escaped Enemy {loss}").AddComponent<EnemyUnit>();
                    game.ResolveEnemy(escapedEnemy, false, 0);
                }

                Assert.That(game.Lives, Is.Zero, sceneName);
                Assert.That(game.State, Is.EqualTo(GameState.Defeat), sceneName);
                Assert.That(bridge.LastLivesValue, Is.Zero, sceneName);
                Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(20), sceneName);
                Assert.That(bridge.LastTerminalState, Is.EqualTo("Defeat"), sceneName);
            }
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator SuccessfulTowerPlacementPlaysAuthoringEmitterOnceForEveryTowerAndLevel()
        {
            LogAssert.ignoreFailingMessages = true;
            foreach (string sceneName in new[] { "Level01", "Level02" })
            {
                SceneManager.LoadScene(sceneName);
                yield return null;

                GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
                StudioEventEmitter emitter = authoring.transform.Find("Tower Placed").GetComponent<StudioEventEmitter>();
                FMODAudioBridge bridge = authoring.GetComponent<FMODAudioBridge>();
                Assert.That(emitter, Is.Not.Null, sceneName);

                TowerDefenseGame game = TowerDefenseGame.Instance;
                game.AddGold(10000);
                BuildPlot[] plots = Object.FindObjectsByType<BuildPlot>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                TowerType[] towerTypes = { TowerType.Cannon, TowerType.Archer, TowerType.Mage, TowerType.Inferno };
                Assert.That(plots.Length, Is.GreaterThanOrEqualTo(towerTypes.Length), sceneName);

                for (int index = 0; index < towerTypes.Length; index++)
                {
                    int playsBeforePlacement = bridge.AuthoringEmitterPlayCount;
                    Assert.That(plots[index].TryBuild(towerTypes[index]), Is.True, $"{sceneName}: {towerTypes[index]}");
                    Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(playsBeforePlacement + 1),
                        $"{sceneName}: {towerTypes[index]}");
                    Assert.That(bridge.LastPlayedAuthoringEmitter, Is.EqualTo("Tower Placed"), sceneName);

                    int playsAfterPlacement = bridge.AuthoringEmitterPlayCount;
                    Assert.That(plots[index].TryBuild(towerTypes[index]), Is.False,
                        $"{sceneName}: occupied {towerTypes[index]} plot");
                    Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(playsAfterPlacement),
                        $"{sceneName}: failed {towerTypes[index]} placement");
                }
            }
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator TowerUpgradesPlayTheCorrectSuccessOrInsufficientGoldEmitterInEveryLevel()
        {
            LogAssert.ignoreFailingMessages = true;
            foreach (string sceneName in new[] { "Level01", "Level02", "Level03" })
            {
                SceneManager.LoadScene(sceneName);
                yield return null;

                GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
                StudioEventEmitter emitter = authoring.transform.Find("Tower Upgraded").GetComponent<StudioEventEmitter>();
                StudioEventEmitter failedEmitter = authoring.transform.Find("Tower Upgrade Failed").GetComponent<StudioEventEmitter>();
                FMODAudioBridge bridge = authoring.GetComponent<FMODAudioBridge>();
                TowerDefenseGame game = TowerDefenseGame.Instance;
                Assert.That(emitter, Is.Not.Null, sceneName);
                Assert.That(failedEmitter, Is.Not.Null, sceneName);
                Assert.That(failedEmitter.EventReference.IsNull, Is.False, sceneName);
                Assert.That(failedEmitter.EventReference.Path, Is.EqualTo("event:/UI/Tower_Upgrade_No"), sceneName);

                game.AddGold(10000);
                BuildPlot plot = Object.FindObjectsByType<BuildPlot>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)[0];
                Assert.That(plot.TryBuild(TowerType.Archer), Is.True, sceneName);
                Tower tower = plot.Tower;

                int playsBeforeUpgrade = bridge.AuthoringEmitterPlayCount;
                Assert.That(tower.TryUpgrade(), Is.True, sceneName);
                Assert.That(tower.Level, Is.EqualTo(2), sceneName);
                Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(playsBeforeUpgrade + 1), sceneName);
                Assert.That(bridge.LastPlayedAuthoringEmitter, Is.EqualTo("Tower Upgraded"), sceneName);

                Assert.That(tower.TryUpgrade(), Is.True, sceneName);
                Assert.That(tower.Level, Is.EqualTo(3), sceneName);
                Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(playsBeforeUpgrade + 2), sceneName);

                int playsAtMaximum = bridge.AuthoringEmitterPlayCount;
                Assert.That(tower.TryUpgrade(), Is.False, sceneName);
                Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(playsAtMaximum), sceneName);

                GameObject unaffordableObject = new GameObject("Unaffordable Upgrade Tower");
                Tower unaffordableTower = unaffordableObject.AddComponent<Tower>();
                unaffordableTower.Configure(TowerType.Archer);
                Assert.That(game.TrySpend(game.Gold), Is.True, sceneName);
                int playsWithoutGold = bridge.AuthoringEmitterPlayCount;
                int goldWithoutGold = game.Gold;
                int levelWithoutGold = unaffordableTower.Level;
                Assert.That(unaffordableTower.TryUpgrade(), Is.False, sceneName);
                Assert.That(unaffordableTower.Level, Is.EqualTo(levelWithoutGold), sceneName);
                Assert.That(game.Gold, Is.EqualTo(goldWithoutGold), sceneName);
                Assert.That(bridge.AuthoringEmitterPlayCount, Is.EqualTo(playsWithoutGold + 1), sceneName);
                Assert.That(bridge.LastPlayedAuthoringEmitter, Is.EqualTo("Tower Upgrade Failed"), sceneName);
                Object.Destroy(unaffordableObject);
            }
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator WaveCounterUsesAuthoritativeFmodLabels()
        {
            LogAssert.ignoreFailingMessages = true;
            SceneManager.LoadScene("Level01");
            yield return null;

            FMODAudioBridge bridge = GameObject.Find(StudentAuthoringTemplates.RootName).GetComponent<FMODAudioBridge>();
            string[] labels = { "Wave0", "Wave1", "Wave2", "Wave3", "Wave4", "Victory", "Defeat" };
            for (int wave = 0; wave < labels.Length; wave++)
            {
                GameplayAudioEvents.SetParameter("WaveCounter", wave);
                Assert.That(bridge.LastWaveCounterLabel, Is.EqualTo(labels[wave]));
            }

            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator SceneMusicAndAmbienceUseOfficialLoadLifecycleInEveryLevel()
        {
            LogAssert.ignoreFailingMessages = true;
            foreach (string sceneName in new[] { "Level01", "Level02", "Level03" })
            {
                SceneManager.LoadScene(sceneName);
                yield return null;

                GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
                StudioEventEmitter music = authoring.transform.Find("Level Music").GetComponent<StudioEventEmitter>();
                StudioEventEmitter ambience = authoring.transform.Find("Level Ambience").GetComponent<StudioEventEmitter>();
                Assert.That(music.EventPlayTrigger, Is.EqualTo(EmitterGameEvent.ObjectStart), sceneName);
                Assert.That(ambience.EventPlayTrigger, Is.EqualTo(EmitterGameEvent.ObjectStart), sceneName);
                Assert.That(music.EventReference.IsNull, Is.False, sceneName);
                Assert.That(ambience.EventReference.IsNull, Is.True,
                    $"{sceneName} ambience remains an intentionally unassigned student authoring surface.");
                Assert.That(authoring.transform.Find("UI Control Sources/Start Wave"), Is.Not.Null, sceneName);

                TowerDefenseGame.Instance.StartWave();
                FMODAudioBridge bridge = authoring.GetComponent<FMODAudioBridge>();
                Assert.That(bridge.LastWaveCounterLabel, Is.EqualTo("Wave1"), sceneName);

                GameplayAudioEvents.SetParameter("WaveCounter", 2);
                GameplayAudioEvents.SetParameter("WaveCounter", 3);
                GameplayAudioEvents.SetParameter("WaveCounter", 4);
                Assert.That(bridge.LastWaveCounterLabel, Is.EqualTo("Wave4"), sceneName);
            }
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator LivesPublishesInitialAndBoundedValues()
        {
            LogAssert.ignoreFailingMessages = true;
            SceneManager.LoadScene("Level01");
            yield return null;

            FMODAudioBridge bridge = GameObject.Find(StudentAuthoringTemplates.RootName).GetComponent<FMODAudioBridge>();
            Assert.That(bridge.LastLivesValue, Is.EqualTo(20f));
            GameplayAudioEvents.SetParameter("Lives", -1f);
            Assert.That(bridge.LastLivesValue, Is.EqualTo(0f));
            GameplayAudioEvents.SetParameter("Lives", 21f);
            Assert.That(bridge.LastLivesValue, Is.EqualTo(20f));
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator EveryTowerFireUsesOneSharedEmitterWithCanonicalTowerType()
        {
            LogAssert.ignoreFailingMessages = true;
            SceneManager.LoadScene("Level01");
            yield return null;

            GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
            StudioEventEmitter emitter = authoring.transform.Find("Tower Fire").GetComponent<StudioEventEmitter>();
            FMODAudioBridge bridge = authoring.GetComponent<FMODAudioBridge>();
            Assert.That(emitter, Is.Not.Null);
            Assert.That(emitter.EventReference.IsNull, Is.False);
            Assert.That(emitter.EventReference.Path, Is.EqualTo("event:/Towers/Tower_Fire"));

            GameObject idleTowerObject = new GameObject("No Target Tower");
            idleTowerObject.AddComponent<Tower>().Configure(TowerType.Archer);
            int firesWithNoTarget = bridge.TowerFireEmitterPlayCount;
            yield return null;
            Assert.That(bridge.TowerFireEmitterPlayCount, Is.EqualTo(firesWithNoTarget));
            Object.Destroy(idleTowerObject);

            MethodInfo fire = typeof(Tower).GetMethod("Fire", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(fire, Is.Not.Null);
            foreach ((TowerType type, int expectedValue) in new[]
                     {
                         (TowerType.Cannon, 0),
                         (TowerType.Archer, 1),
                         (TowerType.Inferno, 2),
                         (TowerType.Mage, 3)
                     })
            {
                GameObject towerObject = new GameObject(type + " Fire Test Tower");
                Tower tower = towerObject.AddComponent<Tower>();
                tower.Configure(type);
                towerObject.transform.position = new Vector3(expectedValue + 1f, 0f, 2f);

                GameObject enemyObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                enemyObject.name = type + " Fire Test Enemy";
                EnemyUnit enemy = enemyObject.AddComponent<EnemyUnit>();
                enemy.Configure(EnemyType.RuinKnight,
                    new[] { Vector3.zero, Vector3.forward * 100f }, 100f, 1f);

                int firesBefore = bridge.TowerFireEmitterPlayCount;
                fire.Invoke(tower, new object[] { enemy });
                Assert.That(bridge.TowerFireEmitterPlayCount, Is.EqualTo(firesBefore + 1), type.ToString());
                Assert.That(bridge.LastTowerFireTypeValue, Is.EqualTo(expectedValue), type.ToString());
                Assert.That(bridge.LastTowerFireTypeLabel, Is.EqualTo(type.ToString()), type.ToString());
                Assert.That(bridge.LastTowerFirePosition, Is.EqualTo(towerObject.transform.position), type.ToString());
                // Inferno can synchronously emit EnemyHit after its one fire
                // request; the dedicated fire counters/type/position above
                // verify this contract without conflating unrelated emitters.

                if (type == TowerType.Inferno)
                {
                    yield return new WaitForSeconds(0.5f);
                    Assert.That(bridge.TowerFireEmitterPlayCount, Is.EqualTo(firesBefore + 1),
                        "Inferno damage ticks must not emit extra Tower Fire requests");
                }

                Object.Destroy(towerObject);
                Object.Destroy(enemyObject);
            }
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator TerminalStatesUseWaveCounterAndStopPersistentTowerAudio()
        {
            LogAssert.ignoreFailingMessages = true;
            SceneManager.LoadScene("Level01");
            yield return null;

            GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
            foreach (string source in new[] { "UI Control Sources/Restart", "UI Control Sources/Next", "UI Control Sources/Levels", "UI Control Sources/Home" })
            {
                Assert.That(authoring.transform.Find(source).GetComponent<StudioEventEmitter>(), Is.Not.Null, source);
            }
            Assert.That(authoring.transform.Find("Victory").GetComponent<StudioEventEmitter>(), Is.Null);
            Assert.That(authoring.transform.Find("Defeat").GetComponent<StudioEventEmitter>(), Is.Null);

            TowerDefenseGame game = TowerDefenseGame.Instance;
            game.AddGold(1000);
            BuildPlot plot = Object.FindAnyObjectByType<BuildPlot>();
            Assert.That(plot.TryBuild(TowerType.Inferno), Is.True);
            MSP603FMODTowerParameters parameters = plot.Tower.GetComponent<MSP603FMODTowerParameters>();
            Assert.That(parameters, Is.Not.Null);

            game.TryEnterTerminalState(true);
            yield return null;
            Assert.That(parameters.TerminalStopRequested, Is.True);
            Assert.That(parameters.enabled, Is.False);
            Assert.That(authoring.GetComponent<FMODAudioBridge>().LastTerminalState, Is.EqualTo("Victory"));
            Assert.That(authoring.GetComponent<FMODAudioBridge>().LastWaveCounterLabel, Is.EqualTo("Victory"));
            Assert.That(authoring.GetComponent<FMODAudioBridge>().LastStoppedAuthoringEmitter, Is.EqualTo("Level Ambience"));
            foreach (string button in new[] { "Restart", "Next Level", "Main Menu" })
            {
                Assert.That(FindProductionObject(button).GetComponent<StudioEventEmitter>(), Is.Not.Null, button);
            }
            Time.timeScale = 1f;

            SceneManager.LoadScene("Level01");
            yield return null;
            FMODAudioBridge restartedBridge = GameObject.Find(StudentAuthoringTemplates.RootName).GetComponent<FMODAudioBridge>();
            Assert.That(restartedBridge.LastWaveCounterLabel, Is.EqualTo("Wave0"));
            LogAssert.ignoreFailingMessages = false;
        }

        private static void AssertSlider(string objectName, string parameterName, float testValue)
        {
            Slider slider = FindProductionObject(objectName).GetComponent<Slider>();
            MSP603FMODGlobalParameterSlider bridge = slider.GetComponent<MSP603FMODGlobalParameterSlider>();
            Assert.That(slider.minValue, Is.EqualTo(0f));
            Assert.That(slider.maxValue, Is.EqualTo(100f));
            Assert.That(slider.wholeNumbers, Is.False);
            Assert.That(bridge.GlobalParameterName, Is.EqualTo(parameterName));
            slider.value = testValue;
            Assert.That(bridge.LastAttemptedValue, Is.EqualTo(testValue).Within(.001f));
        }
    }
}

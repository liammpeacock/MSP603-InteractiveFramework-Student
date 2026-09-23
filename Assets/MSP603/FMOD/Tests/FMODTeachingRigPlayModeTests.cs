using System.Collections;
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
            Assert.That(authoring.GetComponentsInChildren<StudioEventEmitter>(true), Has.Length.EqualTo(2));
            Assert.That(authoring.transform.Find("Pause Menu Ambience"), Is.Null);
            Assert.That(Camera.main.GetComponent<StudioListener>(), Is.Not.Null);

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
            GameObject.Find("Settings").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Toggle musicToggle = GameObject.Find("MUSIC ON").GetComponent<Toggle>();
            Toggle sfxToggle = GameObject.Find("SFX ON").GetComponent<Toggle>();
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
        public IEnumerator EveryLevelProvidesOnePauseMenuAmbienceTeachingEmitter()
        {
            LogAssert.ignoreFailingMessages = true;
            foreach (string sceneName in new[] { "Level01", "Level02", "Level03" })
            {
                SceneManager.LoadScene(sceneName);
                yield return null;
                yield return null;

                GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
                Assert.That(authoring, Is.Not.Null, sceneName);
                Assert.That(authoring.GetComponent<FMODAudioBridge>(), Is.Not.Null, sceneName);
                Assert.That(authoring.GetComponentsInChildren<StudioEventEmitter>(true), Has.Length.EqualTo(21), sceneName);
                Transform pauseAmbience = authoring.transform.Find("Pause Menu Ambience");
                Assert.That(pauseAmbience, Is.Not.Null, sceneName);
                Assert.That(pauseAmbience.GetComponents<StudioEventEmitter>(), Has.Length.EqualTo(1), sceneName);
                StudioEventEmitter pauseEmitter = pauseAmbience.GetComponent<StudioEventEmitter>();
                Assert.That(pauseEmitter.EventReference.Path, Is.EqualTo("event:/UI/Pause_Menu_Ambience"), sceneName);
                FMOD.GUID resolvedGuid = RuntimeManager.PathToGUID("event:/UI/Pause_Menu_Ambience");
                Assert.That(pauseEmitter.EventReference.Guid.Data1, Is.EqualTo(resolvedGuid.Data1), sceneName);
                Assert.That(pauseEmitter.EventReference.Guid.Data2, Is.EqualTo(resolvedGuid.Data2), sceneName);
                Assert.That(pauseEmitter.EventReference.Guid.Data3, Is.EqualTo(resolvedGuid.Data3), sceneName);
                Assert.That(pauseEmitter.EventReference.Guid.Data4, Is.EqualTo(resolvedGuid.Data4), sceneName);
                Assert.That(authoring.GetComponentInChildren<StudioParameterTrigger>(true), Is.Not.Null, sceneName);
                Assert.That(authoring.GetComponentInChildren<StudioGlobalParameterTrigger>(true), Is.Not.Null, sceneName);
                Assert.That(Camera.main.GetComponent<StudioListener>(), Is.Not.Null, sceneName);
            }
            LogAssert.ignoreFailingMessages = false;
        }

        [UnityTest]
        public IEnumerator GameplayPausePublishesLabelsAndManagesUiAmbienceWithoutPausingBuses()
        {
            LogAssert.ignoreFailingMessages = true;
            SceneManager.LoadScene("Level01");
            yield return null;
            yield return null;

            TowerDefenseGame game = TowerDefenseGame.Instance;
            GameObject authoring = GameObject.Find(StudentAuthoringTemplates.RootName);
            FMODAudioBridge bridge = authoring.GetComponent<FMODAudioBridge>();
            StudioEventEmitter pauseAmbience = authoring.transform.Find("Pause Menu Ambience").GetComponent<StudioEventEmitter>();

            AssertPauseParameterContract();
            Assert.That(RuntimeManager.StudioSystem.getEvent("event:/UI/Pause_Menu_Ambience", out _),
                Is.EqualTo(FMOD.RESULT.OK));
            AssertBusPaused("bus:/Gameplay", false);
            AssertBusPaused("bus:/UI", false);
            Assert.That(bridge.LastPauseStateLabel, Is.EqualTo("Unpaused"));
            AssertPauseLabel("Unpaused");

            game.SetPaused(true);
            Assert.That(bridge.LastPauseStateLabel, Is.EqualTo("Paused"));
            AssertPauseLabel("Paused");
            Assert.That(bridge.PauseMenuAmbiencePlayCount, Is.EqualTo(1));
            Assert.That(bridge.PauseMenuAmbienceStopCount, Is.Zero);
            Assert.That(pauseAmbience.EventInstance.isValid(), Is.True);
            Assert.That(pauseAmbience.EventInstance.getPlaybackState(out FMOD.Studio.PLAYBACK_STATE playbackState),
                Is.EqualTo(FMOD.RESULT.OK));
            Assert.IsTrue(
                playbackState == FMOD.Studio.PLAYBACK_STATE.STARTING ||
                playbackState == FMOD.Studio.PLAYBACK_STATE.PLAYING);
            AssertBusPaused("bus:/Gameplay", false);
            AssertBusPaused("bus:/UI", false);

            game.SetPaused(true);
            Assert.That(bridge.PauseMenuAmbiencePlayCount, Is.EqualTo(1));

            game.SetPaused(false);
            Assert.That(bridge.LastPauseStateLabel, Is.EqualTo("Unpaused"));
            AssertPauseLabel("Unpaused");
            Assert.That(bridge.PauseMenuAmbienceStopCount, Is.EqualTo(1));
            AssertBusPaused("bus:/Gameplay", false);
            AssertBusPaused("bus:/UI", false);

            game.SetPaused(true);
            Assert.That(bridge.PauseMenuAmbiencePlayCount, Is.EqualTo(2));
            Assert.That(game.TryEnterTerminalState(true), Is.True);
            Assert.That(bridge.LastPauseStateLabel, Is.EqualTo("Unpaused"));
            Assert.That(bridge.PauseMenuAmbienceStopCount, Is.EqualTo(2));
            AssertBusPaused("bus:/Gameplay", false);

            SceneManager.LoadScene("Level02");
            yield return null;
            yield return null;
            FMODAudioBridge freshBridge = GameObject.Find(StudentAuthoringTemplates.RootName).GetComponent<FMODAudioBridge>();
            Assert.That(freshBridge.LastPauseStateLabel, Is.EqualTo("Unpaused"));
            AssertPauseLabel("Unpaused");

            TowerDefenseGame.Instance.SetPaused(true);
            Assert.That(TowerDefenseGame.Instance.TryEnterTerminalState(false), Is.True);
            Assert.That(freshBridge.LastPauseStateLabel, Is.EqualTo("Unpaused"));
            Assert.That(freshBridge.PauseMenuAmbienceStopCount, Is.EqualTo(1));
            Time.timeScale = 1f;
            LogAssert.ignoreFailingMessages = false;
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
                Assert.That(failedEmitter.EventReference.IsNull, Is.True, sceneName);

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

                GameObject enemyObject = new GameObject(type + " Fire Test Enemy");
                EnemyUnit enemy = enemyObject.AddComponent<EnemyUnit>();
                enemy.Configure(EnemyType.RuinKnight,
                    new[] { Vector3.zero, Vector3.forward * 100f }, 100f, 1f);

                int firesBefore = bridge.TowerFireEmitterPlayCount;
                fire.Invoke(tower, new object[] { enemy });
                Assert.That(bridge.TowerFireEmitterPlayCount, Is.EqualTo(firesBefore + 1), type.ToString());
                Assert.That(bridge.LastTowerFireTypeValue, Is.EqualTo(expectedValue), type.ToString());
                Assert.That(bridge.LastTowerFireTypeLabel, Is.EqualTo(type.ToString()), type.ToString());
                Assert.That(bridge.LastTowerFirePosition, Is.EqualTo(towerObject.transform.position), type.ToString());
                Assert.That(bridge.LastPlayedAuthoringEmitter, Is.EqualTo("Tower Fire"), type.ToString());

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
                Assert.That(GameObject.Find(button).GetComponent<StudioEventEmitter>(), Is.Not.Null, button);
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
            Slider slider = GameObject.Find(objectName).GetComponent<Slider>();
            MSP603FMODGlobalParameterSlider bridge = slider.GetComponent<MSP603FMODGlobalParameterSlider>();
            Assert.That(slider.minValue, Is.EqualTo(0f));
            Assert.That(slider.maxValue, Is.EqualTo(100f));
            Assert.That(slider.wholeNumbers, Is.False);
            Assert.That(bridge.GlobalParameterName, Is.EqualTo(parameterName));
            slider.value = testValue;
            Assert.That(bridge.LastAttemptedValue, Is.EqualTo(testValue).Within(.001f));
        }

        private static void AssertBusPaused(string path, bool expected)
        {
            Assert.That(RuntimeManager.StudioSystem.getBus(path, out FMOD.Studio.Bus bus), Is.EqualTo(FMOD.RESULT.OK), path);
            Assert.That(bus.getPaused(out bool paused), Is.EqualTo(FMOD.RESULT.OK), path);
            Assert.That(paused, Is.EqualTo(expected), path);
        }

        private static void AssertPauseParameterContract()
        {
            Assert.That(RuntimeManager.StudioSystem.getParameterDescriptionByName("Paused", out FMOD.Studio.PARAMETER_DESCRIPTION parameter),
                Is.EqualTo(FMOD.RESULT.OK));
            Assert.That(parameter.flags.HasFlag(FMOD.Studio.PARAMETER_FLAGS.GLOBAL), Is.True);
            Assert.That(parameter.flags.HasFlag(FMOD.Studio.PARAMETER_FLAGS.LABELED), Is.True);

            string[] labels = new string[Mathf.RoundToInt(parameter.maximum - parameter.minimum)];
            for (int i = 0; i < labels.Length; i++)
            {
                Assert.That(RuntimeManager.StudioSystem.getParameterLabelByName("Paused", i, out labels[i]),
                    Is.EqualTo(FMOD.RESULT.OK));
            }
            CollectionAssert.AreEquivalent(new[] { "Paused", "Unpaused" }, labels);
            int unpausedIndex = System.Array.IndexOf(labels, "Unpaused");
            Assert.That(parameter.defaultvalue, Is.EqualTo(parameter.minimum + unpausedIndex));
        }

        private static void AssertPauseLabel(string expectedLabel)
        {
            Assert.That(RuntimeManager.StudioSystem.getParameterDescriptionByName("Paused", out FMOD.Studio.PARAMETER_DESCRIPTION parameter),
                Is.EqualTo(FMOD.RESULT.OK));
            int labelCount = Mathf.RoundToInt(parameter.maximum - parameter.minimum);
            int expectedIndex = -1;
            for (int i = 0; i < labelCount; i++)
            {
                Assert.That(RuntimeManager.StudioSystem.getParameterLabelByName("Paused", i, out string label),
                    Is.EqualTo(FMOD.RESULT.OK));
                if (label == expectedLabel) expectedIndex = i;
            }
            Assert.That(expectedIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(RuntimeManager.StudioSystem.getParameterByName("Paused", out _, out float value),
                Is.EqualTo(FMOD.RESULT.OK));
            Assert.That(value, Is.EqualTo(parameter.minimum + expectedIndex));
        }
    }
}

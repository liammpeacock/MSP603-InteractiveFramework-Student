using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MSP603.TowerDefense.Tests
{
    public sealed class VerticalSlicePlayModeTests
    {
        [UnityTest]
        public IEnumerator RuntimeUiUsesOneProjectConfiguredInputSystemModuleAcrossScenes()
        {
            foreach (string sceneName in new[] { "MainMenu", "Level01", "MainMenu" })
            {
                SceneManager.LoadScene(sceneName);
                yield return null;

                EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                InputSystemUIInputModule[] inputModules = Object.FindObjectsByType<InputSystemUIInputModule>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                StandaloneInputModule[] legacyModules = Object.FindObjectsByType<StandaloneInputModule>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);

                Assert.That(eventSystems, Has.Length.EqualTo(1), sceneName);
                Assert.That(inputModules, Has.Length.EqualTo(1), sceneName);
                Assert.That(legacyModules, Is.Empty, sceneName);

                InputSystemUIInputModule module = inputModules[0];
                Assert.That(module.gameObject, Is.EqualTo(eventSystems[0].gameObject), sceneName);
                Assert.That(module.actionsAsset, Is.SameAs(InputSystem.actions), sceneName);
                AssertUiAction(module.point, "UI/Point", sceneName);
                AssertUiAction(module.leftClick, "UI/Click", sceneName);
                AssertUiAction(module.rightClick, "UI/RightClick", sceneName);
                AssertUiAction(module.middleClick, "UI/MiddleClick", sceneName);
                AssertUiAction(module.scrollWheel, "UI/ScrollWheel", sceneName);
                AssertUiAction(module.move, "UI/Navigate", sceneName);
                AssertUiAction(module.submit, "UI/Submit", sceneName);
                AssertUiAction(module.cancel, "UI/Cancel", sceneName);
                AssertUiAction(module.trackedDevicePosition, "UI/TrackedDevicePosition", sceneName);
                AssertUiAction(module.trackedDeviceOrientation, "UI/TrackedDeviceOrientation", sceneName);
            }
        }

        [UnityTest]
        public IEnumerator MainMenuSceneCreatesItsController()
        {
            SceneManager.LoadScene("MainMenu");
            yield return null;

            Assert.That(Object.FindAnyObjectByType<MainMenuController>(), Is.Not.Null);
            Button play = GameObject.Find("Play").GetComponent<Button>();
            Assert.That(play.targetGraphic.GetComponent<Image>().sprite, Is.Not.Null);
            Assert.That(play.targetGraphic.GetComponent<Image>().sprite.rect.height, Is.LessThan(700f));
            Assert.That(GameObject.Find("Report a Bug").GetComponent<Button>(), Is.Not.Null);
            Assert.That(StudentSupport.BugReportUrl, Does.StartWith("https://forms.cloud.microsoft/"));
        }

        [Test]
        public void ProductionUiSpritesUseVisibleArtworkBounds()
        {
            Sprite button = FantasyUI.Sprite("UI_Button_Large.png");
            Sprite hud = FantasyUI.Sprite("UI_HUD_Frame.png");
            Sprite slider = FantasyUI.Sprite("UI_Slider_Track");

            Assert.That(button, Is.Not.Null);
            Assert.That(hud, Is.Not.Null);
            Assert.That(slider, Is.Not.Null);
            Assert.That(button.rect.height, Is.LessThan(button.texture.height * .6f));
            Assert.That(hud.rect.height, Is.LessThan(hud.texture.height * .5f));
            Assert.That(slider.rect.height, Is.LessThan(slider.texture.height * .3f));
        }

        [Test]
        public void ProductionSliderHierarchyIsVisibleAndWired()
        {
            GameObject root = new("Slider Test Root", typeof(RectTransform));
            Slider slider = FantasyUI.Slider(root.transform, "Volume", .5f, _ => { });

            Assert.That(slider.fillRect, Is.Not.Null);
            Assert.That(slider.handleRect, Is.Not.Null);
            Assert.That(slider.transform.Find("Track").GetComponent<Image>().sprite, Is.Not.Null);
            Assert.That(slider.fillRect.GetComponent<Image>().sprite, Is.Not.Null);
            Assert.That(slider.handleRect.GetComponent<Image>().sprite, Is.Not.Null);
            Assert.That(slider.interactable, Is.True);
            Assert.That(slider.minValue, Is.EqualTo(0f));
            Assert.That(slider.maxValue, Is.EqualTo(100f));
            Assert.That(slider.wholeNumbers, Is.False);
            Assert.That(slider.GetComponent<Image>().raycastTarget, Is.True);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void ProductionToggleHasAnInteractiveRaycastTarget()
        {
            GameObject root = new("Toggle Test Root", typeof(RectTransform));
            Toggle toggle = FantasyUI.Toggle(root.transform, "Music", "MUSIC ON", true, _ => { });

            Assert.That(toggle.interactable, Is.True);
            Assert.That(toggle.GetComponent<Image>().raycastTarget, Is.True);
            Object.DestroyImmediate(root);
        }

        [UnityTest]
        public IEnumerator LevelCardsShareTheirUnlockedStateWithTheWholeCard()
        {
            PlayerPrefs.SetInt("MSP603.HighestUnlockedLevel", 2);
            MainMenuController.OpenLevelSelectOnLoad = true;
            SceneManager.LoadScene("MainMenu");
            yield return null;

            Assert.That(GameObject.Find("Level 1 Card").GetComponent<Button>().interactable, Is.True);
            Assert.That(GameObject.Find("Level 2 Card").GetComponent<Button>().interactable, Is.True);
            Assert.That(GameObject.Find("Level 3 Card").GetComponent<Button>().interactable, Is.False);
            Assert.That(GameObject.Find("Level 1 Card").GetComponent<RectTransform>().sizeDelta.x, Is.GreaterThanOrEqualTo(456f));
            PlayerPrefs.SetInt("MSP603.HighestUnlockedLevel", 1);
        }

        [UnityTest]
        public IEnumerator MainSettingsControlsUpdateTheSharedAudioState()
        {
            SceneManager.LoadScene("MainMenu");
            yield return null;
            GameObject.Find("Settings").GetComponent<Button>().onClick.Invoke();
            yield return null;

            Slider master = GameObject.Find("MASTER VOLUME Slider").GetComponent<Slider>();
            Toggle music = GameObject.Find("MUSIC ON").GetComponent<Toggle>();
            master.value = 37f;
            music.isOn = false;
            Assert.That(AudioControlState.MasterVolume, Is.EqualTo(.37f).Within(.001f));
            Assert.That(AudioControlState.MusicEnabled, Is.False);
            AudioControlState.SetMasterVolume(.8f);
            AudioControlState.SetMusicEnabled(true);
        }

        [UnityTest]
        public IEnumerator GameplayUiHasOneWaveControlAndReusableUnaffordableSelector()
        {
            SceneManager.LoadScene("Level01");
            yield return null;

            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int waveControls = 0;
            foreach (Button button in buttons) if (button.name == "Start Wave") waveControls++;
            Assert.That(waveControls, Is.EqualTo(1));

            TowerDefenseGame game = TowerDefenseGame.Instance;
            Assert.That(game.TrySpend(game.Gold), Is.True);
            BuildPlot plot = Object.FindAnyObjectByType<BuildPlot>();
            game.SelectBuildPlot(plot);
            yield return null;
            Assert.That(GameObject.Find("Archer").GetComponent<Button>().interactable, Is.False);
            foreach (string towerName in new[] { "Archer", "Mage", "Cannon", "Inferno" })
            {
                RectTransform slot = GameObject.Find(towerName).GetComponent<RectTransform>();
                RectTransform name = slot.Find("Tower Name").GetComponent<RectTransform>();
                RectTransform price = slot.Find("Tower Price").GetComponent<RectTransform>();
                AssertContained(slot, name);
                AssertContained(slot, price);
                Assert.That(price.anchorMin.y, Is.GreaterThanOrEqualTo(.24f));
                Assert.That(name.anchorMin.y, Is.GreaterThan(price.anchorMax.y));
            }
            RectTransform selectorPanel = GameObject.Find("Production Panel").GetComponent<RectTransform>();
            AssertContained(selectorPanel, GameObject.Find("Back").GetComponent<RectTransform>());
            game.AddGold(500);
            yield return null;
            Assert.That(GameObject.Find("Archer").GetComponent<Button>().interactable, Is.True);

            game.SelectBuildPlot(plot);
            yield return null;
            RectTransform[] transforms = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int selectors = 0;
            foreach (RectTransform transform in transforms) if (transform.name == "Tower Context") selectors++;
            Assert.That(selectors, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TowerSelectionBlocksWorldPointerInputUntilItsClosingFrameEnds()
        {
            SceneManager.LoadScene("Level01");
            yield return null;

            TowerDefenseGame game = TowerDefenseGame.Instance;
            BuildPlot plot = Object.FindAnyObjectByType<BuildPlot>();
            game.SelectBuildPlot(plot);

            Assert.That(game.IsTowerSelectionOpen, Is.True);
            Assert.That(game.IsWorldPointerInputBlocked, Is.True);

            GameObject.Find("Back").GetComponent<Button>().onClick.Invoke();
            Assert.That(game.IsTowerSelectionOpen, Is.False);
            Assert.That(game.IsWorldPointerInputBlocked, Is.True,
                "The click that closes tower selection must not reach a world object in the same frame.");

            yield return null;
            Assert.That(game.IsWorldPointerInputBlocked, Is.False);

            game.SelectBuildPlot(plot);
            GameplayFantasyUI ui = Object.FindAnyObjectByType<GameplayFantasyUI>();
            typeof(GameplayFantasyUI).GetMethod("HandleBack", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ui, null);
            Assert.That(game.IsTowerSelectionOpen, Is.False);
            yield return null;
            Assert.That(game.IsWorldPointerInputBlocked, Is.False);
        }

        [UnityTest]
        public IEnumerator GameplayHudAndPauseActionsRemainInsideTheirProductionFrames()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            Canvas.ForceUpdateCanvases();

            RectTransform hud = GameObject.Find("HUD Frame").GetComponent<RectTransform>();
            Assert.That(hud.anchorMin, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(hud.anchorMax, Is.EqualTo(new Vector2(0f, 1f)));
            Assert.That(hud.localScale.x, Is.EqualTo(.6f).Within(.001f));
            Assert.That(hud.rect.width * hud.localScale.x, Is.LessThan(600f));
            Assert.That(hud.rect.height * hud.localScale.y, Is.LessThan(200f));
            Assert.That(GameObject.Find("Icon_Heart Value").GetComponent<Text>(), Is.Not.Null);
            Assert.That(GameObject.Find("Icon_Gold Value").GetComponent<Text>(), Is.Not.Null);
            Assert.That(GameObject.Find("Icon_Star_Filled Value").GetComponent<Text>(), Is.Not.Null);
            Assert.That(GameObject.Find("Gameplay Status").GetComponent<Text>(), Is.Not.Null);
            AssertContained(hud, GameObject.Find("Icon_Heart").GetComponent<RectTransform>());
            AssertContained(hud, GameObject.Find("Icon_Gold").GetComponent<RectTransform>());
            AssertContained(hud, GameObject.Find("Icon_Star_Filled").GetComponent<RectTransform>());
            AssertContained(hud, GameObject.Find("Gameplay Status").GetComponent<RectTransform>());
            AssertContained(hud, GameObject.Find("Pause").GetComponent<RectTransform>());

            GameObject.Find("Pause").GetComponent<Button>().onClick.Invoke();
            Assert.That(TowerDefenseGame.Instance.IsPaused, Is.True);
            Canvas.ForceUpdateCanvases();
            RectTransform panel = GameObject.Find("Production Panel").GetComponent<RectTransform>();
            Assert.That(panel.sizeDelta, Is.EqualTo(new Vector2(700, 900)));
            Assert.That(panel.GetComponent<Image>().preserveAspect, Is.False);
            string[] actions = { "Resume", "Settings", "Restart", "Levels", "Home", "Report a Bug" };
            foreach (string action in actions) AssertContained(panel, GameObject.Find(action).GetComponent<RectTransform>());
            Assert.That(TowerDefenseGame.Instance.IsPaused, Is.True);

            GameObject.Find("Settings").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Canvas.ForceUpdateCanvases();
            panel = GameObject.Find("Production Panel").GetComponent<RectTransform>();
            string[] settingsControls =
            {
                "Icon_Volume", "MASTER", "MASTER Slider",
                "Icon_Music", "MUSIC", "MUSIC Slider",
                "Icon_SFX", "SFX", "SFX Slider",
                "MUSIC ON", "SFX ON", "Back"
            };
            foreach (string control in settingsControls) AssertContained(panel, GameObject.Find(control).GetComponent<RectTransform>());
            Assert.That(GameObject.Find("Back").GetComponent<Button>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator OccupiedTowerContextControlsStayInsideOneProductionPanel()
        {
            SceneManager.LoadScene("Level01");
            yield return null;

            TowerDefenseGame game = TowerDefenseGame.Instance;
            game.AddGold(1000);
            BuildPlot plot = Object.FindAnyObjectByType<BuildPlot>();
            Assert.That(plot.TryBuild(TowerType.Archer), Is.True);
            game.SelectBuildPlot(plot);
            yield return null;
            Canvas.ForceUpdateCanvases();

            RectTransform panel = GameObject.Find("Production Panel").GetComponent<RectTransform>();
            foreach (string control in new[] { "Tower Type", "Tower Tier", "Upgrade", "Sell", "Back" })
                AssertContained(panel, GameObject.Find(control).GetComponent<RectTransform>());
            foreach (string action in new[] { "Upgrade", "Sell", "Back" })
            {
                RectTransform button = GameObject.Find(action).GetComponent<RectTransform>();
                AssertContained(button, button.Find("Label").GetComponent<RectTransform>());
            }

            RectTransform[] transforms = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (string control in new[] { "Tower Type", "Tower Tier", "Upgrade", "Sell", "Back" })
            {
                int matches = 0;
                foreach (RectTransform transform in transforms) if (transform.name == control) matches++;
                Assert.That(matches, Is.EqualTo(1), $"Expected one occupied-context {control} control.");
            }

            GameObject.Find("Upgrade").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.That(plot.Tower.Level, Is.EqualTo(2));
            GameObject.Find("Sell").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.That(plot.IsOccupied, Is.False);
            Assert.That(plot.TryBuild(TowerType.Mage), Is.True);
            game.SelectBuildPlot(plot);
            yield return null;
            GameObject.Find("Back").GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.That(GameObject.Find("Tower Context"), Is.Null);
        }

        [UnityTest]
        public IEnumerator StartWaveButtonUsesTheExistingWaveCallback()
        {
            float observedWave = -1f;
            void Capture(string parameter, float value)
            {
                if (parameter == "WaveCounter") observedWave = value;
            }

            GameplayAudioEvents.ParameterChanged += Capture;
            SceneManager.LoadScene("Level01");
            yield return null;

            Assert.That(TowerDefenseGame.Instance.Wave, Is.EqualTo(0));
            Assert.That(observedWave, Is.EqualTo(0f));
            GameObject.Find("Start Wave").GetComponent<Button>().onClick.Invoke();
            Assert.That(TowerDefenseGame.Instance.Wave, Is.EqualTo(1));
            Assert.That(observedWave, Is.EqualTo(1f));
            GameplayAudioEvents.ParameterChanged -= Capture;
        }

        [Test]
        public void EveryLevelDefinesExactlyFourPlayableWaves()
        {
            Assert.That(LevelDefinition.FromScene("Level01").TotalWaves, Is.EqualTo(4));
            Assert.That(LevelDefinition.FromScene("Level02").TotalWaves, Is.EqualTo(4));
            Assert.That(LevelDefinition.FromScene("Level03").TotalWaves, Is.EqualTo(4));
        }

        [Test]
        public void RunStatisticsCountActualEnemyTypesAndResetPerInstance()
        {
            RunStatistics statistics = new();
            statistics.RecordEnemyDefeated(EnemyType.GoblinRaider);
            statistics.RecordEnemyDefeated(EnemyType.GoblinBrute);
            statistics.RecordEnemyDefeated(EnemyType.RuinKnight);
            statistics.RecordEnemyDefeated(EnemyType.RuinKnight);
            statistics.RecordWaveCompleted();

            Assert.That(statistics.TotalEnemiesDefeated, Is.EqualTo(4));
            Assert.That(statistics.GoblinRaidersDefeated, Is.EqualTo(1));
            Assert.That(statistics.GoblinBrutesDefeated, Is.EqualTo(1));
            Assert.That(statistics.RuinKnightsDefeated, Is.EqualTo(2));
            Assert.That(statistics.WavesCompleted, Is.EqualTo(1));
            Assert.That(new RunStatistics().TotalEnemiesDefeated, Is.Zero);
        }

        [UnityTest]
        public IEnumerator VictoryIsIdempotentAndLeavesTowersIntactButInactive()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            TowerDefenseGame game = TowerDefenseGame.Instance;
            game.AddGold(1000);
            BuildPlot plot = Object.FindAnyObjectByType<BuildPlot>();
            Assert.That(plot.TryBuild(TowerType.Inferno), Is.True);
            Tower tower = plot.Tower;
            int victoryStates = 0;
            void Capture(string parameter, float value)
            {
                if (parameter == "WaveCounter" && value == (float)WaveMusicState.Victory) victoryStates++;
            }
            GameplayAudioEvents.ParameterChanged += Capture;

            Assert.That(game.TryEnterTerminalState(true), Is.True);
            Assert.That(game.TryEnterTerminalState(true), Is.False);
            Assert.That(game.State, Is.EqualTo(GameState.Victory));
            Assert.That(victoryStates, Is.EqualTo(1));
            Assert.That(tower.IsDestroyed, Is.False);
            Assert.That(tower.IsCombatEnabled, Is.False);
            Assert.That(game.IsWorldPointerInputBlocked, Is.True);
            int wave = game.Wave;
            game.StartWave();
            Assert.That(game.Wave, Is.EqualTo(wave));

            GameplayAudioEvents.ParameterChanged -= Capture;
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator DefeatIsIdempotentAndUsesDestroyedTowerLifecycle()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            TowerDefenseGame game = TowerDefenseGame.Instance;
            game.AddGold(1000);
            BuildPlot plot = Object.FindAnyObjectByType<BuildPlot>();
            Assert.That(plot.TryBuild(TowerType.Cannon), Is.True);
            Tower tower = plot.Tower;
            int destroyedCues = 0;
            int defeatStates = 0;
            void Capture(string id, Vector3 _) { if (id == "TowerDestroyed") destroyedCues++; }
            void CaptureState(string parameter, float value)
            {
                if (parameter == "WaveCounter" && value == (float)WaveMusicState.Defeat) defeatStates++;
            }
            GameplayAudioEvents.OneShotRequested += Capture;
            GameplayAudioEvents.ParameterChanged += CaptureState;

            Assert.That(game.TryEnterTerminalState(false), Is.True);
            Assert.That(game.TryEnterTerminalState(false), Is.False);
            Assert.That(game.State, Is.EqualTo(GameState.Defeat));
            Assert.That(tower.IsDestroyed, Is.True);
            Assert.That(tower.IsCombatEnabled, Is.False);
            Assert.That(destroyedCues, Is.EqualTo(1));
            Assert.That(defeatStates, Is.EqualTo(1));

            GameObject lateEnemyObject = new("Terminal cleanup enemy");
            EnemyUnit lateEnemy = lateEnemyObject.AddComponent<EnemyUnit>();
            game.ResolveEnemy(lateEnemy, true, 100);
            Assert.That(game.Statistics.TotalEnemiesDefeated, Is.Zero,
                "Terminal cleanup must not create artificial kills.");
            Object.Destroy(lateEnemyObject);
            GameplayAudioEvents.OneShotRequested -= Capture;
            GameplayAudioEvents.ParameterChanged -= CaptureState;
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator TerminalResultIsFullScreenAndSuppressesGameplayHud()
        {
            SceneManager.LoadScene("Level01");
            yield return null;
            TowerDefenseGame game = TowerDefenseGame.Instance;
            game.TryEnterTerminalState(true);
            yield return null;
            Canvas.ForceUpdateCanvases();

            RectTransform result = GameObject.Find("Victory").GetComponent<RectTransform>();
            Assert.That(result.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(result.anchorMax, Is.EqualTo(Vector2.one));
            Assert.That(result.GetComponent<Image>().raycastTarget, Is.True);
            Assert.That(GameObject.Find("HUD Frame"), Is.Null);
            Assert.That(GameObject.Find("Start Wave"), Is.Null);
            foreach (string control in new[] { "Restart", "Next Level", "Main Menu" })
            {
                RectTransform button = GameObject.Find(control).GetComponent<RectTransform>();
                AssertContained(GameObject.Find("Production Panel").GetComponent<RectTransform>(), button);
            }
            Assert.That(GameObject.Find("Restart").GetComponent<RectTransform>().anchorMin.y, Is.EqualTo(.41f));
            Assert.That(GameObject.Find("Next Level").GetComponent<RectTransform>().anchorMin.y, Is.EqualTo(.31f));
            Assert.That(GameObject.Find("Main Menu").GetComponent<RectTransform>().anchorMin.y, Is.EqualTo(.21f));
            Assert.That(GameObject.Find("Main Menu").GetComponent<RectTransform>().sizeDelta.y, Is.EqualTo(64f));
            Assert.That(GameObject.Find("Run Statistics").GetComponent<Text>().text, Does.Contain("Ruin Knights defeated: 0"));
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator FourthCompletedWaveEndsTheLevelWithoutStartingAFifth()
        {
            SceneManager.LoadScene("Level03");
            yield return null;

            TowerDefenseGame game = TowerDefenseGame.Instance;
            FieldInfo waveRunning = typeof(TowerDefenseGame).GetField("_waveRunning", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo completeWave = typeof(TowerDefenseGame).GetMethod("CompleteWave", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(waveRunning, Is.Not.Null);
            Assert.That(completeWave, Is.Not.Null);

            for (int wave = 1; wave <= 4; wave++)
            {
                game.StartWave();
                Assert.That(game.Wave, Is.EqualTo(wave));
                game.StopAllCoroutines();
                waveRunning.SetValue(game, false);
            }

            completeWave.Invoke(game, null);
            Assert.That(game.IsFinished, Is.True);
            game.StartWave();
            Assert.That(game.Wave, Is.EqualTo(4));
        }

        private static void AssertUiAction(InputActionReference reference, string expectedPath, string sceneName)
        {
            Assert.That(reference, Is.Not.Null, $"{sceneName}: {expectedPath}");
            Assert.That(reference.action, Is.SameAs(InputSystem.actions.FindAction(expectedPath, true)),
                $"{sceneName}: {expectedPath}");
            Assert.That(reference.action.enabled, Is.True, $"{sceneName}: {expectedPath}");
        }

        private static void AssertContained(RectTransform container, RectTransform child)
        {
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(container, child);
            Rect rect = container.rect;
            Assert.That(bounds.min.x, Is.GreaterThanOrEqualTo(rect.xMin - .1f), $"{child.name} crosses the left edge of {container.name}.");
            Assert.That(bounds.max.x, Is.LessThanOrEqualTo(rect.xMax + .1f), $"{child.name} crosses the right edge of {container.name}.");
            Assert.That(bounds.min.y, Is.GreaterThanOrEqualTo(rect.yMin - .1f), $"{child.name} crosses the bottom edge of {container.name}.");
            Assert.That(bounds.max.y, Is.LessThanOrEqualTo(rect.yMax + .1f), $"{child.name} crosses the top edge of {container.name}.");
        }

        [UnityTest]
        public IEnumerator PauseResumeButtonPreservesPauseStateCallback()
        {
            SceneManager.LoadScene("Level01");
            yield return null;

            GameObject.Find("Pause").GetComponent<Button>().onClick.Invoke();
            Assert.That(TowerDefenseGame.Instance.IsPaused, Is.True);
            GameObject.Find("Resume").GetComponent<Button>().onClick.Invoke();
            Assert.That(TowerDefenseGame.Instance.IsPaused, Is.False);
        }

        [UnityTest]
        public IEnumerator LevelSceneCreatesAPlayableBoard()
        {
            SceneManager.LoadScene("Level01");
            yield return null;

            Assert.That(TowerDefenseGame.Instance, Is.Not.Null);
            Assert.That(Object.FindObjectsByType<BuildPlot>(FindObjectsSortMode.None), Has.Length.EqualTo(5));
            foreach (BuildPlot plot in Object.FindObjectsByType<BuildPlot>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                Assert.That(plot.GetComponent<Renderer>(), Is.Null);
                Assert.That(plot.GetComponent<SphereCollider>(), Is.Not.Null);
            }
            Assert.That(GameObject.Find("Enemy Entrance"), Is.Null);
            Assert.That(Camera.main, Is.Not.Null);
            Assert.That(GameObject.Find("Keep"), Is.Not.Null);
        }

        [TestCase("Level01", 1, "HIGHLAND APPROACH", 4, 5)]
        [TestCase("Level02", 2, "RUINED CROSSING", 5, 3)]
        [TestCase("Level03", 3, "SHATTERED CITADEL", 6, 6)]
        public void LevelDefinitionsProvideDistinctProgression(string sceneName, int number, string name, int waves, int plots)
        {
            LevelDefinition level = LevelDefinition.FromScene(sceneName);
            Assert.That(level.Number, Is.EqualTo(number));
            Assert.That(level.Name, Is.EqualTo(name));
            Assert.That(level.TotalWaves, Is.EqualTo(waves));
            Assert.That(level.BuildPlots, Has.Length.EqualTo(plots));
            Assert.That(level.Path, Has.Length.GreaterThan(6));
        }

        [Test]
        public void TowerTypesHaveDistinctPositiveStats()
        {
            TowerStats archer = TowerCatalog.Get(TowerType.Archer);
            TowerStats mage = TowerCatalog.Get(TowerType.Mage);
            TowerStats cannon = TowerCatalog.Get(TowerType.Cannon);
            TowerStats inferno = TowerCatalog.Get(TowerType.Inferno);

            Assert.That(archer.Cost, Is.GreaterThan(0));
            Assert.That(mage.Damage, Is.GreaterThan(archer.Damage));
            Assert.That(cannon.SplashRadius, Is.GreaterThan(0f));
            Assert.That(inferno.Damage, Is.GreaterThan(cannon.Damage));
            Assert.That(inferno.ProjectileSpeed, Is.EqualTo(0f));
        }

        [TestCase(TowerType.Archer, 0.09f, 0.65f)]
        [TestCase(TowerType.Cannon, 0.34f, 0.34f)]
        [TestCase(TowerType.Mage, 0.24f, 0.24f)]
        public void ProjectileAppearanceMatchesItsTowerType(TowerType towerType, float expectedWidth, float expectedLength)
        {
            PrimitiveType shape = towerType == TowerType.Archer ? PrimitiveType.Cube : PrimitiveType.Sphere;
            GameObject projectileObject = GameObject.CreatePrimitive(shape);
            Projectile projectile = projectileObject.AddComponent<Projectile>();
            TowerStats stats = TowerCatalog.Get(towerType);

            projectile.Configure(null, stats.Damage, stats.ProjectileSpeed, stats.SplashRadius, towerType);

            Assert.That(projectile.SourceTowerType, Is.EqualTo(towerType));
            Assert.That(projectileObject.transform.localScale.x, Is.EqualTo(expectedWidth).Within(0.001f));
            Assert.That(projectileObject.transform.localScale.z, Is.EqualTo(expectedLength).Within(0.001f));
            Assert.That(projectileObject.GetComponent<Renderer>().enabled, Is.True);

            Object.DestroyImmediate(projectileObject);
        }

        [Test]
        public void ProductionTowerPresentationSpritesAreAvailable()
        {
            Assert.That(Resources.Load<Sprite>("Towers/Cannon/Idle/Cannon_Idle_1"), Is.Not.Null);
            Assert.That(Resources.Load<Sprite>("Towers/Archer/Idle/Archer_Idle_01"), Is.Not.Null);
            Assert.That(Resources.Load<Sprite>("Towers/Mage/Idle/Mage_ReturnToIdle"), Is.Not.Null);
        }

        [Test]
        public void EveryCannonAnimationFrameIsAvailable()
        {
            string[] paths =
            {
                "Towers/Cannon/Idle/Cannon_Idle_1",
                "Towers/Cannon/Idle/Cannon_Idle_2",
                "Towers/Cannon/Aim/Cannon_Aim_1",
                "Towers/Cannon/Fire/Cannon_Fire_1",
                "Towers/Cannon/Fire/Cannon_Fire_2",
                "Towers/Cannon/Recoil/Cannon_Recoil_1",
                "Towers/Cannon/Reload/Cannon_Reload_1",
                "Towers/Cannon/Reload/Cannon_Reload_2",
                "Towers/Cannon/Idle/Cannon_ReturnToIdle_1",
                "Towers/Cannon/Destroy/Cannon_Destroy_1",
                "Towers/Cannon/Destroy/Cannon_Destroy_2",
                "Towers/Cannon/Destroy/Cannon_Destroy_3"
            };

            foreach (string path in paths)
            {
                Assert.That(Resources.Load<Sprite>(path), Is.Not.Null, $"Missing Cannon sprite: {path}");
            }
        }

        [UnityTest]
        public IEnumerator DestroyedCannonStopsOperatingAndKeepsItsRuinedFrame()
        {
            GameObject towerObject = new GameObject("CannonTowerTest");
            Tower tower = towerObject.AddComponent<Tower>();
            tower.Configure(TowerType.Cannon);
            Sprite ruinedSprite = Resources.Load<Sprite>("Towers/Cannon/Destroy/Cannon_Destroy_3");

            tower.DestroyTower();
            yield return new WaitForSeconds(0.5f);

            SpriteRenderer renderer = towerObject.GetComponentInChildren<SpriteRenderer>();
            Assert.That(tower.IsDestroyed, Is.True);
            Assert.That(tower.TryUpgrade(), Is.False);
            Assert.That(renderer.sprite, Is.SameAs(ruinedSprite));

            Object.Destroy(towerObject);
        }

        [Test]
        public void AudioControlsPublishNormalizedGameplayParameters()
        {
            float observed = -1f;
            void Capture(string parameter, float value)
            {
                if (parameter == "MusicVolume") observed = value;
            }

            GameplayAudioEvents.ParameterChanged += Capture;
            AudioControlState.SetMusicVolume(0.42f);
            GameplayAudioEvents.ParameterChanged -= Capture;
            AudioControlState.SetMusicVolume(0.8f);

            Assert.That(observed, Is.EqualTo(0.42f).Within(0.001f));
        }
    }
}

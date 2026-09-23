using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MSP603.TowerDefense
{
    public sealed class GameplayFantasyUI : MonoBehaviour
    {
        private enum UiState { Gameplay, PauseRoot, PauseSettings, TowerContext, Result }

        private TowerDefenseGame _game;
        private Canvas _canvas;
        private Text _lives, _gold, _wave, _status;
        private GameObject _modal, _result;
        private GameObject _hud;
        private Button _waveButton;
        private Button[] _buildChoices;
        private int[] _buildChoiceCosts;
        private UiState _state = UiState.Gameplay;

        private void Awake()
        {
            _game = GetComponent<TowerDefenseGame>();
            _canvas = FantasyUI.Canvas("Gameplay UI Canvas");
            BuildHud();
            BuildTowerBar();
        }

        private void BuildHud()
        {
            RectTransform frame = FantasyUI.Image(_canvas.transform, "HUD Frame", "UI_HUD_Frame.png", Vector2.zero, Vector2.one);
            _hud = frame.gameObject;
            // UI_HUD_Frame's cropped artwork is approximately 3:1. Preserve that
            // proportion and populate its circular life well plus rectangular strip.
            // Keep the proven internal 3:1 layout and uniformly scale the complete HUD.
            // At the 1536x1024 reference this displays as 558x186 with a 12px safe margin.
            bool useLevelThreeLayout = SceneManager.GetActiveScene().name == "Level03";
            Vector2 hudAnchor = useLevelThreeLayout ? new Vector2(.5f, 1f) : new Vector2(0f, 1f);
            Vector2 hudPosition = useLevelThreeLayout ? new Vector2(0f, -105f) : new Vector2(291f, -105f);
            FantasyUI.Place(frame, hudAnchor, new Vector2(930, 310), hudPosition);
            frame.localScale = Vector3.one * .6f;
            AddHudItem(frame, "Icon_Heart", .035f, .13f, .12f, .22f, out _lives);
            AddHudItem(frame, "Icon_Gold", .285f, .36f, .35f, .46f, out _gold);
            AddHudItem(frame, "Icon_Star_Filled", .475f, .545f, .54f, .65f, out _wave);
            Button pause = FantasyUI.Button(frame, "Pause", "", "Icon_Pause", ShowPauseRoot);
            FantasyUI.Place(pause.GetComponent<RectTransform>(), new Vector2(.91f, .51f), new Vector2(82, 82), Vector2.zero);
            _status = FantasyUI.Label(frame, "Gameplay Status", "", 22);
            _status.rectTransform.anchorMin = new Vector2(.65f, .25f);
            _status.rectTransform.anchorMax = new Vector2(.85f, .76f);
        }

        private static void AddHudItem(Transform frame, string icon, float iconMin, float iconMax, float textMin, float textMax, out Text text)
        {
            FantasyUI.Image(frame, icon, icon, new Vector2(iconMin, .29f), new Vector2(iconMax, .71f));
            text = FantasyUI.Label(frame, icon + " Value", "0", 27, TextAnchor.MiddleLeft);
            text.rectTransform.anchorMin = new Vector2(textMin, .25f);
            text.rectTransform.anchorMax = new Vector2(textMax, .75f);
        }

        private void BuildTowerBar()
        {
            _waveButton = FantasyUI.Button(_canvas.transform, "Start Wave", "START WAVE", "Icon_Play", _game.StartWave);
            FantasyUI.Place(_waveButton.GetComponent<RectTransform>(), new Vector2(.5f, 0f), new Vector2(238, 68), new Vector2(0, 40));
        }

        private void Update()
        {
            _lives.text = _game.Lives.ToString();
            _gold.text = _game.Gold.ToString();
            _wave.text = $"{_game.Wave}/{_game.TotalWaves}";
            _status.text = $"LEVEL {_game.LevelNumber}: {_game.LevelName}";

            if (_buildChoices != null)
            {
                for (int i = 0; i < _buildChoices.Length; i++)
                    if (_buildChoices[i] != null) _buildChoices[i].interactable = _game.Gold >= _buildChoiceCosts[i];
            }

            if (Input.GetKeyDown(KeyCode.Escape)) HandleBack();
            if (_game.ResultVisible && _result == null) ShowResult();
        }

        private void HandleBack()
        {
            switch (_state)
            {
                case UiState.Gameplay: ShowPauseRoot(); break;
                case UiState.PauseRoot: ResumeGameplay(); break;
                case UiState.PauseSettings: ShowPauseRoot(); break;
                case UiState.TowerContext: CloseModal(); break;
            }
        }

        private void ShowPauseRoot()
        {
            if (_game.IsFinished) return;
            _game.SetPaused(true);
            ReplaceModal(PausePanel("Pause Menu", "GAME PAUSED"));
            _state = UiState.PauseRoot;
            AddPanelButton(_modal.transform, "Resume", "RESUME", "Icon_Play", .68f, ResumeGameplay);
            AddPanelButton(_modal.transform, "Settings", "SETTINGS", "Icon_Settings", .56f, ShowPauseSettings);
            AddPanelButton(_modal.transform, "Restart", "RESTART LEVEL", "Icon_Restart", .44f, _game.ReloadLevel);
            AddPanelButton(_modal.transform, "Levels", "LEVEL SELECT", "Icon_Back", .32f, TowerDefenseGame.LoadLevelSelect);
            AddPanelButton(_modal.transform, "Home", "MAIN MENU", "Icon_Home", .20f, TowerDefenseGame.LoadMenu);
            AddPanelButton(_modal.transform, "Report a Bug", "REPORT A BUG", null, .08f, StudentSupport.OpenBugReport);
        }

        private void ResumeGameplay()
        {
            CloseModal();
            _game.SetPaused(false);
            _state = UiState.Gameplay;
        }

        private void ShowPauseSettings()
        {
            ReplaceModal(PausePanel("Settings", "AUDIO SETTINGS"));
            _state = UiState.PauseSettings;
            AddSetting(_modal.transform, "MASTER", .69f, AudioControlState.MasterVolume, AudioControlState.SetMasterVolume, "Icon_Volume");
            AddSetting(_modal.transform, "MUSIC", .56f, AudioControlState.MusicVolume, AudioControlState.SetMusicVolume, "Icon_Music");
            AddSetting(_modal.transform, "SFX", .43f, AudioControlState.SfxVolume, AudioControlState.SetSfxVolume, "Icon_SFX");
            AddSettingToggle(_modal.transform, "MUSIC ON", .30f, AudioControlState.MusicEnabled, AudioControlState.SetMusicEnabled);
            AddSettingToggle(_modal.transform, "SFX ON", .235f, AudioControlState.SfxEnabled, AudioControlState.SetSfxEnabled);
            AddPanelButton(_modal.transform, "Back", "BACK TO PAUSE", "Icon_Back", .145f, ShowPauseRoot);
        }

        public void ShowBuildPlotContext(BuildPlot plot)
        {
            if (plot == null || _game.IsPaused || _game.IsFinished) return;
            bool isTowerSelection = !plot.IsOccupied;
            ReplaceModal(plot.IsOccupied
                ? Panel("Tower Context", "TOWER", false)
                : Panel("Tower Context", "BUILD TOWER", false, "UI_TowerSelection_Frame.png", new Vector2(1120, 450)));
            Button dismiss = _modal.AddComponent<Button>();
            dismiss.targetGraphic = _modal.GetComponent<Image>();
            dismiss.onClick.AddListener(CloseModal);
            _state = UiState.TowerContext;
            _game.SetTowerSelectionOpen(isTowerSelection);
            if (!plot.IsOccupied)
            {
                TowerType[] types = { TowerType.Archer, TowerType.Mage, TowerType.Cannon, TowerType.Inferno };
                string[] previews = { "Towers/Archer/Idle/Archer_Idle_01", "Towers/Mage/Idle/Mage_ReturnToIdle", "Towers/Cannon/Idle/Cannon_Idle_1", "Towers/Inferno/Master/Inferno_Master" };
                _buildChoices = new Button[types.Length];
                _buildChoiceCosts = new int[types.Length];
                for (int i = 0; i < types.Length; i++)
                {
                    TowerType type = types[i];
                    TowerStats stats = TowerCatalog.Get(type);
                    Button choice = FantasyUI.Button(PanelTransform(_modal), type.ToString(), stats.Name.ToUpperInvariant(), null,
                        () => { if (plot.TryBuild(type)) CloseModal(); });
                    AddRangePreview(choice, type);
                    FantasyUI.Place(choice.GetComponent<RectTransform>(), new Vector2(.11f + i * .19f, .47f), new Vector2(185, 285), Vector2.zero);
                    AddTowerPreview(choice.transform, previews[i]);
                    Text choiceLabel = choice.transform.Find("Label").GetComponent<Text>();
                    choiceLabel.name = "Tower Name";
                    choiceLabel.fontSize = 25; choiceLabel.resizeTextMaxSize = 25;
                    SetTowerChoiceTextRect(choiceLabel.rectTransform, .34f, .43f);
                    Text priceLabel = FantasyUI.Label(choice.transform, "Tower Price", $"{stats.Cost} GOLD", 22);
                    SetTowerChoiceTextRect(priceLabel.rectTransform, .24f, .33f);
                    choiceLabel.transform.SetAsLastSibling();
                    priceLabel.transform.SetAsLastSibling();
                    choice.interactable = _game.Gold >= stats.Cost;
                    _buildChoices[i] = choice;
                    _buildChoiceCosts[i] = stats.Cost;
                }
                Button close = FantasyUI.Button(PanelTransform(_modal), "Back", "CLOSE", "Icon_Back", CloseModal);
                FantasyUI.Place(close.GetComponent<RectTransform>(), new Vector2(.85f, .47f), new Vector2(230, 285), Vector2.zero);
            }
            else
            {
                Tower tower = plot.Tower;
                Transform panel = PanelTransform(_modal);
                Text towerType = FantasyUI.Label(panel, "Tower Type", TowerCatalog.Get(tower.Type).Name.ToUpperInvariant(), 28);
                towerType.rectTransform.anchorMin = new Vector2(.18f, .69f); towerType.rectTransform.anchorMax = new Vector2(.82f, .76f);
                Text towerTier = FantasyUI.Label(panel, "Tower Tier", $"TIER {tower.Level}", 25);
                towerTier.rectTransform.anchorMin = new Vector2(.18f, .62f); towerTier.rectTransform.anchorMax = new Vector2(.82f, .69f);
                string upgrade = tower.Level >= 3 ? "MAXIMUM TIER" : $"UPGRADE - {tower.UpgradeCost} GOLD";
                Button upgradeButton = AddTowerContextButton(_modal.transform, "Upgrade", upgrade, "Icon_Star_Filled", .54f, () => UpgradeTower(plot));
                upgradeButton.interactable = tower.Level < 3;
                int refund = Mathf.RoundToInt(tower.TotalGoldSpent * _game.TowerSellRefundPercentage);
                AddTowerContextButton(_modal.transform, "Sell", $"SELL - REFUND {refund} GOLD", "Icon_Gold", .40f, () => SellTower(plot));
                AddTowerContextButton(_modal.transform, "Back", "CLOSE", "Icon_Back", .21f, CloseModal);
            }
        }

        private static void AddTowerPreview(Transform parent, string resourcePath)
        {
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null) return;
            GameObject previewObject = new("Tower Preview", typeof(RectTransform), typeof(Image));
            previewObject.transform.SetParent(parent, false);
            Image preview = previewObject.GetComponent<Image>();
            preview.sprite = sprite; preview.preserveAspect = true; preview.raycastTarget = false;
            RectTransform rt = (RectTransform)previewObject.transform;
            rt.anchorMin = new Vector2(.12f, .44f); rt.anchorMax = new Vector2(.88f, .96f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private void AddRangePreview(Button choice, TowerType type)
        {
            EventTrigger trigger = choice.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry pointerEnter = new() { eventID = EventTriggerType.PointerEnter };
            pointerEnter.callback.AddListener(_ => _game.PreviewTowerRange(type));
            trigger.triggers.Add(pointerEnter);
        }

        private static void SetTowerChoiceTextRect(RectTransform text, float minY, float maxY)
        {
            text.anchorMin = new Vector2(.10f, minY);
            text.anchorMax = new Vector2(.90f, maxY);
            text.offsetMin = text.offsetMax = Vector2.zero;
        }

        private void UpgradeTower(BuildPlot plot)
        {
            Tower tower = plot.Tower;
            if (tower == null) { CloseModal(); return; }
            if (!tower.TryUpgrade()) _game.ShowNotice(tower.Level >= 3 ? "Tower is at maximum tier" : $"Need {tower.UpgradeCost} gold to upgrade");
            ShowBuildPlotContext(plot);
        }

        private void SellTower(BuildPlot plot)
        {
            int refund = plot.SellTower(_game.TowerSellRefundPercentage);
            _game.AddGold(refund);
            _game.ShowNotice($"Tower sold for {refund} gold");
            CloseModal();
        }

        private GameObject Panel(string name, string title, bool dimGameplay = true, string panelSprite = "UI_Panel_Large.png", Vector2 panelSize = default)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(_canvas.transform, false);
            RectTransform rr = (RectTransform)root.transform;
            rr.anchorMin = Vector2.zero; rr.anchorMax = Vector2.one; rr.offsetMin = rr.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0, 0, 0, dimGameplay ? .58f : .28f);
            RectTransform panel = FantasyUI.Image(root.transform, "Production Panel", panelSprite, Vector2.zero, Vector2.one);
            FantasyUI.Place(panel, new Vector2(.5f, .5f), panelSize == default ? new Vector2(650, 820) : panelSize, Vector2.zero);
            Text heading = FantasyUI.Label(panel, "Title", title, 42);
            heading.rectTransform.anchorMin = new Vector2(.12f, .82f); heading.rectTransform.anchorMax = new Vector2(.88f, .96f);
            return root;
        }

        private GameObject PausePanel(string name, string title)
        {
            GameObject root = Panel(name, title, true, "UI_Panel_Large.png", new Vector2(700, 900));
            PanelTransform(root).GetComponent<Image>().preserveAspect = false;
            return root;
        }

        private void ReplaceModal(GameObject modal)
        {
            if (_modal != null) Destroy(_modal);
            _buildChoices = null;
            _buildChoiceCosts = null;
            _modal = modal;
        }

        private void CloseModal()
        {
            _game.SetTowerSelectionOpen(false);
            if (_modal != null) Destroy(_modal);
            _modal = null;
            _buildChoices = null;
            _buildChoiceCosts = null;
            _game.ClearBuildPlotSelection();
            if (_state == UiState.TowerContext) _state = UiState.Gameplay;
        }

        private static Transform PanelTransform(GameObject root) => root.transform.Find("Production Panel");

        private static Button AddPanelButton(Transform root, string name, string label, string icon, float y, System.Action action)
        {
            Button button = FantasyUI.Button(PanelTransform(root.gameObject), name, label, icon, action);
            FantasyUI.Place(button.GetComponent<RectTransform>(), new Vector2(.5f, y), new Vector2(440, 76), Vector2.zero);
            return button;
        }

        private static Button AddTowerContextButton(Transform root, string name, string label, string icon, float y, System.Action action)
        {
            Button button = AddPanelButton(root, name, label, icon, y, action);
            FantasyUI.Place(button.GetComponent<RectTransform>(), new Vector2(.5f, y), new Vector2(500, 76), Vector2.zero);
            Text buttonLabel = button.transform.Find("Label").GetComponent<Text>();
            buttonLabel.resizeTextMinSize = 18;
            buttonLabel.resizeTextMaxSize = 27;
            buttonLabel.rectTransform.anchorMin = new Vector2(.25f, 0f);
            buttonLabel.rectTransform.anchorMax = new Vector2(.96f, 1f);
            buttonLabel.rectTransform.offsetMin = new Vector2(8, 8);
            buttonLabel.rectTransform.offsetMax = new Vector2(-8, -8);
            return button;
        }

        private static void AddSetting(Transform root, string label, float y, float value, System.Action<float> changed, string icon)
        {
            Transform panel = PanelTransform(root.gameObject);
            FantasyUI.Image(panel, icon, icon, new Vector2(.1f, y - .04f), new Vector2(.2f, y + .04f));
            Text text = FantasyUI.Label(panel, label, label, 23, TextAnchor.MiddleLeft);
            text.rectTransform.anchorMin = new Vector2(.22f, y - .06f); text.rectTransform.anchorMax = new Vector2(.45f, y + .06f);
            Slider slider = FantasyUI.Slider(panel, label + " Slider", value, changed);
            FantasyUI.Place(slider.GetComponent<RectTransform>(), new Vector2(.7f, y), new Vector2(280, 55), Vector2.zero);
        }

        private static void AddSettingToggle(Transform root, string label, float y, bool value, System.Action<bool> changed)
        {
            Toggle toggle = FantasyUI.Toggle(PanelTransform(root.gameObject), label, label, value, changed);
            FantasyUI.Place(toggle.GetComponent<RectTransform>(), new Vector2(.5f, y), new Vector2(300, 52), Vector2.zero);
        }

        private void ShowResult()
        {
            CloseModal();
            _state = UiState.Result;
            _hud.SetActive(false);
            _waveButton.gameObject.SetActive(false);
            _result = ResultPanel(_game.Victory ? "Victory" : "Defeat", _game.Victory ? "VICTORY" : "DEFEAT");
            Transform panel = PanelTransform(_result);
            RunStatistics stats = _game.Statistics;
            string lives = _game.Victory ? $"\nLives remaining: {_game.Lives}" : string.Empty;
            Text summary = FantasyUI.Label(panel, "Run Statistics",
                $"{_game.LevelName}\nWave reached: {_game.Wave}/{_game.TotalWaves}\nWaves completed: {stats.WavesCompleted}/{_game.TotalWaves}\n" +
                $"Enemies defeated: {stats.TotalEnemiesDefeated}\nGoblin Raiders defeated: {stats.GoblinRaidersDefeated}\n" +
                $"Goblin Brutes defeated: {stats.GoblinBrutesDefeated}\nRuin Knights defeated: {stats.RuinKnightsDefeated}{lives}", 27);
            summary.alignment = TextAnchor.MiddleLeft;
            summary.rectTransform.anchorMin = new Vector2(.16f, .45f); summary.rectTransform.anchorMax = new Vector2(.84f, .80f);
            AddResultButton("Restart", "Restart", "RESTART", "Icon_Restart", .41f, _game.ReloadLevel);
            if (_game.Victory && _game.LevelNumber < 3)
                AddResultButton("Next", "Next Level", "NEXT LEVEL", "Icon_Next", .31f, () => { Time.timeScale = 1; SceneManager.LoadScene($"Level{_game.LevelNumber + 1:00}"); });
            else if (!_game.Victory)
                AddResultButton("Levels", "Level Select", "LEVEL SELECT", "Icon_Back", .31f, TowerDefenseGame.LoadLevelSelect);
            AddResultButton("Home", "Main Menu", "MAIN MENU", "Icon_Home", _game.Victory && _game.LevelNumber == 3 ? .31f : .21f, TowerDefenseGame.LoadMenu);
        }

        private GameObject ResultPanel(string name, string title)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(_canvas.transform, false);
            RectTransform rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = Vector2.zero; rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
            Image blocker = root.GetComponent<Image>();
            blocker.color = new Color(.025f, .02f, .035f, .94f);
            blocker.raycastTarget = true;

            GameObject backdrop = new("Result Backdrop", typeof(RectTransform), typeof(RawImage));
            backdrop.transform.SetParent(root.transform, false);
            RectTransform backdropRect = (RectTransform)backdrop.transform;
            backdropRect.anchorMin = Vector2.zero; backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = backdropRect.offsetMax = Vector2.zero;
            RawImage backdropImage = backdrop.GetComponent<RawImage>();
            backdropImage.texture = Resources.Load<Texture2D>(_game.Victory ? "Presentation/Victory" : "Presentation/Lose");
            backdropImage.color = new Color(1f, 1f, 1f, .38f);
            backdropImage.raycastTarget = false;

            RectTransform panel = FantasyUI.Image(root.transform, "Production Panel", "UI_Panel_Large.png", Vector2.zero, Vector2.one);
            FantasyUI.Place(panel, new Vector2(.5f, .5f), new Vector2(780, 920), Vector2.zero);
            panel.GetComponent<Image>().preserveAspect = false;
            Text heading = FantasyUI.Label(panel, "Title", title, 54);
            heading.rectTransform.anchorMin = new Vector2(.12f, .76f); heading.rectTransform.anchorMax = new Vector2(.88f, .89f);
            return root;
        }

        private void AddResultButton(string sourceName, string hierarchyName, string label, string icon, float y, System.Action action)
        {
            Button button = AddPanelButton(_result.transform, sourceName, label, icon, y, action);
            button.gameObject.name = hierarchyName;
            FantasyUI.Place(button.GetComponent<RectTransform>(), new Vector2(.5f, y), new Vector2(500, 64), Vector2.zero);
        }
    }
}

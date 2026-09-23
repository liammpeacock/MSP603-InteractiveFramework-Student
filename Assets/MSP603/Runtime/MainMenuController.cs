using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MSP603.TowerDefense
{
    public sealed class MainMenuController : MonoBehaviour
    {
        public static bool OpenLevelSelectOnLoad { get; set; }
        [Header("Developer Testing")]
        [SerializeField] private bool _developerUnlockAllLevels;
        private Canvas _canvas;
        private GameObject _page;
        private enum MenuPage { Main, LevelSelect, Settings }
        private MenuPage _currentPage;

        private void Awake()
        {
            Time.timeScale = 1f;
            _canvas = FantasyUI.Canvas("Main Menu Canvas");
            if (OpenLevelSelectOnLoad) { OpenLevelSelectOnLoad = false; ShowLevelSelect(); }
            else ShowMain();
        }

        private void NewPage(string name, string background)
        {
            if (_page != null) Destroy(_page);
            _page = new GameObject(name, typeof(RectTransform)); _page.transform.SetParent(_canvas.transform, false);
            RectTransform rt = (RectTransform)_page.transform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            FantasyUI.Image(rt, "Non-interactive Background", background, Vector2.zero, Vector2.one).GetComponent<Image>().preserveAspect = false;
        }

        private void ShowMain()
        {
            _currentPage = MenuPage.Main;
            NewPage("Main Menu", "MainMenu_Background");
            AddButton("Play", "PLAY", "Icon_Play", .64f, () => LoadLevel(1));
            AddButton("Level Select", "LEVEL SELECT", "Icon_Home", .50f, ShowLevelSelect);
            AddButton("Settings", "SETTINGS", "Icon_Settings", .36f, ShowSettings);
            AddButton("Quit", "QUIT", "Icon_Quit", .22f, QuitApplication);
            AddButton("Report a Bug", "REPORT A BUG", null, .09f, StudentSupport.OpenBugReport);
        }

        private void AddButton(string name, string label, string icon, float y, UnityEngine.Events.UnityAction action)
        {
            Button button = FantasyUI.Button(_page.transform, name, label, icon, () => action());
            FantasyUI.Place(button.GetComponent<RectTransform>(), new Vector2(.23f, y), new Vector2(470, 105), Vector2.zero);
        }

        private void ShowLevelSelect()
        {
            _currentPage = MenuPage.LevelSelect;
            NewPage("Level Select", "LevelSelect_Background");
            Button back = FantasyUI.Button(_page.transform, "Back", "BACK", "Icon_Back", ShowMain);
            PlaceSecondaryBack(back);
            string[] names = { "HIGHLAND APPROACH", "RUINED CROSSING", "SHATTERED CITADEL" };
            for (int i = 0; i < 3; i++)
            {
                int level = i + 1; bool unlocked = _developerUnlockAllLevels || GameProgression.IsUnlocked(level);
                Button cardButton = FantasyUI.ButtonSurface(_page.transform, $"Level {level} Card", "UI_LevelCard_Frame", () => LoadLevel(level));
                RectTransform card = cardButton.GetComponent<RectTransform>();
                FantasyUI.Place(card, new Vector2(.18f + i * .32f, .47f), new Vector2(456, 500), Vector2.zero);
                Image cardImage = card.GetComponent<Image>();
                cardImage.raycastTarget = true;
                cardButton.interactable = unlocked;
                Text title = FantasyUI.Label(card, "Level Name", $"LEVEL {level}\n{names[i]}", 30); title.rectTransform.anchorMin = new Vector2(.08f, .54f); title.rectTransform.anchorMax = new Vector2(.92f, .91f);
                string icon = unlocked ? (GameProgression.IsCompleted(level) ? "Icon_Star_Filled" : "Icon_Play") : "Icon_Lock";
                FantasyUI.Image(card, "State Icon", icon, new Vector2(.36f, .32f), new Vector2(.64f, .54f));
                Button play = FantasyUI.Button(card, unlocked ? "Play" : "Locked", unlocked ? "PLAY" : "LOCKED", null, () => LoadLevel(level));
                FantasyUI.Place(play.GetComponent<RectTransform>(), new Vector2(.5f, .16f), new Vector2(245, 72), Vector2.zero); play.interactable = unlocked;
            }
        }

        private void ShowSettings()
        {
            _currentPage = MenuPage.Settings;
            NewPage("Settings", "MainMenu_Background");
            RectTransform panel = FantasyUI.Image(_page.transform, "Settings Panel", "UI_Panel_Large.png", Vector2.zero, Vector2.one, true);
            FantasyUI.Place(panel, new Vector2(.5f, .5f), new Vector2(850, 700), Vector2.zero);
            Text title = FantasyUI.Label(panel, "Title", "SETTINGS", 44); title.rectTransform.anchorMin = new Vector2(.2f, .82f); title.rectTransform.anchorMax = new Vector2(.8f, .96f);
            AddSlider(panel, "MASTER VOLUME", .69f, AudioControlState.MasterVolume, AudioControlState.SetMasterVolume, "Icon_Volume");
            AddSlider(panel, "MUSIC VOLUME", .55f, AudioControlState.MusicVolume, AudioControlState.SetMusicVolume, "Icon_Music");
            AddSlider(panel, "SFX VOLUME", .41f, AudioControlState.SfxVolume, AudioControlState.SetSfxVolume, "Icon_SFX");
            AddToggle(panel, "MUSIC ON", .285f, AudioControlState.MusicEnabled, AudioControlState.SetMusicEnabled);
            AddToggle(panel, "SFX ON", .205f, AudioControlState.SfxEnabled, AudioControlState.SetSfxEnabled);
            Button close = FantasyUI.Button(_page.transform, "Back", "BACK", "Icon_Back", ShowMain);
            PlaceSecondaryBack(close);
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(KeyCode.F8))
            {
                _developerUnlockAllLevels = !_developerUnlockAllLevels;
                Debug.Log($"Developer Unlock All Levels: {_developerUnlockAllLevels}", this);
                if (_currentPage == MenuPage.LevelSelect) ShowLevelSelect();
            }
#endif
            if (Input.GetKeyDown(KeyCode.Escape) && _currentPage != MenuPage.Main) ShowMain();
        }

        private static void AddToggle(Transform panel, string label, float y, bool value, System.Action<bool> changed)
        {
            Toggle toggle = FantasyUI.Toggle(panel, label, label, value, changed);
            FantasyUI.Place(toggle.GetComponent<RectTransform>(), new Vector2(.5f, y), new Vector2(300, 54), Vector2.zero);
        }

        private static void PlaceSecondaryBack(Button back)
        {
            FantasyUI.Place(back.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(210, 82), new Vector2(125, -62));
        }

        private static void QuitApplication()
        {
#if UNITY_EDITOR
            Debug.Log("QUIT invoked. Stopping Editor Play Mode.");
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void AddSlider(Transform panel, string label, float y, float value, System.Action<float> changed, string icon)
        {
            FantasyUI.Image(panel, icon, icon, new Vector2(.1f, y - .04f), new Vector2(.18f, y + .04f));
            Text text = FantasyUI.Label(panel, label, label, 24, TextAnchor.MiddleLeft); text.rectTransform.anchorMin = new Vector2(.2f, y - .06f); text.rectTransform.anchorMax = new Vector2(.48f, y + .06f);
            Slider slider = FantasyUI.Slider(panel, label + " Slider", value, changed); FantasyUI.Place(slider.GetComponent<RectTransform>(), new Vector2(.7f, y), new Vector2(330, 58), Vector2.zero);
        }

        private static void LoadLevel(int level) { GameplayAudioEvents.RequestOneShot("MenuConfirm", Vector3.zero); SceneManager.LoadScene($"Level{level:00}"); }
    }
}

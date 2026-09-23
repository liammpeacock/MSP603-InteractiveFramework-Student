using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace MSP603.TowerDefense
{
    public sealed class TowerDefenseGame : MonoBehaviour
    {
        private readonly List<EnemyUnit> _activeEnemies = new();
        private readonly List<List<Vector3>> _paths = new();
        private bool _waveRunning;
        private bool _spawningWave;
        private bool _paused;
        private int _wave;
        private int _gold;
        private int _lives = 20;
        private string _notice = "Choose a tower, then click a build plot";
        private float _noticeUntil = 6f;
        private GUIStyle _hudStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _bannerStyle;
        private LevelDefinition _level;
        private Texture2D _pauseBackground;
        private Texture2D _victoryBackground;
        private Texture2D _defeatBackground;
        private bool _showSettings;
        private bool _resultVisible;
        private float _gameSpeed = 1f;
        private int _spawnedEnemyCount;
        private bool _towerSelectionOpen;
        private bool _uiOwnsPointerInteraction;
        private int _worldPointerInputBlockedThroughFrame = -1;
        [SerializeField, Range(0f, 1f)] private float _towerSellRefundPercentage = 0.6f;

        public static TowerDefenseGame Instance { get; private set; }
        public IReadOnlyList<EnemyUnit> ActiveEnemies => _activeEnemies;
        public TowerType SelectedTower { get; private set; } = TowerType.Archer;
        public bool IsPaused => _paused;
        public GameState State { get; private set; } = GameState.Playing;
        public bool IsFinished => State != GameState.Playing;
        public int Gold => _gold;
        public int Lives => _lives;
        public int Wave => _wave;
        public int TotalWaves => _level.TotalWaves;
        public int LevelNumber => _level.Number;
        public string LevelName => _level.Name;
        public bool WaveRunning => _waveRunning;
        public bool ResultVisible => _resultVisible;
        public bool Victory => State == GameState.Victory;
        public RunStatistics Statistics { get; } = new();
        public float TowerSellRefundPercentage => _towerSellRefundPercentage;
        public float GetTowerRangeBonus(TowerType type) => _level.GetTowerRangeBonus(type);
        public float GetBaseTowerRange(TowerType type) => TowerCatalog.Get(type).Range + GetTowerRangeBonus(type);
        public BuildPlot SelectedBuildPlot { get; private set; }
        public Tower HoveredTower { get; private set; }
        public bool IsTowerSelectionOpen => _towerSelectionOpen;
        public bool IsWorldPointerInputBlocked
        {
            get
            {
                if (IsFinished || IsPaused || _towerSelectionOpen || _uiOwnsPointerInteraction ||
                    Time.frameCount <= _worldPointerInputBlockedThroughFrame)
                {
                    return true;
                }

                if (!IsPointerOverGameplayUi()) return false;

                // OnMouseDown is independent of EventSystem UI handling. Claim this
                // physical interaction for UI and retain that ownership until release.
                if (IsPointerPressed()) _uiOwnsPointerInteraction = true;
                return true;
            }
        }

        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
            _level = LevelDefinition.FromScene(SceneManager.GetActiveScene().name);
            _gold = _level.StartingGold;
            _pauseBackground = Resources.Load<Texture2D>("Presentation/Main_Menu_Pause");
            _victoryBackground = Resources.Load<Texture2D>("Presentation/Victory");
            _defeatBackground = Resources.Load<Texture2D>("Presentation/Lose");
            BuildWorld();
            gameObject.AddComponent<TowerRangeIndicator>();
            gameObject.AddComponent<GameplayFantasyUI>();
            GameplayAudioEvents.SetMusicState(_level.SceneName);
        }

        private void Start()
        {
            AudioControlState.PublishCurrentState();
            PublishMusicState(WaveMusicState.Ready);
            GameplayAudioEvents.SetParameter("Lives", _lives);
        }

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera != null && camera.orthographic)
            {
                camera.orthographicSize = Mathf.Max(7.1f, 10.8f / Mathf.Max(0.1f, camera.aspect));
            }
        }

        private void Update()
        {
            UpdatePointerInteractionOwnership();
            if (Input.GetKeyDown(KeyCode.F8))
            {
                AddGold(500);
                Debug.Log($"MSP603 QA: Added 500 Gold. Current Gold: {_gold}", this);
            }
            else if (Input.GetKeyDown(KeyCode.F9) && TryEnterTerminalState(true))
            {
                Debug.Log("MSP603 QA: Skipped to Victory through the normal terminal-state pathway.", this);
            }
            else if (Input.GetKeyDown(KeyCode.F10) && TryEnterTerminalState(false))
            {
                Debug.Log("MSP603 QA: Skipped to Defeat through the normal terminal-state pathway.", this);
            }
        }

        private void UpdatePointerInteractionOwnership()
        {
            if (IsPointerPressed() && IsPointerOverGameplayUi())
            {
                _uiOwnsPointerInteraction = true;
            }
            else if (_uiOwnsPointerInteraction && !IsPointerPressed())
            {
                _uiOwnsPointerInteraction = false;
            }
        }

        private static bool IsPointerPressed()
        {
            if (Input.GetMouseButton(0)) return true;
            for (int index = 0; index < Input.touchCount; index++)
            {
                TouchPhase phase = Input.GetTouch(index).phase;
                if (phase != TouchPhase.Ended && phase != TouchPhase.Canceled) return true;
            }
            return false;
        }

        private static bool IsPointerOverGameplayUi()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null) return false;
            if (eventSystem.IsPointerOverGameObject()) return true;

            for (int index = 0; index < Input.touchCount; index++)
                if (eventSystem.IsPointerOverGameObject(Input.GetTouch(index).fingerId)) return true;

            return false;
        }

        public bool TrySpend(int amount)
        {
            if (IsFinished || _gold < amount)
            {
                return false;
            }

            _gold -= amount;
            return true;
        }

        public void AddGold(int amount)
        {
            if (!IsFinished) _gold += Mathf.Max(0, amount);
        }

        public void SelectBuildPlot(BuildPlot plot)
        {
            if (IsFinished) return;
            HoveredTower = null;
            SelectedBuildPlot = plot;
            GetComponent<GameplayFantasyUI>()?.ShowBuildPlotContext(plot);
        }

        public void ClearBuildPlotSelection() => SelectedBuildPlot = null;

        public void SetHoveredTower(Tower tower)
        {
            if (tower != null && !IsWorldPointerInputBlocked) HoveredTower = tower;
        }

        public void ClearHoveredTower(Tower tower)
        {
            if (HoveredTower == tower) HoveredTower = null;
        }

        public void SetTowerSelectionOpen(bool isOpen)
        {
            _towerSelectionOpen = isOpen;
            if (!isOpen)
            {
                // Keep the world locked until this UI pointer event has fully finished.
                _worldPointerInputBlockedThroughFrame = Time.frameCount;
            }
        }

        public void ResolveEnemy(EnemyUnit enemy, bool defeated, int reward)
        {
            if (IsFinished) return;
            _activeEnemies.Remove(enemy);
            if (defeated)
            {
                _gold += reward;
                Statistics.RecordEnemyDefeated(enemy.Type);
            }
            else
            {
                _lives--;
                GameplayAudioEvents.RequestOneShot("PlayerLifeLost", enemy.transform.position);
                GameplayAudioEvents.SetParameter("Lives", _lives);
                if (_lives <= 0)
                {
                    Finish(false);
                }
            }

            if (_waveRunning && !_spawningWave && _activeEnemies.Count == 0)
            {
                CompleteWave();
            }
        }

        public void ShowNotice(string message)
        {
            _notice = message;
            _noticeUntil = Time.unscaledTime + 3f;
        }

        public static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        public void StartWave()
        {
            if (_waveRunning || IsFinished || _wave >= _level.TotalWaves)
            {
                return;
            }

            _wave++;
            _waveRunning = true;
            _spawningWave = true;
            GameplayAudioEvents.RequestOneShot("WaveStarted", Vector3.zero);
            PublishMusicState((WaveMusicState)_wave);
            StartCoroutine(SpawnWave(_wave));
        }

        private IEnumerator SpawnWave(int waveNumber)
        {
            int count = 5 + waveNumber * 3 + (_level.Number - 1) * 2;
            for (int index = 0; index < count; index++)
            {
                EnemyType type = EnemyType.GoblinBrute;
                if (waveNumber >= 2 && index % 3 == 1)
                {
                    type = EnemyType.GoblinRaider;
                }
                if (waveNumber >= 3 && index % 4 == 3)
                {
                    type = EnemyType.RuinKnight;
                }

                SpawnEnemy(type,
                    _level.HealthScale * (1f + 0.18f * (waveNumber - 1)),
                    _level.SpeedScale * (1f + 0.04f * (waveNumber - 1)));
                yield return new WaitForSeconds(Mathf.Max(0.45f, 1.05f - waveNumber * 0.12f));
            }

            _spawningWave = false;
            if (_activeEnemies.Count == 0)
            {
                CompleteWave();
            }
        }

        private void CompleteWave()
        {
            if (IsFinished) return;
            _waveRunning = false;
            Statistics.RecordWaveCompleted();
            GameplayAudioEvents.RequestOneShot("WaveEnded", Vector3.zero);
            if (_wave >= _level.TotalWaves)
            {
                Finish(true);
            }
            else
            {
                ShowNotice($"Wave {_wave} cleared - prepare your defences");
            }
        }

        private void SpawnEnemy(EnemyType type, float healthMultiplier, float speedMultiplier)
        {
            GameObject enemyObject = StudentAuthoringTemplates.InstantiatePrefab($"Enemies/{type}", type.ToString())
                ?? GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyObject.name = type.ToString();
            EnemyUnit enemy = enemyObject.GetComponent<EnemyUnit>() ?? enemyObject.AddComponent<EnemyUnit>();
            IReadOnlyList<Vector3> route = _paths[_spawnedEnemyCount % _paths.Count];
            _spawnedEnemyCount++;
            enemy.Configure(type, route, healthMultiplier, speedMultiplier);
            _activeEnemies.Add(enemy);
        }

        private void Finish(bool victory)
        {
            TryEnterTerminalState(victory);
        }

        public bool TryEnterTerminalState(bool victory)
        {
            if (IsFinished) return false;

            State = victory ? GameState.Victory : GameState.Defeat;
            _waveRunning = false;
            _spawningWave = false;
            _paused = false;
            GameplayAudioEvents.SetPauseState(false);
            StopAllCoroutines();
            _notice = victory ? "VICTORY - The keep stands!" : "DEFEAT - The keep has fallen";
            _noticeUntil = float.PositiveInfinity;

            SetTowerSelectionOpen(false);
            ClearBuildPlotSelection();
            HoveredTower = null;
            foreach (EnemyUnit enemy in FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None))
            {
                enemy.EnterTerminalState();
            }
            foreach (Projectile projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
            {
                Destroy(projectile.gameObject);
            }
            foreach (Tower tower in FindObjectsByType<Tower>(FindObjectsSortMode.None))
            {
                tower.EnterTerminalState(!victory);
            }

            PublishMusicState(victory ? WaveMusicState.Victory : WaveMusicState.Defeat);
            if (victory)
            {
                GameProgression.CompleteLevel(_level.Number);
            }
            _resultVisible = true;
            Time.timeScale = 0f;
            return true;
        }

        private static void PublishMusicState(WaveMusicState state)
        {
            GameplayAudioEvents.SetParameter("WaveCounter", (float)state);
        }

        public void TogglePause()
        {
            if (IsFinished)
            {
                return;
            }

            _paused = !_paused;
            _showSettings = false;
            Time.timeScale = _paused ? 0f : _gameSpeed;
            GameplayAudioEvents.SetPauseState(_paused);
        }

        public void SetPaused(bool paused)
        {
            if (IsFinished) return;
            if (_paused == paused) return;
            _paused = paused;
            Time.timeScale = paused ? 0f : _gameSpeed;
            GameplayAudioEvents.SetPauseState(_paused);
        }

        private void OnGUI()
        {
            return; // Production presentation is built from interactive uGUI components by GameplayFantasyUI.
#pragma warning disable CS0162
            EnsureStyles();
            float uiScale = Mathf.Min(1f, Screen.width / 900f, Screen.height / 560f);
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * uiScale);
            float viewWidth = Screen.width / uiScale;
            float viewHeight = Screen.height / uiScale;

            // The approved map concepts include illustrative HUD numbers. Cover that
            // strip so only live gameplay values are presented to the player.
            GUI.Box(new Rect(0f, 0f, viewWidth, 96f), GUIContent.none);
            GUI.Box(new Rect(14f, 14f, 485f, 76f), GUIContent.none);
            GUI.Label(new Rect(28f, 22f, 620f, 30f), $"LEVEL {_level.Number}: {_level.Name}    Lives: {_lives}    Gold: {_gold}    Wave: {_wave}/{_level.TotalWaves}", _hudStyle);
            GUI.Label(new Rect(28f, 54f, 455f, 25f), _waveRunning ? $"Enemies remaining: {_activeEnemies.Count}" : "Build phase", _hudStyle);
            if (GUI.Button(new Rect(viewWidth - 132f, 18f, 118f, 48f), _paused ? "RESUME [ESC]" : "PAUSE [ESC]", _buttonStyle))
            {
                TogglePause();
            }
            if (!_paused && !IsFinished && GUI.Button(new Rect(viewWidth - 258f, 18f, 118f, 48f), $"SPEED x{_gameSpeed:0}", _buttonStyle))
            {
                _gameSpeed = _gameSpeed < 2f ? 2f : 1f;
                Time.timeScale = _gameSpeed;
            }

            float toolbarWidth = Mathf.Min(780f, viewWidth - 28f);
            GUILayout.BeginArea(new Rect(14f, viewHeight - 92f, toolbarWidth, 78f), GUI.skin.box);
            GUILayout.BeginHorizontal();
            DrawTowerButton(TowerType.Archer);
            DrawTowerButton(TowerType.Mage);
            DrawTowerButton(TowerType.Cannon);
            DrawTowerButton(TowerType.Inferno);
            GUI.enabled = !_waveRunning && !IsFinished;
            if (GUILayout.Button(_wave == 0 ? "START WAVE 1" : "START NEXT WAVE", _buttonStyle, GUILayout.Height(54f)))
            {
                StartWave();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (Time.unscaledTime < _noticeUntil || IsFinished)
            {
                GUI.Box(new Rect(viewWidth * 0.5f - 260f, 110f, 520f, 62f), _notice, _bannerStyle);
            }

            if (_paused)
            {
                if (_pauseBackground != null) GUI.DrawTexture(new Rect(0f, 0f, viewWidth, viewHeight), _pauseBackground, ScaleMode.ScaleAndCrop);
                Rect overlay = new Rect(viewWidth * 0.5f - 230f, viewHeight * 0.5f - 240f, 460f, 480f);
                GUI.Box(overlay, GUIContent.none);
                GUILayout.BeginArea(new Rect(overlay.x + 34f, overlay.y + 24f, overlay.width - 68f, overlay.height - 48f));
                GUILayout.Label(_showSettings ? "SETTINGS" : "GAME PAUSED", _bannerStyle);
                GUILayout.Space(10f);
                if (_showSettings)
                {
                    DrawAudioSettings();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("BACK TO PAUSE", _buttonStyle, GUILayout.Height(42f))) _showSettings = false;
                    GUILayout.EndArea();
                    GUI.matrix = previousMatrix;
                    return;
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("RESUME", _buttonStyle, GUILayout.Height(42f)))
                {
                    TogglePause();
                }
                if (GUILayout.Button("RESTART LEVEL", _buttonStyle, GUILayout.Height(42f))) ReloadLevel();
                if (GUILayout.Button("LEVEL SELECT", _buttonStyle, GUILayout.Height(42f))) LoadLevelSelect();
                if (GUILayout.Button("SETTINGS", _buttonStyle, GUILayout.Height(42f))) _showSettings = true;
                if (GUILayout.Button("MAIN MENU", _buttonStyle, GUILayout.Height(42f))) LoadMenu();
                if (GUILayout.Button("QUIT TO DESKTOP", _buttonStyle, GUILayout.Height(42f))) Application.Quit();
                GUILayout.EndArea();
            }

            if (IsFinished && _resultVisible)
            {
                DrawResultScreen(viewWidth, viewHeight);
            }
            GUI.matrix = previousMatrix;
#pragma warning restore CS0162
        }

        private void DrawResultScreen(float viewWidth, float viewHeight)
        {
            bool victory = _lives > 0;
            Texture2D background = victory ? _victoryBackground : _defeatBackground;
            if (background != null) GUI.DrawTexture(new Rect(0f, 0f, viewWidth, viewHeight), background, ScaleMode.ScaleAndCrop);
            Rect facts = new Rect(viewWidth * 0.5f - 245f, viewHeight * 0.5f - 75f, 490f, 150f);
            GUI.Box(facts, GUIContent.none);
            GUI.Label(new Rect(facts.x + 20f, facts.y + 18f, facts.width - 40f, 45f), victory ? $"{_level.Name} DEFENDED" : "THE STRONGHOLD HAS FALLEN", _bannerStyle);
            GUI.Label(new Rect(facts.x + 20f, facts.y + 78f, facts.width - 40f, 35f), $"Wave: {_wave}/{_level.TotalWaves}    Lives: {_lives}    Gold: {_gold}", _hudStyle);
            GUILayout.BeginArea(new Rect(viewWidth * 0.5f - 220f, viewHeight - 120f, 440f, 105f));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(victory ? "REPLAY" : "RETRY LEVEL", _buttonStyle, GUILayout.Height(55f))) ReloadLevel();
            if (victory && _level.Number < 3 && GUILayout.Button("NEXT LEVEL", _buttonStyle, GUILayout.Height(55f)))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene($"Level{_level.Number + 1:00}");
            }
            if ((!victory || _level.Number == 3) && GUILayout.Button("LEVEL SELECT", _buttonStyle, GUILayout.Height(55f))) LoadLevelSelect();
            if (GUILayout.Button("MAIN MENU", _buttonStyle, GUILayout.Height(55f))) LoadMenu();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        public void ReloadLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(_level.SceneName);
        }

        public static void LoadMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public static void LoadLevelSelect()
        {
            MainMenuController.OpenLevelSelectOnLoad = true;
            LoadMenu();
        }

        private void DrawAudioSettings()
        {
            GUILayout.Label("AUDIO", _hudStyle);
            GUILayout.Label($"Master Volume  {Mathf.RoundToInt(AudioControlState.MasterVolume * 100f)}%", _hudStyle);
            float master = GUILayout.HorizontalSlider(AudioControlState.MasterVolume, 0f, 1f, GUILayout.Height(22f));
            if (!Mathf.Approximately(master, AudioControlState.MasterVolume)) AudioControlState.SetMasterVolume(master);

            GUILayout.Label($"Music Volume  {Mathf.RoundToInt(AudioControlState.MusicVolume * 100f)}%", _hudStyle);
            float music = GUILayout.HorizontalSlider(AudioControlState.MusicVolume, 0f, 1f, GUILayout.Height(22f));
            if (!Mathf.Approximately(music, AudioControlState.MusicVolume)) AudioControlState.SetMusicVolume(music);

            GUILayout.Label($"SFX Volume  {Mathf.RoundToInt(AudioControlState.SfxVolume * 100f)}%", _hudStyle);
            float sfx = GUILayout.HorizontalSlider(AudioControlState.SfxVolume, 0f, 1f, GUILayout.Height(22f));
            if (!Mathf.Approximately(sfx, AudioControlState.SfxVolume)) AudioControlState.SetSfxVolume(sfx);

            bool musicEnabled = GUILayout.Toggle(AudioControlState.MusicEnabled, " Music Enabled");
            if (musicEnabled != AudioControlState.MusicEnabled) AudioControlState.SetMusicEnabled(musicEnabled);
            bool sfxEnabled = GUILayout.Toggle(AudioControlState.SfxEnabled, " SFX Enabled");
            if (sfxEnabled != AudioControlState.SfxEnabled) AudioControlState.SetSfxEnabled(sfxEnabled);
        }

        private void DrawTowerButton(TowerType type)
        {
            TowerStats stats = TowerCatalog.Get(type);
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = SelectedTower == type ? new Color(0.96f, 0.76f, 0.25f) : Color.white;
            if (GUILayout.Button($"{stats.Name}\n{stats.Cost} gold", _buttonStyle, GUILayout.Height(54f), GUILayout.Width(115f)))
            {
                SelectedTower = type;
                ShowNotice($"{stats.Name} selected - click an empty build plot");
            }
            GUI.backgroundColor = previous;
        }

        public void SelectTower(TowerType type)
        {
            if (IsFinished) return;
            SelectedTower = type;
            ShowNotice($"{TowerCatalog.Get(type).Name} selected - click an empty build plot");
        }

        public void PreviewTowerRange(TowerType type)
        {
            if (!IsFinished) SelectedTower = type;
        }

        public void SetSpeed(float speed)
        {
            _gameSpeed = speed;
            if (!_paused && !IsFinished) Time.timeScale = speed;
        }

        private void EnsureStyles()
        {
            if (_hudStyle != null)
            {
                return;
            }

            _hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold, wordWrap = true };
            _bannerStyle = new GUIStyle(GUI.skin.box) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        }

        private void BuildWorld()
        {
            ConfigureCameraAndLight();
            CreateMapBackdrop();
            CreatePath();
            CreatePlots();
            CreateKeep();
        }

        private void CreateMapBackdrop()
        {
            Camera camera = Camera.main;
            Texture2D mapTexture = Resources.Load<Texture2D>(_level.MapResource);
            if (camera == null || mapTexture == null) return;
            GameObject backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backdrop.name = "WorldBackground";
            Destroy(backdrop.GetComponent<Collider>());
            GameObject backgroundAnchor = GameObject.Find("LevelEnvironment/Background");
            if (backgroundAnchor != null)
            {
                backdrop.transform.SetParent(backgroundAnchor.transform, true);
            }
            backdrop.transform.position = camera.transform.position + camera.transform.forward * 40f;
            backdrop.transform.rotation = camera.transform.rotation;
            float height = camera.orthographicSize * 2f;
            backdrop.transform.localScale = new Vector3(height * camera.aspect, height, 1f);
            Shader shader = Shader.Find("Unlit/Texture") ?? Shader.Find("Universal Render Pipeline/Unlit");
            Material material = new Material(shader) { mainTexture = mapTexture };
            backdrop.GetComponent<Renderer>().material = material;
        }

        private void ConfigureCameraAndLight()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }
            camera.orthographic = true;
            camera.orthographicSize = 8.2f;
            camera.transform.position = new Vector3(0f, 15f, -12f);
            camera.transform.LookAt(new Vector3(0f, 0f, 0f));
            camera.backgroundColor = new Color(0.32f, 0.48f, 0.56f);

            Light light = FindAnyObjectByType<Light>();
            if (light == null)
            {
                light = new GameObject("Sun").AddComponent<Light>();
            }
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        }

        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "LevelGround";
            ground.transform.localScale = new Vector3(2.25f, 1f, 1.45f);
            ground.GetComponent<Renderer>().material = CreateMaterial(new Color(0.30f, 0.50f, 0.24f));
        }

        private void CreatePath()
        {
            GameObject pathsRoot = GameObject.Find("EnemyPaths");
            if (pathsRoot == null)
            {
                _paths.Add(new List<Vector3>(_level.Path));
                return;
            }

            if (_level.Number == 2)
            {
                Transform shared = pathsRoot.transform.Find("Level2_SharedPath");
                AddAuthoredRoute(pathsRoot.transform.Find("Level2_LeftPath"), shared);
                AddAuthoredRoute(pathsRoot.transform.Find("Level2_RightPath"), shared);
            }
            else
            {
                foreach (Transform routeRoot in pathsRoot.transform)
                {
                    AddAuthoredRoute(routeRoot, null);
                }
            }

            if (_paths.Count == 0)
            {
                _paths.Add(new List<Vector3>(_level.Path));
                Debug.LogWarning($"{_level.SceneName} has no complete authored routes; using its legacy route.", this);
            }
        }

        private void CreatePlots()
        {
            GameObject plotsRoot = GameObject.Find("TowerBuildPoints");
            if (plotsRoot != null && plotsRoot.transform.childCount > 0)
            {
                foreach (Transform plotTransform in plotsRoot.transform)
                {
                    if (plotTransform.GetComponent<SphereCollider>() == null)
                    {
                        plotTransform.gameObject.AddComponent<SphereCollider>();
                    }
                    if (plotTransform.GetComponent<BuildPlot>() == null)
                    {
                        plotTransform.gameObject.AddComponent<BuildPlot>();
                    }
                }
                return;
            }

            foreach (Vector3 position in _level.BuildPlots)
            {
                GameObject plot = new("BuildPlot", typeof(SphereCollider), typeof(BuildPlot));
                plot.transform.position = position;
            }
        }

        private void CreateKeep()
        {
            // The completed environment contains the visible destination. This empty,
            // named anchor keeps that gameplay endpoint inspectable without covering art.
            GameObject keep = new("Keep");
            keep.transform.position = _paths[0][^1];
        }

        private void AddAuthoredRoute(Transform routeRoot, Transform sharedRoot)
        {
            if (routeRoot == null || routeRoot.childCount < 2)
            {
                return;
            }

            List<Vector3> route = new();
            foreach (Transform point in routeRoot)
            {
                route.Add(point.position);
            }
            if (sharedRoot != null)
            {
                foreach (Transform point in sharedRoot)
                {
                    if (route.Count == 0 || route[^1] != point.position)
                    {
                        route.Add(point.position);
                    }
                }
            }
            if (route.Count >= 2)
            {
                _paths.Add(route);
            }
        }


        private void CreateScenery()
        {
            Vector3[] positions = { new(-8f, 0f, 4f), new(-5.5f, 0f, 4.8f), new(-1.8f, 0f, 5.6f), new(8f, 0f, -4.5f), new(5.8f, 0f, -5.2f) };
            foreach (Vector3 position in positions)
            {
                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.name = "Tree";
                trunk.transform.position = position + Vector3.up * 0.65f;
                trunk.transform.localScale = new Vector3(0.22f, 0.65f, 0.22f);
                trunk.GetComponent<Renderer>().material = CreateMaterial(new Color(0.28f, 0.18f, 0.10f));
                Destroy(trunk.GetComponent<Collider>());
                GameObject crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                crown.transform.SetParent(trunk.transform, false);
                crown.transform.localPosition = new Vector3(0f, 1.25f, 0f);
                crown.transform.localScale = new Vector3(3.5f, 1.7f, 3.5f);
                crown.GetComponent<Renderer>().material = CreateMaterial(new Color(0.16f, 0.38f, 0.18f));
                Destroy(crown.GetComponent<Collider>());
            }
        }
    }
}

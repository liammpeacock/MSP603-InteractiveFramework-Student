using UnityEngine;

namespace MSP603.TowerDefense
{
    /// <summary>
    /// Shows the targeting footprint for the currently selected build plot.
    /// The ring is runtime-only and follows the same XZ plane used by Tower.FindTarget.
    /// </summary>
    public sealed class TowerRangeIndicator : MonoBehaviour
    {
        private const int SegmentCount = 96;
        private const float GroundOffset = 0.08f;

        private TowerDefenseGame _game;
        private LineRenderer _line;
        private Material _material;
        private BuildPlot _displayedPlot;
        private Tower _displayedTower;
        private TowerType _displayedType;
        private float _displayedRange = -1f;

        private void Awake()
        {
            _game = GetComponent<TowerDefenseGame>();
            CreateLine();
            Hide();
        }

        private void LateUpdate()
        {
            BuildPlot plot = _game != null ? _game.SelectedBuildPlot : null;
            Tower tower = plot != null ? plot.Tower : _game != null ? _game.HoveredTower : null;
            if (_game == null || _game.IsPaused || _game.IsFinished || (plot == null && tower == null))
            {
                Hide();
                return;
            }

            TowerType type = tower != null ? tower.Type : _game.SelectedTower;
            TowerStats stats = TowerCatalog.Get(type);
            float range = tower != null ? tower.EffectiveRange : _game.GetBaseTowerRange(type);
            Vector3 centre = plot != null ? plot.transform.position : tower.transform.position;

            if (!_line.enabled || plot != _displayedPlot || tower != _displayedTower ||
                type != _displayedType || !Mathf.Approximately(range, _displayedRange) ||
                _line.transform.position != centre + Vector3.up * GroundOffset)
            {
                Draw(plot, tower, type, range, stats.Color, centre);
            }
        }

        private void CreateLine()
        {
            GameObject ring = new("Tower Range Indicator");
            ring.transform.SetParent(transform, false);
            _line = ring.AddComponent<LineRenderer>();
            _line.useWorldSpace = false;
            _line.loop = true;
            _line.positionCount = SegmentCount;
            _line.widthMultiplier = 0.10f;
            _line.numCornerVertices = 2;
            _line.numCapVertices = 2;

            Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Unlit/Color");
            if (shader != null)
            {
                _material = new Material(shader) { name = "Tower Range Indicator Material" };
                _line.sharedMaterial = _material;
            }
        }

        private void Draw(BuildPlot plot, Tower tower, TowerType type, float range, Color towerColor, Vector3 centre)
        {
            _line.transform.position = centre + Vector3.up * GroundOffset;
            Color color = Color.Lerp(towerColor, Color.white, 0.35f);
            color.a = 0.95f;
            _line.startColor = color;
            _line.endColor = color;
            if (_material != null) _material.color = color;

            for (int i = 0; i < SegmentCount; i++)
            {
                float angle = i * Mathf.PI * 2f / SegmentCount;
                _line.SetPosition(i, new Vector3(Mathf.Cos(angle) * range, 0f, Mathf.Sin(angle) * range));
            }

            _displayedPlot = plot;
            _displayedTower = tower;
            _displayedType = type;
            _displayedRange = range;
            _line.enabled = true;
        }

        private void Hide()
        {
            if (_line != null) _line.enabled = false;
            _displayedPlot = null;
            _displayedTower = null;
            _displayedRange = -1f;
        }

        private void OnDestroy()
        {
            if (_material != null) Destroy(_material);
        }
    }
}

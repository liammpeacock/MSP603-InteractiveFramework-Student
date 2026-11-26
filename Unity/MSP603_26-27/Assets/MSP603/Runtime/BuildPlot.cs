using UnityEngine;

namespace MSP603.TowerDefense
{
    public sealed class BuildPlot : MonoBehaviour
    {
        private Tower _tower;

        public Tower Tower => _tower;
        public bool IsOccupied => _tower != null;

        private void Awake()
        {
            SphereCollider interaction = GetComponent<SphereCollider>();
            if (interaction != null)
            {
                interaction.center = new Vector3(0f, .45f, 0f);
                interaction.radius = 1.15f;
            }
        }

        private void OnMouseDown()
        {
            if (TowerDefenseGame.Instance == null || TowerDefenseGame.Instance.IsPaused || TowerDefenseGame.Instance.IsFinished ||
                TowerDefenseGame.Instance.IsWorldPointerInputBlocked)
            {
                return;
            }

            if (_tower != null)
            {
                TowerDefenseGame.Instance.SelectBuildPlot(this);
                return;
            }

            TowerDefenseGame.Instance.SelectBuildPlot(this);
        }

        private void OnMouseEnter()
        {
            if (_tower != null) TowerDefenseGame.Instance?.SetHoveredTower(_tower);
        }

        private void OnMouseExit()
        {
            TowerDefenseGame.Instance?.ClearHoveredTower(_tower);
        }

        public bool TryBuild(TowerType type)
        {
            if (_tower != null || TowerDefenseGame.Instance == null || TowerDefenseGame.Instance.IsFinished) return false;

            TowerStats stats = TowerCatalog.Get(type);
            if (!TowerDefenseGame.Instance.TrySpend(stats.Cost))
            {
                TowerDefenseGame.Instance.ShowNotice($"Need {stats.Cost} gold for {stats.Name}");
                return false;
            }

            GameObject towerObject = StudentAuthoringTemplates.InstantiatePrefab($"Towers/{stats.Name}Tower", $"{stats.Name}Tower")
                ?? new GameObject($"{stats.Name}Tower");
            towerObject.transform.position = transform.position;
            _tower = towerObject.GetComponent<Tower>() ?? towerObject.AddComponent<Tower>();
            _tower.Configure(type, TowerDefenseGame.Instance.GetTowerRangeBonus(type));
            GameplayAudioEvents.RequestOneShot("TowerBuilt", transform.position);
            if (type == TowerType.Inferno)
                GameplayAudioEvents.RequestOneShot("InfernoPlaced", transform.position);
            return true;
        }

        public int SellTower(float refundPercentage)
        {
            if (_tower == null || _tower.IsDestroyed || TowerDefenseGame.Instance == null || TowerDefenseGame.Instance.IsFinished) return 0;
            int refund = Mathf.RoundToInt(_tower.TotalGoldSpent * Mathf.Clamp01(refundPercentage));
            GameplayAudioEvents.RequestOneShot("TowerSold", transform.position);
            Destroy(_tower.gameObject);
            _tower = null;
            return refund;
        }
    }
}

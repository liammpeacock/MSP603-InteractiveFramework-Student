using UnityEngine;

namespace MSP603.TowerDefense
{
    /// <summary>
    /// Production presentation baselines plus in-memory, editor-authored offsets.
    /// Temporary values are never serialized and reset for every Play session.
    /// </summary>
    public static class TowerVisualCalibration
    {
        private static readonly Vector2[] Offsets = new Vector2[4];

        public static Vector2 GetPermanent(TowerType type)
        {
            return type switch
            {
                TowerType.Cannon => new Vector2(-0.2f, -1f),
                TowerType.Archer => new Vector2(0f, -0.5f),
                TowerType.Mage => new Vector2(0f, -1f),
                TowerType.Inferno => new Vector2(-0.05f, -0.5f),
                _ => Vector2.zero
            };
        }

        public static float GetPermanentScale(TowerType type)
        {
            return type == TowerType.Mage || type == TowerType.Inferno ? 0.75f : 1f;
        }

        public static Vector2 Get(TowerType type)
        {
            return Offsets[(int)type];
        }

        public static void Set(TowerType type, Vector2 offset)
        {
            Offsets[(int)type] = offset;
        }

        public static void Reset(TowerType type)
        {
            Offsets[(int)type] = Vector2.zero;
        }

        public static void ResetAll()
        {
            for (int i = 0; i < Offsets.Length; i++)
            {
                Offsets[i] = Vector2.zero;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForPlaySession()
        {
            ResetAll();
        }
    }
}

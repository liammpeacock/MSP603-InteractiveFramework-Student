using UnityEngine;

namespace MSP603.TowerDefense
{
    public enum GameState
    {
        Playing,
        Victory,
        Defeat
    }

    public enum WaveMusicState
    {
        Ready = 0,
        Wave1 = 1,
        Wave2 = 2,
        Wave3 = 3,
        Wave4 = 4,
        Victory = 5,
        Defeat = 6
    }

    public sealed class RunStatistics
    {
        public int WavesCompleted { get; private set; }
        public int TotalEnemiesDefeated { get; private set; }
        public int GoblinRaidersDefeated { get; private set; }
        public int GoblinBrutesDefeated { get; private set; }
        public int RuinKnightsDefeated { get; private set; }

        public void RecordWaveCompleted() => WavesCompleted++;

        public void RecordEnemyDefeated(EnemyType type)
        {
            TotalEnemiesDefeated++;
            switch (type)
            {
                case EnemyType.GoblinRaider: GoblinRaidersDefeated++; break;
                case EnemyType.GoblinBrute: GoblinBrutesDefeated++; break;
                case EnemyType.RuinKnight: RuinKnightsDefeated++; break;
            }
        }
    }

    public enum TowerType
    {
        Archer,
        Mage,
        Cannon,
        Inferno
    }

    public enum EnemyType
    {
        GoblinBrute = 0,
        GoblinRaider = 1,
        RuinKnight = 2
    }

    public readonly struct TowerStats
    {
        public TowerStats(string name, int cost, float range, float damage, float cooldown, float projectileSpeed, float splashRadius, Color color)
        {
            Name = name;
            Cost = cost;
            Range = range;
            Damage = damage;
            Cooldown = cooldown;
            ProjectileSpeed = projectileSpeed;
            SplashRadius = splashRadius;
            Color = color;
        }

        public string Name { get; }
        public int Cost { get; }
        public float Range { get; }
        public float Damage { get; }
        public float Cooldown { get; }
        public float ProjectileSpeed { get; }
        public float SplashRadius { get; }
        public Color Color { get; }
    }

    public static class TowerCatalog
    {
        public static TowerStats Get(TowerType type)
        {
            return type switch
            {
                TowerType.Archer => new TowerStats("Archer", 70, 4.2f, 12f, 0.55f, 12f, 0f, new Color(0.24f, 0.64f, 0.30f)),
                TowerType.Mage => new TowerStats("Mage", 100, 3.8f, 30f, 1.15f, 8f, 0f, new Color(0.45f, 0.30f, 0.78f)),
                TowerType.Cannon => new TowerStats("Cannon", 125, 3.5f, 24f, 1.65f, 7f, 1.25f, new Color(0.78f, 0.35f, 0.18f)),
                TowerType.Inferno => new TowerStats("Inferno", 150, 3.5f, 48f, 1.4f, 0f, 0f, new Color(1f, 0.28f, 0.04f)),
                _ => throw new System.ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}

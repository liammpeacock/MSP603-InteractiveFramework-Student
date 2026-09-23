using UnityEngine;

namespace MSP603.TowerDefense
{
    public sealed class LevelDefinition
    {
        public LevelDefinition(int number, string name, string sceneName, string mapResource,
            int totalWaves, int startingGold, float healthScale, float speedScale,
            Vector3[] path, Vector3[] buildPlots)
        {
            Number = number;
            Name = name;
            SceneName = sceneName;
            MapResource = mapResource;
            TotalWaves = totalWaves;
            StartingGold = startingGold;
            HealthScale = healthScale;
            SpeedScale = speedScale;
            Path = path;
            BuildPlots = buildPlots;
        }

        public int Number { get; }
        public string Name { get; }
        public string SceneName { get; }
        public string MapResource { get; }
        public int TotalWaves { get; }
        public int StartingGold { get; }
        public float HealthScale { get; }
        public float SpeedScale { get; }
        public Vector3[] Path { get; }
        public Vector3[] BuildPlots { get; }

        // Level 2's fixed tower pads sit slightly farther from its route than the
        // other maps. Matching the old first-upgrade reach keeps the base Archer useful
        // without changing the shared Archer balance or redesigning the map.
        public float GetTowerRangeBonus(TowerType type)
        {
            return Number == 2 && type == TowerType.Archer ? 0.35f : 0f;
        }

        // Production maps are 1536x1024. Keeping authored pixel positions here makes
        // route/pad reviews directly comparable with the approved background art.
        private static Vector3 MapPoint(float pixelX, float pixelY, float height = 0.42f)
        {
            const float halfWorldWidth = 12.3f;
            const float halfWorldDepth = 10.5f;
            return new Vector3((pixelX / 1536f * 2f - 1f) * halfWorldWidth, height,
                (1f - pixelY / 1024f * 2f) * halfWorldDepth);
        }

        public static LevelDefinition FromScene(string sceneName)
        {
            return sceneName switch
            {
                "Level02" => CreateLevelTwo(),
                "Level03" => CreateLevelThree(),
                _ => CreateLevelOne()
            };
        }

        private static LevelDefinition CreateLevelOne() => new(
            1, "HIGHLAND APPROACH", "Level01", "Presentation/Level_1_Map", 4, 250, 1f, 1f,
            new[]
            {
                MapPoint(760, 990), MapPoint(620, 870), MapPoint(335, 760),
                MapPoint(300, 610), MapPoint(430, 480), MapPoint(625, 420),
                MapPoint(760, 330), MapPoint(785, 145)
            },
            new[]
            {
                MapPoint(478, 234, .18f), MapPoint(982, 210, .18f),
                MapPoint(870, 345, .18f), MapPoint(741, 430, .18f),
                MapPoint(437, 658, .18f)
            });

        private static LevelDefinition CreateLevelTwo() => new(
            2, "RUINED CROSSING", "Level02", "Presentation/Level_2_Map", 4, 250, 1.22f, 1.08f,
            new[]
            {
                MapPoint(20, 520), MapPoint(170, 675), MapPoint(405, 700),
                MapPoint(600, 575), MapPoint(690, 780), MapPoint(835, 850),
                MapPoint(1010, 770), MapPoint(1070, 585), MapPoint(1240, 450),
                MapPoint(1260, 285), MapPoint(1120, 245), MapPoint(950, 345),
                MapPoint(775, 390), MapPoint(620, 300), MapPoint(705, 205), MapPoint(715, 125)
            },
            new[]
            {
                MapPoint(993, 166, .18f), MapPoint(438, 468, .18f),
                MapPoint(907, 675, .18f)
            });

        private static LevelDefinition CreateLevelThree() => new(
            3, "SHATTERED CITADEL", "Level03", "Presentation/Level_3_Map", 4, 420, 1.5f, 1.15f,
            new[]
            {
                MapPoint(20, 405), MapPoint(145, 455), MapPoint(210, 300),
                MapPoint(255, 195), MapPoint(400, 140), MapPoint(530, 215),
                MapPoint(590, 335), MapPoint(690, 365), MapPoint(745, 265), MapPoint(745, 105)
            },
            new[]
            {
                MapPoint(1049, 176, .18f), MapPoint(423, 267, .18f),
                MapPoint(327, 402, .18f), MapPoint(817, 407, .18f),
                MapPoint(884, 612, .18f), MapPoint(693, 820, .18f)
            });
    }
}

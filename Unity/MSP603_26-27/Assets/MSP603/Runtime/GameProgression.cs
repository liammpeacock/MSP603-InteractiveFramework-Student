using UnityEngine;

namespace MSP603.TowerDefense
{
    public static class GameProgression
    {
        private const string HighestUnlockedLevelKey = "MSP603.HighestUnlockedLevel";

        public static int HighestUnlockedLevel => Mathf.Clamp(PlayerPrefs.GetInt(HighestUnlockedLevelKey, 1), 1, 3);

        public static bool IsUnlocked(int levelNumber) => levelNumber <= HighestUnlockedLevel;
        public static bool IsCompleted(int levelNumber) => levelNumber < HighestUnlockedLevel || (levelNumber == 3 && PlayerPrefs.GetInt("MSP603.GameCompleted", 0) == 1);

        public static void CompleteLevel(int levelNumber)
        {
            int unlockedLevel = Mathf.Min(3, levelNumber + 1);
            if (unlockedLevel > HighestUnlockedLevel)
            {
                PlayerPrefs.SetInt(HighestUnlockedLevelKey, unlockedLevel);
                PlayerPrefs.Save();
            }

            if (levelNumber == 3)
            {
                PlayerPrefs.SetInt("MSP603.GameCompleted", 1);
                PlayerPrefs.Save();
            }
        }
    }
}

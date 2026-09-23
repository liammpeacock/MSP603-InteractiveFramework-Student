using System;
using UnityEngine;

namespace MSP603.TowerDefense
{
    public static class GameplayAudioEvents
    {
        private static readonly string[] TowerTypeParameterLabels = { "Cannon", "Archer", "Inferno", "Mage" };

        public static event Action<string, Vector3> OneShotRequested;
        public static event Action<TowerType, Vector3> TowerFireRequested;
        public static event Action<string, EnemyType, Vector3> EnemyOneShotRequested;
        public static event Action<EnemyType, Transform, bool> EnemyMovementChanged;
        public static event Action<string, float> ParameterChanged;
        public static event Action<string> MusicStateChanged;
        public static event Action<bool> PauseStateChanged;

        public static void RequestOneShot(string eventId, Vector3 position)
        {
            OneShotRequested?.Invoke(eventId, position);
        }

        public static void RequestTowerFire(TowerType towerType, Vector3 position)
        {
            TowerFireRequested?.Invoke(towerType, position);
        }

        public static int TowerTypeParameterValue(TowerType towerType)
        {
            return Array.IndexOf(TowerTypeParameterLabels, TowerTypeParameterLabel(towerType));
        }

        public static string TowerTypeParameterLabel(TowerType towerType)
        {
            return towerType switch
            {
                TowerType.Cannon => "Cannon",
                TowerType.Archer => "Archer",
                TowerType.Inferno => "Inferno",
                TowerType.Mage => "Mage",
                _ => throw new ArgumentOutOfRangeException(nameof(towerType), towerType, null)
            };
        }

        public static void RequestEnemyOneShot(string eventId, EnemyType enemyType, Vector3 position)
        {
            EnemyOneShotRequested?.Invoke(eventId, enemyType, position);
        }

        public static void SetEnemyMovement(EnemyType enemyType, Transform enemy, bool moving)
        {
            EnemyMovementChanged?.Invoke(enemyType, enemy, moving);
        }

        public static void SetParameter(string parameterId, float value)
        {
            ParameterChanged?.Invoke(parameterId, value);
        }

        public static void SetMusicState(string stateId)
        {
            MusicStateChanged?.Invoke(stateId);
        }

        public static void SetPauseState(bool paused)
        {
            PauseStateChanged?.Invoke(paused);
        }
    }
}

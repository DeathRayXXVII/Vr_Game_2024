using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Data/ManagerData/LevelData")]
    public class LevelData : ScriptableObject
    {
        [System.Serializable]
        public struct Level
        {
            public int spawnCount;
            public int laneActiveLimit;
            public int spawnBaseHealth;
            public int spawnBaseDamage;
            public int spawnValue;
            public int spawnScore;
        }
        
        public IntData currentLevel;
        public IntData currentSpawnBounty;

        public int spawnCount => levels[currentLevel].spawnCount;
        public int spawnBounty => levels[currentLevel].spawnValue;

        public Level[] levels;

        private void OnValidate()
        {
            if (!currentLevel) Debug.LogError("Current Level is null. Please assign a value.", this);
            if (!currentSpawnBounty) Debug.LogError("Current Spawn Value is null. Please assign a value.", this);
        }
    }
}
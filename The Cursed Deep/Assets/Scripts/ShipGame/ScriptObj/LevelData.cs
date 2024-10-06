using UnityEngine;
using ZPTools.Interface;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Data/ManagerData/LevelData")]
    public class LevelData : ScriptableObject, IStartupLoader
    {
        [System.Serializable]
        internal struct Level
        {
            public int spawnCount;
            public float spawnRateMin;
            public float spawnRateMax;
            public int laneActiveLimit;
            public int spawnBaseHealth;
            public int spawnBaseDamage;
            public int spawnBounty;
            public int spawnScore;
        }
        
        [System.Serializable]
        public class LevelDataJson
        {
            public int elements;
            public int[] laneActiveLimit;
            public int[] spawnCount;
            public float[] spawnRateMin;
            public float[] spawnRateMax;
            public int[] spawnBaseHealth;
            public int[] spawnBaseDamage;
            public float[] spawnMovementSpeed;
            public int[] spawnBounty;
            public int[] spawnScore;
        }

        
        public IntData currentLevel;

        private Level[] _levels;

        public int spawnCount 
        {
            get => _levels[currentLevel].spawnCount;
            private set => _levels[currentLevel].spawnCount = value;
        }
        public float spawnRateMin => _levels[currentLevel].spawnRateMin;
        public float spawnRateMax => _levels[currentLevel].spawnRateMax;
        public int laneActiveLimit => _levels[currentLevel].laneActiveLimit;
        public int spawnBaseHealth => _levels[currentLevel].spawnBaseHealth;
        public int spawnBaseDamage => _levels[currentLevel].spawnBaseDamage;
        public int spawnBounty => _levels[currentLevel].spawnBounty;
        public int spawnScore => _levels[currentLevel].spawnScore;

        public bool isLoaded { get; private set; }

        public void LoadOnStartup()
        {
            if (isLoaded) return;
            var jsonFile = Resources.Load<TextAsset>("GameData/LevelDataJson");

            if (!jsonFile) return;
            var data = JsonUtility.FromJson<LevelDataJson>(jsonFile.text);
#if UNITY_EDITOR
            Debug.Log($"Loading level data from JSON file: {jsonFile.name}\n" +
                      $"Element Count: {data.elements}\n" +
                      $"Lane Active Limit: {data.laneActiveLimit[currentLevel]}\n" +
                      $"Spawn Count: {data.spawnCount[currentLevel]}\n" +
                      $"Spawn Rate Min: {data.spawnRateMin[currentLevel]}\n" +
                      $"Spawn Rate Max: {data.spawnRateMax[currentLevel]}\n" +
                      $"Spawn Base Health: {data.spawnBaseHealth[currentLevel]}\n" +
                      $"Spawn Base Damage: {data.spawnBaseDamage[currentLevel]}\n" +
                      $"Spawn Bounty: {data.spawnBounty[currentLevel]}\n" +
                      $"Spawn Score: {data.spawnScore[currentLevel]}\n" +
                      "----------------------");
#endif
            
            var levelCount = data.elements;
            _levels = new Level[levelCount];

            for (var i = 0; i < levelCount; i++)
            {
                _levels[i] = new Level
                {
                    laneActiveLimit = data.laneActiveLimit[i],
                    spawnCount = data.spawnCount[i],
                    spawnRateMin = data.spawnRateMin[i],
                    spawnRateMax = data.spawnRateMax[i],
                    spawnBaseHealth = data.spawnBaseHealth[i],
                    spawnBaseDamage = data.spawnBaseDamage[i],
                    spawnBounty = data.spawnBounty[i],
                    spawnScore = data.spawnScore[i]
                };
            }
            
            Resources.UnloadAsset(jsonFile);
#if UNITY_EDITOR
            Debug.Log($"------Level Data------\n" +
                      $"Level: {currentLevel}\n" +
                      $"Spawn Count: {spawnCount}\n" +
                      $"Spawn Rate Min: {spawnRateMin}\n" +
                      $"Spawn Rate Max: {spawnRateMax}\n" +
                      $"Lane Active Limit: {laneActiveLimit}\n" +
                      $"Spawn Base Health: {spawnBaseHealth}\n" +
                      $"Spawn Base Damage: {spawnBaseDamage}\n" +
                      $"Spawn Bounty: {spawnBounty}\n" +
                      $"Spawn Score: {spawnScore}\n" +
                      $"----------------------");
#endif
            isLoaded = true;
        }
        
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!currentLevel) Debug.LogError("Current Level is null. Please assign a value.", this);
#endif
        }
    }
}
using UnityEngine;
using ZPTools.Interface;
using ZPTools.Utility;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Data/ManagerData/LevelData")]
    public class LevelData : ScriptableObject, IStartupLoader
    {
#if UNITY_EDITOR
        public bool _allowDebug;
#endif
        
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
        internal class LevelDataJson
        {
            public int elements;
            public int[] laneActiveLimitsByLevel;
            public int[] spawnCountsByLevel;
            public float[] spawnRateMinByLevel;
            public float[] spawnRateMaxByLevel;
            public int[] healthValuesByLevel;
            public int[] damageValuesByLevel;
            public float[] movementSpeedValuesByLevel;
            public int[] bountyValuesByLevel;
            public int[] scoreValuesByLevel;
        }

        
        public IntData currentLevel;

        private readonly string _dataFilePath = Application.dataPath + "/Resources/GameData/LevelDataJson.json";
        private HashFileChangeDetector _hashFileChangeDetector;
        private Level[] _levels;

        public int spawnCount => _levels[currentLevel].spawnCount;
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
            _hashFileChangeDetector ??= new HashFileChangeDetector(_dataFilePath, _allowDebug);
            var hasChanged = _hashFileChangeDetector.HasChanged();
            
            // Use the change detector to see if the JSON has changed
            if (isLoaded && hasChanged == false)
            {
#if UNITY_EDITOR
                if (_allowDebug) Debug.LogWarning("LevelData is already loaded, and the file has not changed.", this);
                LogCurrentLevelData();
#endif
                return;
            }
            
            var jsonFile = Resources.Load<TextAsset>("GameData/LevelDataJson");

            if (!jsonFile)
            {
#if UNITY_EDITOR
                Debug.LogError("JSON file not found.", this);
#endif
                return;
            }
            var data = JsonUtility.FromJson<LevelDataJson>(jsonFile.text);
            
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"Loading level data from JSON file: {jsonFile.name}\n" +
                      $"Element Count: {data.elements}\n" +
                      $"Lane Active Limit: {data.laneActiveLimitsByLevel[currentLevel]}\n" +
                      $"Spawn Count: {data.spawnCountsByLevel[currentLevel]}\n" +
                      $"Spawn Rate Min: {data.spawnRateMinByLevel[currentLevel]}\n" +
                      $"Spawn Rate Max: {data.spawnRateMaxByLevel[currentLevel]}\n" +
                      $"Spawn Base Health: {data.healthValuesByLevel[currentLevel]}\n" +
                      $"Spawn Base Damage: {data.damageValuesByLevel[currentLevel]}\n" +
                      $"Spawn Bounty: {data.bountyValuesByLevel[currentLevel]}\n" +
                      $"Spawn Score: {data.scoreValuesByLevel[currentLevel]}\n" +
                      "----------------------");
#endif
            
            // Populate levels based on parsed JSON data
            var levelCount = data.elements;
            _levels = new Level[levelCount];

            for (var i = 0; i < levelCount; i++)
            {
                _levels[i] = new Level
                {
                    laneActiveLimit = data.laneActiveLimitsByLevel[i],
                    spawnCount = data.spawnCountsByLevel[i],
                    spawnRateMin = data.spawnRateMinByLevel[i],
                    spawnRateMax = data.spawnRateMaxByLevel[i],
                    spawnBaseHealth = data.healthValuesByLevel[i],
                    spawnBaseDamage = data.damageValuesByLevel[i],
                    spawnBounty = data.bountyValuesByLevel[i],
                    spawnScore = data.scoreValuesByLevel[i]
                };
            }
            
            _hashFileChangeDetector.UpdateState();
            
            Resources.UnloadAsset(jsonFile);

            LogCurrentLevelData();
            
            isLoaded = true;
        }
        
        private void LogCurrentLevelData()
        {
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"------Level Data------\n" +
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
        }
        
        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!currentLevel) Debug.LogError("Current Level is null. Please assign a value.", this);
#endif
        }
    }
}
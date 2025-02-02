using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using ZPTools.Interface;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Data/ManagerData/LevelData")]
    public class LevelData : ScriptableObjectLoadOnStartupDataFromJson, ISaveSystem
    {
        [System.Serializable]
        internal struct Level
        {
            public int spawnCount;
            public float spawnRateMin;
            public float spawnRateMax;
            public int laneActiveLimit;
            public float spawnBaseHealth;
            public float spawnBaseDamage;
            public float spawnBaseSpeed;
            public int spawnBounty;
            public int spawnScore;
        }
        
        [System.Serializable]
        internal class LevelDataJson
        {
            public int elements;
            public int[] levelLaneLimits;
            public int[] levelSpawnCounts;
            public float[] levelMinSpawnRates;
            public float[] levelMaxSpawnRates;
            public float[] levelSpawnHealths;
            public float[] levelSpawnDamages;
            public float[] levelSpawnMoveSpeeds;
            public int[] levelSpawnBounties;
            public int[] levelSpawnScores;
        }
        
        [SerializeField] private IntData _currentLevel;
        [SerializeField] private IntData _countdownToBoss;
        
        [SerializeField] private BoolData _fightingBoss;

        public bool fightingBoss
        {
            get => _fightingBoss;
            set => _fightingBoss.value = value;
        }

        public int currentLevel
        {
            get
            {
                _currentLevel.value = math.clamp(_currentLevel, 1, _levels?.Length - 1 ?? 1);
                return _currentLevel.value;
            }
            set => _currentLevel.value = math.clamp(value, 1, _levels?.Length - 1 ?? 1);
        }

        public int countdownToBoss => currentLevel % 5 == 0 ? 0 : 5 - currentLevel % 5;

        private int _indexDebugged;
        private int currentIndex
        {
            get
            {
                // boss fight occurs every 5 levels
                var isPotentialBossFight = currentLevel % 5 == 0;
                _countdownToBoss.value = countdownToBoss;
                
                // if we are not on a 5th level, we are on a normal level so there is no way we could be fighting a boss
                if (!isPotentialBossFight)
                    fightingBoss = false;
                
                // the current level - 1 to make it 0 based indexed (Since levels start at 1, indexBase 's minimum value is 0)
                var indexBase = currentLevel - 1;
                
                // if _currentLevel is less than 5, return index base
                if (currentLevel < 5)
                {
                    if (!_allowDebug || _indexDebugged == indexBase) return indexBase;
                    
                    _indexDebugged = indexBase;
                    Debug.Log(
                        $"[INFO] Level: {currentLevel}\nIndex Base: {indexBase}\nCountdown to boss: {countdownToBoss}\n" +
                        $"Modified Index: {indexBase}", this);

                    return indexBase;
                }
                
                // increase the index by 1 for every 5 levels (this bypasses the non-boss fight level every 5 levels)
                // Level 0:5 = indexBase+0, Level 6:10 = indexBase+1, Level 11:15 = indexBase+2, etc.
                var modifiedIndex = indexBase + Mathf.FloorToInt((float) indexBase / 5);
                
                if (_allowDebug && _indexDebugged != modifiedIndex)
                {
                    _indexDebugged = modifiedIndex;
                    Debug.Log(
                        $"[INFO] Level: {currentLevel}\nIndex Base: {indexBase}\npotentially fighting boss {isPotentialBossFight}\nCountdown to boss: {countdownToBoss}\n" +
                        $"Modified Index: {modifiedIndex + (fightingBoss ? 0 : 1)}", this);
                }
                
                // if the current level is a multiple of 5 and the player has selected to do the boss fight,
                // then the index is the same as the modified index (boss fight)
                // otherwise, the index is the modified index + 1 (normal fight)
                // Example: On Level 15 you can have either (boss fight) index[16] or (normal fight) index[17]
                return modifiedIndex + (fightingBoss ? 0 : 1);
            }
        }
        
        public void RefreshLevelData()
        {
            var index = currentIndex;
        }
        

        protected override string dataFilePath => Application.dataPath + "/Resources/GameData/LevelDataJson.json";
        protected override string resourcePath => "GameData/LevelDataJson";
        private Level[] _levels;

        public int spawnCount => _levels[currentIndex].spawnCount;
        public float spawnRateMin => _levels[currentIndex].spawnRateMin;
        public float spawnRateMax => _levels[currentIndex].spawnRateMax;
        public int laneActiveLimit => _levels[currentIndex].laneActiveLimit;
        public float spawnBaseHealth => _levels[currentIndex].spawnBaseHealth;
        public float spawnBaseDamage => _levels[currentIndex].spawnBaseDamage;
        public float spawnBaseSpeed => _levels[currentIndex].spawnBaseSpeed;
        public int spawnBounty => _levels[currentIndex].spawnBounty;
        public int spawnScore => _levels[currentIndex].spawnScore;

        private LevelDataJson _tempJsonData;

        protected override void ParseJsonFile(TextAsset jsonObject)
        {
            _tempJsonData = ParseJsonData<LevelDataJson>(jsonObject.text);
        }
        
        protected override void InitializeData()
        {
            if (_levels == null || _levels.Length != _tempJsonData.elements)
            {
                _levels = new Level[_tempJsonData.elements];
            }

            for (var i = 0; i < _tempJsonData.elements; i++)
            {
                _levels[i] = new Level
                {
                    laneActiveLimit = _tempJsonData.levelLaneLimits[i],
                    spawnCount = _tempJsonData.levelSpawnCounts[i],
                    spawnRateMin = _tempJsonData.levelMinSpawnRates[i],
                    spawnRateMax = _tempJsonData.levelMaxSpawnRates[i],
                    spawnBaseHealth = _tempJsonData.levelSpawnHealths[i],
                    spawnBaseDamage = _tempJsonData.levelSpawnDamages[i],
                    spawnBaseSpeed = _tempJsonData.levelSpawnMoveSpeeds[i],
                    spawnBounty = _tempJsonData.levelSpawnBounties[i],
                    spawnScore = _tempJsonData.levelSpawnScores[i]
                };
            }
        }
        
        
        protected override void LogCurrentData()
        {
            if (_allowDebug) 
                Debug.Log($"[INFO] ------Level Data------\n" +
                          $"Level: {currentLevel}\n" +
                          $"Countdown To Boss: {countdownToBoss}\n" +
                          $"Fighting Boss: {fightingBoss}\n" +
                          $"Spawn Count: {spawnCount}\n" +
                          $"Spawn Rate Min: {spawnRateMin}\n" +
                          $"Spawn Rate Max: {spawnRateMax}\n" +
                          $"Lane Active Limit: {laneActiveLimit}\n" +
                          $"Spawn Base Health: {spawnBaseHealth}\n" +
                          $"Spawn Base Damage: {spawnBaseDamage}\n" +
                          $"Spawn Base Speed: {spawnBaseSpeed}\n" +
                          $"Spawn Bounty: {spawnBounty}\n" +
                          $"Spawn Score: {spawnScore}\n" +
                          $"----------------------", this);
        }
        
#if UNITY_EDITOR
        private void OnEnable()
        {
            if (!_currentLevel) Debug.LogError("[ERROR] Current Level IntData is null. Please assign a value.", this);
            if (!_countdownToBoss) Debug.LogError("[ERROR] Current Level IntData is null. Please assign a value.", this);
        }
#endif

        public string filePath => $"{UnityEngine.Application.persistentDataPath}/SaveData/Core/{GetType().Name}.json";
        public bool savePathExists => System.IO.File.Exists(filePath);

        public void Save()
        {
            // JsonUtility
        }

        public void Load()
        {
            
        }

        public void DeleteSavedData()
        {
            
        }
    }
}
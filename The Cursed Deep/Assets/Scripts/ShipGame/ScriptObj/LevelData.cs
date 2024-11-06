using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Data/ManagerData/LevelData")]
    public class LevelData : ScriptableObjectLoadOnStartupDataFromJson
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
            public float[] healthValuesByLevel;
            public float[] damageValuesByLevel;
            public float[] movementSpeedValuesByLevel;
            public int[] bountyValuesByLevel;
            public int[] scoreValuesByLevel;
        }
        
        [SerializeField] private IntData _currentLevel;
        public bool fightingBoss { private get; set; }

        public int currentLevel
        {
            get => _currentLevel;
            set => _currentLevel.value = value < 1 ? 1 : value;
        }

        private int currentIndex
        {
            get
            {
                // boss fight occurs every 5 levels
                var isBossFightLevel = currentLevel % 5 == 0;
                
                // if not fighting a boss, we are on a normal level so _fightingBoss is false
                if (!isBossFightLevel) 
                    fightingBoss = false;
                
                // the current level - 1 to make it 0 based indexed (Since levels start at 1, indexBase 's minimum value is 0)
                var indexBase = currentLevel - 1;
                
                // if _currentLevel is less than 5, return index base
                if (currentLevel < 5) return indexBase;
                
                // increase the index by 1 for every 5 levels (this bypasses the non-boss fight level every 5 levels)
                // Level 0:5 = indexBase+0, Level 6:10 = indexBase+1, Level 11:15 = indexBase+2, etc.
                var modifiedIndex = indexBase + Mathf.FloorToInt((float) indexBase / 5);
                
#if UNITY_EDITOR
                Debug.Log($"Level: {currentLevel}, Index Base: {indexBase}, Modified Index: {modifiedIndex + (fightingBoss ? 0 : 1)}");
#endif
                // if the current level is a multiple of 5 and the player has selected to do the boss fight,
                // then the index is the same as the modified index (boss fight)
                // otherwise, the index is the modified index + 1 (normal fight)
                // Example: On Level 15 you can have either (boss fight) index[16] or (normal fight) index[17]
                return modifiedIndex + (fightingBoss ? 0 : 1);
            }
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
                    laneActiveLimit = _tempJsonData.laneActiveLimitsByLevel[i],
                    spawnCount = _tempJsonData.spawnCountsByLevel[i],
                    spawnRateMin = _tempJsonData.spawnRateMinByLevel[i],
                    spawnRateMax = _tempJsonData.spawnRateMaxByLevel[i],
                    spawnBaseHealth = _tempJsonData.healthValuesByLevel[i],
                    spawnBaseDamage = _tempJsonData.damageValuesByLevel[i],
                    spawnBounty = _tempJsonData.bountyValuesByLevel[i],
                    spawnScore = _tempJsonData.scoreValuesByLevel[i]
                };
            }
        }
        
#if UNITY_EDITOR
        protected override void LogCurrentData()
        {
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
        }
#endif
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_currentLevel) Debug.LogError("Current Level IntData is null. Please assign a value.", this);
        }
#endif
    }
}
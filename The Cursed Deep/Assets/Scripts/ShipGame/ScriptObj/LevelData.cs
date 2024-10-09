using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Data/ManagerData/LevelData")]
    public class LevelData : ScriptableObjectStartupDataFromJson
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
        
        public IntData currentLevel;

        protected override string dataFilePath => Application.dataPath + "/Resources/GameData/LevelDataJson.json";
        protected override string resourcePath => "GameData/LevelDataJson";
        private Level[] _levels;

        public int spawnCount => _levels[currentLevel].spawnCount;
        public float spawnRateMin => _levels[currentLevel].spawnRateMin;
        public float spawnRateMax => _levels[currentLevel].spawnRateMax;
        public int laneActiveLimit => _levels[currentLevel].laneActiveLimit;
        public float spawnBaseHealth => _levels[currentLevel].spawnBaseHealth;
        public float spawnBaseDamage => _levels[currentLevel].spawnBaseDamage;
        public int spawnBounty => _levels[currentLevel].spawnBounty;
        public int spawnScore => _levels[currentLevel].spawnScore;

        private LevelDataJson _tempJsonData;
        
        protected override object tempJsonData
        {
            get => _tempJsonData;
            set => _tempJsonData = (LevelDataJson)value;
        }

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
        
        protected override void LogCurrentData()
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
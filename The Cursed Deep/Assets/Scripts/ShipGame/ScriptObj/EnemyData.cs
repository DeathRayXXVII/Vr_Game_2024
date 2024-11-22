using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Data/ShipGame/EnemyData", order = 0)]
    public class EnemyData : ScriptableObjectLoadOnStartupDataFromJson
    {
        [System.Serializable]
        internal struct EnemyInstanceData
        {
            public float health;
            public float damage;
            public float speed;
            public int bounty;
            public int score;
        }

        [System.Serializable]
        internal struct EnemyDataJson
        {
            public int elements;
            public float[] enemyHealths;
            public float[] enemyDamages;
            public float[] enemySpeeds;
            public int[] enemyBounties;
            public int[] enemyScores;
        }

        [System.Serializable]
        internal struct Enemy
        {
            [SerializeField] private string name;

            // Prefab List that contains variants of a specific enemy type
            public PrefabDataList prefabVariantList;

            public CreepData creepData;
        }

        [SerializeField] [ReadOnly] private int currentIndex;
        public int selectionIndex
        {
            get => currentIndex;
            set
            {
                if (_enemyData == null || _enemyData.Length == 0)
                {
#if UNITY_EDITOR
                    ArrayError("enemyArray", "not initialized or is empty", this);
#endif
                    return;
                }

                // Index clamped between 0 and the length of the enemy array
                currentIndex = Mathf.Clamp(value, 0, _enemyData.Length - 1);
            }
        }

        private EnemyInstanceData[] _enemyInstanceData;
        public float selectionHealth => _enemyInstanceData[selectionIndex].health;
        public float selectionDamage => _enemyInstanceData[selectionIndex].damage;
        public float selectionSpeed => _enemyInstanceData[selectionIndex].speed;
        public int selectionBounty => _enemyInstanceData[selectionIndex].bounty;
        public int selectionScore => _enemyInstanceData[selectionIndex].score;

        [SerializeField] private Enemy[] _enemyData;

        private Enemy enemy => _enemyData[selectionIndex];
        public PrefabDataList prefabList => enemy.prefabVariantList;
        public float health => enemy.creepData.health;
        public void SetHealth(float newHealth) => enemy.creepData.health = newHealth;
        public float damage => enemy.creepData.damage;
        public void SetDamage(float newDamage) => enemy.creepData.damage = newDamage;
        public float speed => enemy.creepData.speed;
        public void SetSpeed(float newSpeed) => enemy.creepData.speed = newSpeed;
        public int bounty => enemy.creepData.bounty;
        public void SetBounty(int newBounty) => enemy.creepData.bounty = newBounty;
        public int score => enemy.creepData.score;
        public void SetScore(int newScore) => enemy.creepData.score = newScore;
        
        public void RandomizeEnemySelection() => selectionIndex = Random.Range(0, _enemyData.Length);

        protected override string dataFilePath => Application.dataPath + "/Resources/GameData/EnemyDataJson.json";
        protected override string resourcePath => "GameData/EnemyDataJson";

        private EnemyDataJson _tempJsonData;

        protected override void ParseJsonFile(TextAsset jsonObject)
        {
            _tempJsonData = ParseJsonData<EnemyDataJson>(jsonObject.text);
        }
        
        protected override void InitializeData()
        {
            if (_enemyInstanceData == null || _enemyInstanceData.Length != _tempJsonData.elements)
            {
                _enemyInstanceData = new EnemyInstanceData[_tempJsonData.elements];
            }
            
            for (int i = 0; i < _tempJsonData.elements; i++)
            {
                _enemyInstanceData[i] = new EnemyInstanceData
                {
                    health = _tempJsonData.enemyHealths[i],
                    damage = _tempJsonData.enemyDamages[i],
                    speed = _tempJsonData.enemySpeeds[i],
                    bounty = _tempJsonData.enemyBounties[i],
                    score = _tempJsonData.enemyScores[i]
                };

                if (i != currentIndex) continue;
                SetHealth(_tempJsonData.enemyHealths[i]);
                SetDamage(_tempJsonData.enemyDamages[i]);
                SetSpeed(_tempJsonData.enemySpeeds[i]);
                SetBounty(_tempJsonData.enemyBounties[i]);
                SetScore(_tempJsonData.enemyScores[i]);
            }
        }
        protected override void LogCurrentData()
        {
#if UNITY_EDITOR
            if (_allowDebug)
                Debug.Log("------Enemy Data------\n" +
                          $"Current Enemy Index: {currentIndex}\n" +
                          $"Current Enemy Base Health: {selectionHealth}\n" +
                          $"Current Enemy Total Health: {health}\n" +
                          $"Current Enemy Base Damage: {selectionDamage}\n" +
                          $"Current Enemy Total Damage: {damage}\n" +
                          $"Current Enemy Base Speed: {selectionSpeed}\n" +
                          $"Current Enemy Total Speed: {speed}\n" +
                          $"Current Enemy Base Bounty: {selectionBounty}\n" +
                          $"Current Enemy Total Bounty: {bounty}\n" +
                          $"Current Enemy Base Score: {selectionScore}\n" +
                          $"Current Enemy Total Score: {score}\n" +
                          "----------------------", this);
#endif
        }
    }
}
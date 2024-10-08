using UnityEngine;

namespace ShipGame.Inventory
{
    [CreateAssetMenu(fileName = "ShipData", menuName = "ShipGame/EnemyData", order = 0)]
    public class EnemyData : GameSelectionData
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
            public float[] enemyHealthValues;
            public float[] enemyDamageValues;
            public float[] enemySpeedValues;
            public int[] enemyBountyValues;
            public int[] enemyScoreValues;
        }

        [System.Serializable]
        internal struct Enemy
        {
            [SerializeField] private string name;

            // Prefab List that contains variants of a specific enemy type
            public PrefabDataList prefabVariantList;

            public CreepData creepData;
        }

        public override int selectionIndex
        {
            get => currentIndex;
            set
            {
                if (_enemyData == null || _enemyData.Length == 0)
                {
#if UNITY_EDITOR
                    Debug.LogError("enemySelections is not initialized or is empty.", this);
#endif
                    return;
                }

                // Index clamped between 0 and the length of the enemy array
                currentIndex = Mathf.Clamp(value, 0, _enemyData.Length - 1);
            }
        }

        [SerializeField] private EnemyInstanceData[] _enemyInstanceData;
        public float selectionHealth => _enemyInstanceData[currentIndex].health;
        public float selectionDamage => _enemyInstanceData[currentIndex].damage;
        public float selectionSpeed => _enemyInstanceData[currentIndex].speed;
        public int selectionBounty => _enemyInstanceData[currentIndex].bounty;
        public int selectionScore => _enemyInstanceData[currentIndex].score;

        private Enemy[] _enemyData;

        private Enemy enemy => _enemyData[currentIndex];
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

        protected override void InitializeData(int count)
        {
            _enemyInstanceData = new EnemyInstanceData[count];
        }

        protected override int ParseJsonFile(string jsonContent)
        {
            var data = JsonUtility.FromJson<EnemyDataJson>(jsonContent);
            for (int i = 0; i < data.elements; i++)
            {
                _enemyInstanceData[i] = new EnemyInstanceData
                {
                    health = data.enemyHealthValues[i],
                    damage = data.enemyDamageValues[i],
                    speed = data.enemySpeedValues[i],
                    bounty = data.enemyBountyValues[i],
                    score = data.enemyScoreValues[i]
                };
            }

            return data.elements;
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
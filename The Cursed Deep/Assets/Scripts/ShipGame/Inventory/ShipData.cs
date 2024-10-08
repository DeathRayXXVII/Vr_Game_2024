using UnityEngine;

namespace ShipGame.Inventory
{
    [CreateAssetMenu(fileName = "ShipData", menuName = "ShipGame/ShipData", order = 0)]
    public class ShipData : GameSelectionData
    {
        [System.Serializable]
        internal struct ShipInstanceData
        {
            public int numberOfLanes;
            public float health;
        }

        [System.Serializable]
        internal struct ShipDataJson
        {
            public int elements;
            public int[] shipLaneCounts;
            public float[] shipHealthValues;
        }
        
        [System.Serializable]
        internal struct Ship
        {
            [SerializeField] private string name;

            // Prefab that determines all other data within this selection
            public PrefabData prefab;

            // Instantiates a cannon in every lane of the ship dependent on and used in the prefab below
            // Requires the cannon selection's prefab and offset
            public InstancerData cannonInstancerData;

            // Ship specific spawner data for ammo
            // Requires the ammo selection's prefab list
            public SpawnerData ammoSpawnerData;

            // Ship specific spawner data for enemies
            // Requires the enemy selection's prefab list
            public SpawnerData enemySpawnerData;
        }
        
        public override int selectionIndex
        {
            get => currentIndex;
            set
            {
                if (
                    _shipInstanceData == null ||
                    _shipInstanceData.Length == 0 ||
                    _shipData == null ||
                    _shipData.Length == 0
                    )
                {
#if UNITY_EDITOR
                    Debug.LogError("shipSelections is not initialized or is empty.", this);
#endif
                    return;
                }
                
                // Index clamped between 0 and the length of the ship array
                currentIndex = Mathf.Clamp(value, 0, _shipInstanceData.Length - 1);
                
                // Pass the prefab to the ship's instancer
                shipInstancerData.SetPrefabData(ship.prefab);
            }
        }
        
        [SerializeField] private ShipInstanceData[] _shipInstanceData;
        private Ship[] _shipData;
        private Ship ship => _shipData[currentIndex];
        public int numberOfLanes => _shipInstanceData[currentIndex].numberOfLanes;
        public float health => _shipInstanceData[currentIndex].health;
        
        // Instancer that performs the instantiation of the ship
        // all other instancers and spawners are inside the instanced ship making them dependent on this instancer
        public InstancerData shipInstancerData;
        public void SetCannonPrefabData(PrefabData cannonPrefab) => ship.cannonInstancerData.SetPrefabData(cannonPrefab);
        public void SetCannonPrefabOffset(Vector3Data offset) => ship.cannonInstancerData.SetPrefabOffset(offset);
        public void SetAmmoPrefabDataList(PrefabDataList ammoPrefabList) => ship.ammoSpawnerData.SetPrefabDataList(ammoPrefabList);
        public void SetEnemyPrefabDataList(PrefabDataList enemyPrefabList) => ship.ammoSpawnerData.SetPrefabDataList(enemyPrefabList);

        protected override string dataFilePath => Application.dataPath + "/Resources/GameData/ShipDataJson.json";
        protected override string resourcePath => "GameData/ShipDataJson";
        
        protected override void InitializeData(int count)
        {
            _shipInstanceData = new ShipInstanceData[count];
        }
        
        protected override int ParseJsonFile(string jsonContent)
        {
            var data = JsonUtility.FromJson<ShipDataJson>(jsonContent);
            for (int i = 0; i < data.elements; i++)
            {
                _shipInstanceData[i] = new ShipInstanceData
                {
                    numberOfLanes = data.shipLaneCounts[i],
                    health = data.shipHealthValues[i]
                };
            }
            return data.elements;
        }
        
        protected override void LogCurrentData()
        {
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"------Ship Data------\n" +
                                       $"Current Ship Index: {selectionIndex}\n" +
                                       $"Current Ship Health: {health}\n" +
                                       $"Current Ship Lane Count: {numberOfLanes}\n" +
                                       $"----------------------", this);
#endif
        }
    }
}

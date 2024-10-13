using UnityEngine;
using UnityEngine.tvOS;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "ShipData", menuName = "Data/ShipGame/ShipData", order = 0)]
    public class ShipData : ScriptableObjectStartupDataFromJson
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
            public int[] shipPurchaseCosts;
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
        
        [SerializeField] [InspectorReadOnly] private int currentIndex;
        public int selectionIndex
        {
            get => currentIndex;
            set
            {
                if (_shipInstanceData == null)
                {
                    ArrayError("shipSelections", "null");
                    return;
                }

                if (_shipInstanceData.Length == 0)
                {
                    ArrayError("shipSelections", "empty");
                    return;
                }

                if (_shipData == null)
                {
                    ArrayError("shipInstanceData", "null");
                    return;
                }

                if (_shipData.Length == 0)
                {
                    ArrayError("shipInstanceData", "empty");
                    return;
                }
                
                // Index clamped between 0 and the length of the ship array
                currentIndex = Mathf.Clamp(value, 0, _shipInstanceData.Length - 1);
                
                // Pass the prefab to the ship's instancer
                shipInstancerData.SetPrefabData(ship.prefab);
            }
        }
        
        private void ArrayError(string arrayName, string error)
        {
#if UNITY_EDITOR
            Debug.LogError($"{arrayName} is {error}.", this);
#endif
        }
        
        
        
        // Instancer that performs the instantiation of the ship
        // all other instancers and spawners are inside the instanced ship making them dependent on this instancer
        public InstancerData shipInstancerData;
        
        private ShipInstanceData[] _shipInstanceData;
        [SerializeField] private Ship[] _shipData;
        private Ship ship => _shipData[selectionIndex];
        public int numberOfLanes => _shipInstanceData[selectionIndex].numberOfLanes;
        public float health => _shipInstanceData[selectionIndex].health;
        public void SetCannonPrefabData(PrefabData cannonPrefab) => ship.cannonInstancerData.SetPrefabData(cannonPrefab);
        public void SetCannonPrefabOffset(Vector3Data offset) => ship.cannonInstancerData.SetPrefabOffset(offset);
        public void SetAmmoPrefabDataList(PrefabDataList ammoPrefabList) => ship.ammoSpawnerData.SetPrefabDataList(ammoPrefabList);
        public void SetEnemyPrefabDataList(PrefabDataList enemyPrefabList) => ship.ammoSpawnerData.SetPrefabDataList(enemyPrefabList);

        protected override string dataFilePath => Application.dataPath + "/Resources/GameData/ShipDataJson.json";
        protected override string resourcePath => "GameData/ShipDataJson";

        private ShipDataJson _tempJsonData;
        
        protected override object tempJsonData
        {
            get => _tempJsonData;
            set => _tempJsonData = (ShipDataJson)value;
        }

        protected override void ParseJsonFile(TextAsset jsonObject)
        {
            _tempJsonData = ParseJsonData<ShipDataJson>(jsonObject.text);
        }
        
        protected override void InitializeData()
        {
            if (_shipInstanceData == null || _shipInstanceData.Length != _tempJsonData.elements)
            {
                _shipInstanceData = new ShipInstanceData[_tempJsonData.elements];
            }
            
            for (int i = 0; i < _tempJsonData.elements; i++)
            {
                _shipInstanceData[i] = new ShipInstanceData
                {
                    numberOfLanes = _tempJsonData.shipLaneCounts[i],
                    health = _tempJsonData.shipHealthValues[i]
                };
            }
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

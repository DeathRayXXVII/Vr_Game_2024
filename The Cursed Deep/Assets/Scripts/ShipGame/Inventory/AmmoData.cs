using UnityEngine;

namespace ShipGame.Inventory
{
    [CreateAssetMenu(fileName = "ShipData", menuName = "ShipGame/AmmoData", order = 0)]
    public class AmmoData : GameSelectionData
    {
        [System.Serializable]
        internal struct AmmoInstanceData
        {
            public int damage;
        }

        [System.Serializable]
        internal struct AmmoDataJson
        {
            public int elements;
            public int[] ammoDamageValues;
        }
        
        [System.Serializable]
        internal struct Ammo
        {
            [SerializeField] private string name;

            // Prefab List that contains variants of a specific ammo type
            public PrefabDataList prefabVariantList;
        }
        
        public override int selectionIndex
        {
            get => currentIndex;
            set 
            {
                if (_ammoData == null || _ammoData.Length == 0)
                {
#if UNITY_EDITOR
                    Debug.LogError("ammoSelections is not initialized or is empty.", this);
#endif
                    return;
                }
                
                // Index clamped between 0 and the length of the ammo array
                currentIndex = Mathf.Clamp(value, 0, _ammoData.Length - 1);
            }
        }
        
        [SerializeField] private AmmoInstanceData[] _ammoInstanceData;
        private Ammo[] _ammoData;
        private Ammo ammo  => _ammoData[currentIndex];
        public PrefabDataList prefabList => ammo.prefabVariantList;
        public int damage => _ammoInstanceData[currentIndex].damage;

        protected override string dataFilePath => Application.dataPath + "/Resources/GameData/AmmoDataJson.json";
        protected override string resourcePath => "GameData/AmmoDataJson";
        
        protected override void InitializeData(int count)
        {
            _ammoInstanceData = new AmmoInstanceData[count];
        }
        
        protected override int ParseJsonFile(string jsonContent)
        {
            var data = JsonUtility.FromJson<AmmoDataJson>(jsonContent);
            for (int i = 0; i < data.elements; i++)
            {
                _ammoInstanceData[i] = new AmmoInstanceData
                {
                   damage = data.ammoDamageValues[i],
                };
            }
            return data.elements;
        }
        
        protected override void LogCurrentData()
        {
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"------Ammo Data------\n" +
                                       $"Current Ammo Index: {damage}\n" +
                                       $"Current Ammo Damage: {damage}\n" +
                                       $"----------------------", this);
#endif
        }
    }
}
using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "AmmoData", menuName = "Data/ShipGame/AmmoData", order = 0)]
    public class AmmoData : ScriptableObjectStartupDataFromJson
    {
        [System.Serializable]
        internal struct AmmoInstanceData
        {
            public float damage;
            public float respawnRate;
        }

        [System.Serializable]
        internal struct AmmoDataJson
        {
            public int elements;
            public float[] ammoDamageValues;
            public float[] ammoRespawnRates;
            public int[] upgradeCosts;
        }
        
        [System.Serializable]
        internal struct Ammo
        {
            [SerializeField] private string name;

            // Prefab List that contains variants of a specific ammo type
            public PrefabDataList prefabVariantList;
        }
        
        [SerializeField] [InspectorReadOnly] private int currentUpgradeIndex;
        public int upgradeIndex
        {
            get => currentUpgradeIndex;
            set 
            {
                if (_ammoInstanceData == null || _ammoInstanceData.Length == 0)
                {
#if UNITY_EDITOR
                    Debug.LogError("ammoUpgradeStats is not initialized or is empty.", this);
#endif
                    return;
                }
                
                // Index clamped between 0 and the length of the ammo array
                currentUpgradeIndex = Mathf.Clamp(value, 0, _ammoInstanceData.Length - 1);
            }
        }
        
        private AmmoInstanceData[] _ammoInstanceData;
        public float damage => _ammoInstanceData[upgradeIndex].damage;
        public float respawnRate => _ammoInstanceData[upgradeIndex].respawnRate;
        
        
        [SerializeField] [InspectorReadOnly] private int currentAmmoIndex;
        public int selectionIndex
        {
            get => currentAmmoIndex;
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
                currentAmmoIndex = Mathf.Clamp(value, 0, _ammoData.Length - 1);
            }
        }
        
        [SerializeField] private Ammo[] _ammoData;
        private Ammo ammo  => _ammoData[selectionIndex];
        public PrefabDataList prefabList => ammo.prefabVariantList;

        protected override string dataFilePath => Application.dataPath + "/Resources/GameData/AmmoDataJson.json";
        protected override string resourcePath => "GameData/AmmoDataJson";

        private AmmoDataJson _tempJsonData;
        
        protected override object tempJsonData
        {
            get => _tempJsonData;
            set => _tempJsonData = (AmmoDataJson)value;
        }

        protected override void ParseJsonFile(TextAsset jsonObject)
        {
            _tempJsonData = ParseJsonData<AmmoDataJson>(jsonObject.text);
        }
        
        protected override void InitializeData()
        {
            if (_ammoInstanceData == null || _ammoInstanceData.Length != _tempJsonData.elements)
            {
                _ammoInstanceData = new AmmoInstanceData[_tempJsonData.elements];
            }
            
            for (int i = 0; i < _tempJsonData.elements; i++)
            {
                _ammoInstanceData[i] = new AmmoInstanceData
                {
                    damage = _tempJsonData.ammoDamageValues[i],
                };
            }
        }
        
        protected override void LogCurrentData()
        {
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"------Ammo Data------\n" +
                                       $"Current Ammo Index: {upgradeIndex}\n" +
                                       $"Current Ammo Damage: {damage}\n" +
                                       $"----------------------", this);
#endif
        }
    }
}
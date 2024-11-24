using UnityEngine;
using ZPTools.Interface;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "AmmoData", menuName = "Data/ShipGame/AmmoData", order = 0)]
    public class AmmoData : ScriptableObjectLoadOnStartupDataFromJson, INeedButton
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
            public float[] ammoDamages;
            public float[] ammoRespawnRates;
            public int[] ammoCosts;
        }
        
        [System.Serializable]
        internal struct Ammo
        {
            [SerializeField] private string name;
            
            // Index of the upgrade level required to unlock this ammo model
            [SerializeField] private int _upgradeIndexToUnlock;
            public int unlockIndex => _upgradeIndexToUnlock;
            // Prefab List that contains variants of a specific ammo model
            public PrefabDataList prefabVariantList;
        }
        
        [SerializeField] private UpgradeData shopHandler;
        
        private void HandleUpgradeEvent(int newIndex) => upgradeIndex = newIndex;
        
        private void OnEnable()
        {
            if (shopHandler != null)
            {
                shopHandler.UpgradeEvent += HandleUpgradeEvent;
            }
        }
        
        private void OnDisable()
        {
            if (shopHandler != null)
            {
                shopHandler.UpgradeEvent -= HandleUpgradeEvent;
            }
        }
        
        [SerializeField, ReadOnly] private int currentUpgradeIndex;
        public int upgradeIndex
        {
            get => currentUpgradeIndex;
            set 
            {
                if (_ammoInstanceData == null || _ammoInstanceData.Length == 0)
                {
#if UNITY_EDITOR
                    ArrayError("ammoUpgradeArray", "not initialized or is empty", this);
#endif
                    return;
                }
                var performUnlockCheck = value > 0 && value < _ammoInstanceData.Length;
                if (performUnlockCheck && _ammoData is { Length: > 0 } )
                {
                    if (selectionIndex+1 < _ammoData.Length && value == _ammoData[selectionIndex+1].unlockIndex)
                    {
#if UNITY_EDITOR
                        if (_allowDebug) Debug.Log($"Selection Index increasing from {selectionIndex} to {selectionIndex+1} because upgrade index is {value}", this);
#endif
                        selectionIndex++;
                    }

                    if (selectionIndex > 0 && value < _ammoData[selectionIndex].unlockIndex)
                    {
#if UNITY_EDITOR
                        if (_allowDebug) Debug.Log($"Selection Index decreasing from {selectionIndex} to {selectionIndex-1} because upgrade index is {value}", this);
#endif
                        selectionIndex--;
                    }
                }
                
                // Index clamped between 0 and the length of the ammo array
                currentUpgradeIndex = Mathf.Clamp(value, 0, _ammoInstanceData.Length - 1);
            }
        }
        
        private AmmoInstanceData[] _ammoInstanceData;
        public float damage => _ammoInstanceData[upgradeIndex].damage;
        public float respawnRate => _ammoInstanceData[upgradeIndex].respawnRate;
        
        
        [SerializeField, ReadOnly] private int currentAmmoIndex;
        public int selectionIndex
        {
            get => currentAmmoIndex;
            set 
            {
                if (_ammoData == null || _ammoData.Length == 0)
                {
#if UNITY_EDITOR
                    ArrayError("ammoArray", "not initialized or is empty", this);
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
                    damage = _tempJsonData.ammoDamages[i],
                    respawnRate = _tempJsonData.ammoRespawnRates[i]
                };
            }
        }
        
        protected override void LogCurrentData()
        {
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"------Ammo Data------\n" +
                                       $"Current Ammo Index: {upgradeIndex}\n" +
                                       $"Current Ammo Damage: {damage}\n" +
                                        $"Current Ammo Respawn Rate: {respawnRate}\n" +
                                       $"----------------------", this);
#endif
        }
        
        public System.Collections.Generic.List<(System.Action, string)> GetButtonActions()
        {
            return new System.Collections.Generic.List<(System.Action, string)>
            {
#if UNITY_EDITOR
                (() => upgradeIndex++, "Increase Upgrade Index"),
                (() => upgradeIndex--, "Decrease Upgrade Level"),
#endif
            };
        }
    }
}
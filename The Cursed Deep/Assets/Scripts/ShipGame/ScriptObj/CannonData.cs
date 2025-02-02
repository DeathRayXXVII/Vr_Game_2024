using UnityEngine;
using ZPTools.Interface;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "CannonData", menuName = "Data/ShipGame/CannonData", order = 0)]
    public class CannonData : ScriptableObjectLoadOnStartupDataFromJson, INeedButton
    {
        [System.Serializable]
        internal struct CannonInstanceData
        {
            public float damage;
        }

        [System.Serializable]
        internal struct CannonDataJson
        {
            public int elements;
            public float[] cannonDamages;
        }
        
        [System.Serializable]
        internal struct Cannon
        {
            [SerializeField] private string name;
            
            // Index of the upgrade level required to unlock this cannon model
            [SerializeField] private int _upgradeIndexToUnlock;
            public int unlockIndex => _upgradeIndexToUnlock;
            // Prefab that determines all other data within this selection
            public PrefabData prefab;

            [System.Serializable]
            public struct CannonPrefabOffset
            {
                [SerializeField] private string targetShip;

                // Offset of the cannon from the positions predefined in the ship prefab
                public Vector3Data cannonOffset;
            }

            // Array of cannon prefab offsets
            // The index order need to match the offset for the corresponding ship in the ship selection order to work correctly
            public CannonPrefabOffset[] cannonOffsetsByShip;

            // Returns the cannon offset for the specific ship at given index
            public Vector3Data GetCannonOffset(int shipIndex)
            {
                return cannonOffsetsByShip[shipIndex].cannonOffset;
            }
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
        
        [SerializeField] [ReadOnly] private int currentUpgradeIndex;
        private CannonInstanceData[] _cannonInstanceData;
        public float damage => _cannonInstanceData[upgradeIndex].damage;
        public int upgradeIndex
        {
            get => currentUpgradeIndex;
            set 
            {
                if (_cannonInstanceData == null || _cannonInstanceData.Length == 0)
                {
                    ArrayError("cannonUpgradeArray", "not initialized or is empty", this);
                    if (_cannonInstanceData == null || _cannonInstanceData.Length == 0)
                    {
                        Debug.LogError("[ERROR] Resolution failed. Cannon Upgrade Array is null or empty.", this);
                        return;
                    }
                }
                
                var performUnlockCheck = value > 0 && value < _cannonInstanceData.Length;
                if (performUnlockCheck && _cannonData is { Length: > 0 } )
                {
                    if (selectionIndex+1 < _cannonData.Length && value == _cannonData[selectionIndex+1].unlockIndex)
                    {
                        if (_allowDebug) Debug.Log($"[INFO] Selection Index increasing from {selectionIndex} to {selectionIndex+1} because upgrade index is {value}", this);
                        selectionIndex++;
                    }

                    if (selectionIndex > 0 && value < _cannonData[selectionIndex].unlockIndex)
                    {
                        if (_allowDebug) Debug.Log($"[INFO] Selection Index decreasing from {selectionIndex} to {selectionIndex-1} because upgrade index is {value}", this);
                        selectionIndex--;
                    }
                }
                
                // Index clamped between 0 and the length of the ammo array
                currentUpgradeIndex = Mathf.Clamp(value, 0, _cannonInstanceData.Length - 1);
            }
        }

        [SerializeField] [ReadOnly] private int currentCannonIndex;
        public int selectionIndex
        {
            get => currentCannonIndex;
            set
            {
                if (_cannonData == null || _cannonData.Length == 0)
                {
                    ArrayError("cannonSelectionArray", "not initialized or is empty", this);
                    return;
                }
                // Index clamped between 0 and the length of the cannon array
                currentCannonIndex = Mathf.Clamp(value, 0, _cannonData.Length - 1);
            }
        }
        
        [SerializeField] private Cannon[] _cannonData;
        private Cannon cannon => _cannonData[selectionIndex];
        public PrefabData prefab => cannon.prefab;
        public Vector3Data GetCannonOffset(int shipIndex) => cannon.GetCannonOffset(shipIndex);

        protected override string dataFilePath => Application.dataPath + "/Resources/GameData/CannonDataJson.json";
        protected override string resourcePath => "GameData/CannonDataJson";

        private CannonDataJson _tempJsonData;

        protected override void ParseJsonFile(TextAsset jsonObject)
        {
            _tempJsonData = ParseJsonData<CannonDataJson>(jsonObject.text);
        }
        
        protected override void InitializeData()
        {
            if (_cannonInstanceData == null || _cannonInstanceData.Length != _tempJsonData.elements)
            {
                _cannonInstanceData = new CannonInstanceData[_tempJsonData.elements];
            }
            
            for (int i = 0; i < _tempJsonData.elements; i++)
            {
                _cannonInstanceData[i] = new CannonInstanceData
                {
                    damage = _tempJsonData.cannonDamages[i],
                };
            }
        }
        
        protected override void LogCurrentData()
        {
            if (_allowDebug) Debug.Log("[INFO]\n------Cannon Data------\n" +
                                       $"Current Cannon Index: {selectionIndex}\n" +
                                       $"Current Cannon Damage: {damage}\n" +
                                       $"----------------------", this);
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
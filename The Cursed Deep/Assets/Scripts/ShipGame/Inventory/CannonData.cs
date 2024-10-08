using UnityEngine;

namespace ShipGame.Inventory
{
    [CreateAssetMenu(fileName = "ShipData", menuName = "ShipGame/CannonData", order = 0)]
    public class CannonData : GameSelectionData
    {
        [System.Serializable]
        internal struct CannonInstanceData
        {
            public int damage;
        }

        [System.Serializable]
        internal struct CannonDataJson
        {
            public int elements;
            public int[] cannonDamageValues;
        }
        
        [System.Serializable]
        internal struct Cannon
        {
            [SerializeField] private string name;
            
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

        public override int selectionIndex
        {
            get => currentIndex;
            set
            {
                if (_cannonData == null || _cannonData.Length == 0)
                {
#if UNITY_EDITOR
                    Debug.LogError("cannonSelections is not initialized or is empty.", this);
#endif
                    return;
                }
                // Index clamped between 0 and the length of the cannon array
                currentIndex = Mathf.Clamp(value, 0, _cannonData.Length - 1);
            }
        }
        
        [SerializeField] private CannonInstanceData[] _cannonInstanceData;
        private Cannon[] _cannonData;
        private Cannon cannon => _cannonData[currentIndex];
        public PrefabData prefab => cannon.prefab;
        public int damage => _cannonInstanceData[currentIndex].damage;
        public Vector3Data GetCannonOffset(int shipIndex) => cannon.GetCannonOffset(shipIndex);

        protected override string dataFilePath => Application.dataPath + "/Resources/GameData/CannonDataJson.json";
        protected override string resourcePath => "GameData/CannonDataJson";
        
        protected override void InitializeData(int count)
        {
            _cannonInstanceData = new CannonInstanceData[count];
        }
        
        protected override int ParseJsonFile(string jsonContent)
        {
            var data = JsonUtility.FromJson<CannonDataJson>(jsonContent);
            for (int i = 0; i < data.elements; i++)
            {
                _cannonInstanceData[i] = new CannonInstanceData
                {
                   damage = data.cannonDamageValues[i],
                };
            }
            return data.elements;
        }
        
        protected override void LogCurrentData()
        {
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"------Cannon Data------\n" +
                                       $"Current Cannon Index: {currentIndex}\n" +
                                        $"Current Cannon Damage: {damage}\n" +
                                       $"----------------------", this);
#endif
        }
    }
}
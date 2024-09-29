using UnityEngine;

namespace ShipGame.Inventory
{
    [System.Serializable]
    public struct CannonSelection
    {
        [SerializeField] private string selectionName;
            
        // Prefab that determines all other data within this selection
        public PrefabData prefab;
            
        [System.Serializable]
        public struct CannonPrefabOffset
        {
            public string targetShip;
                
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
}
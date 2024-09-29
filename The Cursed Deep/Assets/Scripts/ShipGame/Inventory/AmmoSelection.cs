using UnityEngine;

namespace ShipGame.Inventory
{
    [System.Serializable]
    public struct AmmoSelection
    {
        [SerializeField] private string selectionName;
            
        // Prefab List that contains variants of a specific ammo type
        public PrefabDataList prefabVariantList;
    }
}
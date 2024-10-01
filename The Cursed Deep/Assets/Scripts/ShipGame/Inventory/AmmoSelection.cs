using UnityEngine;

namespace ShipGame.Inventory
{
    [System.Serializable]
    public struct AmmoSelection
    {
        [SerializeField] private string selectionName;
        [SerializeField] private int damage;
        [SerializeField] private int cost;
            
        // Prefab List that contains variants of a specific ammo type
        public PrefabDataList prefabVariantList;
    }
}
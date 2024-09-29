using UnityEngine;

namespace ShipGame.Inventory
{
    [System.Serializable]
    public struct EnemySelection
    {
        [SerializeField] private string selectionName;
            
        // Prefab List that contains variants of a specific enemy type
        public PrefabDataList prefabVariantList;
    }
}
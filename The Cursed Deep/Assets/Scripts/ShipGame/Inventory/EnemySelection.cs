using UnityEngine;

namespace ShipGame.Inventory
{
    [System.Serializable]
    public struct EnemySelection
    {
        [SerializeField] private string selectionName;
        
        [SerializeField] private int bounty;
        
        [SerializeField] private int health;
        
        [SerializeField] private int speed;
        
        [SerializeField] private int score;
            
        // Prefab List that contains variants of a specific enemy type
        public PrefabDataList prefabVariantList;
    }
}
using UnityEngine;

namespace ShipGame.Inventory
{
    [System.Serializable]
    public struct EnemySelection
    {
        [SerializeField] private string _name;
        
        [SerializeField] private int _health;
        
        [SerializeField] private int _damage;
        
        [SerializeField] private int _speed;
        
        [SerializeField] private int _bounty;
        
        [SerializeField] private int _score;
            
        // Prefab List that contains variants of a specific enemy type
        public PrefabDataList prefabVariantList;
        
        public int health => _health;
        public int damage => _damage;
        public int speed => _speed;
        public int bounty => _bounty;
        public int score => _score;
    }
}
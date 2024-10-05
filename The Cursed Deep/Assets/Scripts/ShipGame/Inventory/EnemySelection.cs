using UnityEngine;

namespace ShipGame.Inventory
{
    [System.Serializable]
    public struct EnemySelection
    {
        [SerializeField] private string _name;
        [SerializeField] private float _health;
        [SerializeField] private float _damage;
        [SerializeField] private float _speed;
        [SerializeField] private int _bounty;
        [SerializeField] private int _score;
            
        // Prefab List that contains variants of a specific enemy type
        public PrefabDataList prefabVariantList;
        
        public CreepData creepData;

        public float health => creepData.health;
        public void SetHealth(float newHealth) => creepData.health = newHealth;
        public float selectionHealth => _health;
        
        public float damage => creepData.damage;
        public void SetDamage(float newDamage) => creepData.damage = newDamage;
        public float selectionDamage => _damage;
        
        public float speed => creepData.speed;
        public void SetSpeed(float newSpeed) => creepData.speed = newSpeed;
        public float selectionSpeed => _speed;
        
        public int bounty => creepData.bounty;
        public void SetBounty(int newBounty) => creepData.bounty = newBounty;
        public int selectionBounty => _bounty;
        
        public int score => creepData.score;
        public void SetScore(int newScore) => creepData.score = newScore;
        public int selectionScore => _score;
    }
}
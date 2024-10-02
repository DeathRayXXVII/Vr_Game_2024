using UnityEngine;

namespace ShipGame.Inventory
{
    [System.Serializable]
    public struct AmmoSelection
    {
        [SerializeField] private string _name;
        [SerializeField] private int _damage;
        [SerializeField] private int _cost;
            
        // Prefab List that contains variants of a specific ammo type
        public PrefabDataList prefabVariantList;
        
        public string name => _name;
        public int damage => _damage;
        public int cost => _cost;
    }
}
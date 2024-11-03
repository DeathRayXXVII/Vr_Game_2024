using UnityEngine;

namespace ZPTools.Interface
{
    public interface IDamageDealer
    {
        float damage { get; set; }
        float health { get; set; }
        Vector3 hitPoint { get; }
        public bool canDealDamage { get; }
        void DealDamage(IDamagable target);
    }
}
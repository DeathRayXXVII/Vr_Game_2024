namespace ZPTools.Interface
{
    public interface IDamageDealer
    {
        float damage { get; set; }
        float health { get; set; }
        public bool canDealDamage { get; }
        void DealDamage(IDamagable target);
    }
}
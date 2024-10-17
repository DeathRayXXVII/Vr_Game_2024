namespace ZPTools.Interface
{
    public interface IDamageDealer
    {
        float damage { get; set; }
        float health { get; set; }
        void DealDamage(IDamagable target);
    }
}
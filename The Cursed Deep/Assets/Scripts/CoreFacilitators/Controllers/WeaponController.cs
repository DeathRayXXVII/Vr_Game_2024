using UnityEngine;
using UnityEngine.Events;
using ZPTools.Interface;
using static ZPTools.Utility.UtilityFunctions;

public class WeaponController : MonoBehaviour, IDamagable, IDamageDealer
{
    public WeaponData weaponData;
    public UnityEvent onDamageDealt, onDurabilityDepleted;
    public bool canDealDamage { get; set; } = true;
    
    public float damage
    {
     get => weaponData.damage;
     set => weaponData.damage = value;
    }

    public float health
    {
     get => weaponData.health;
     set => weaponData.health = value;
    }

    private void OnCollisionEnter(Collision other)
    {
        var damageable = GetInterfaceComponent<IDamagable>(other.gameObject);
        // if(damageable is not { canReceiveDamage: true }) return;
        DealDamage(damageable);
    }

    private void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0) onDurabilityDepleted.Invoke();
    }

    public void TakeDamage(IDamageDealer dealer) => TakeDamage(dealer.damage);
    
    public void DealDamage(IDamagable target)
    {
        if (!canDealDamage) return;
        onDamageDealt.Invoke();
        target.TakeDamage(this);
    }
}

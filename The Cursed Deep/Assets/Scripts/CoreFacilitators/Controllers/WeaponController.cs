using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using ZPTools.Interface;
using static ZPTools.Utility.UtilityFunctions;

public class WeaponController : MonoBehaviour, IDamagable, IDamageDealer
{
    public WeaponData weaponData;
    public UnityEvent onDamageDealt, onDurabilityDepleted;
    public bool canDealDamage { get; private set;  } = true;

    private WaitForSeconds _damageWait;
    private readonly WaitForFixedUpdate _wffu = new();
    private Coroutine _damageCoroutine;

    [SerializeField, SteppedRange(rangeMin:0.5f, rangeMax:10f, step:0.1f)] private float damageCooldown = 3f;
    
    private void Awake() => _damageWait = new WaitForSeconds(damageCooldown);

    private void OnEnable() => canDealDamage = true;

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

    public Vector3 hitPoint { get; private set; }

    private void OnCollisionEnter(Collision other)
    {
        var damageable = AdvancedGetComponent<IDamagable>(other.gameObject);
        if(damageable == null) return;
        hitPoint = other.GetContact(0).point;
        DealDamage(damageable);
    }

    private void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0) onDurabilityDepleted.Invoke();
    }

    public void TakeDamage(IDamageDealer dealer) => TakeDamage(dealer.damage);
    
    private IEnumerator HandleDealingDamage(IDamagable target)
    {
        canDealDamage = false;
        target.TakeDamage(this);
        yield return _damageWait;
        
        canDealDamage = true;
        yield return _wffu;
        
        _damageCoroutine = null;
    }
    
    public void DealDamage(IDamagable target)
    {
        if (!canDealDamage) return;
        _damageCoroutine ??= StartCoroutine(HandleDealingDamage(target));
    }
}

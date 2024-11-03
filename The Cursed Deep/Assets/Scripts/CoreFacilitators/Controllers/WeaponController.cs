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

    [SerializeField, SteppedRange(rangeMin:0.5f, rangeMax:10f, step:0.1f)] private float damageCooldown = 1f;
    
    private void Awake() => _damageWait = new WaitForSeconds(damageCooldown);

    private void OnEnable() => canDealDamage = true;

    private void OnDisable()
    {
        if (_damageCoroutine != null) StopCoroutine(_damageCoroutine);
        _damageCoroutine = null;
    }

    private void OnDestroy()
    {
        if (_damageCoroutine != null) StopCoroutine(_damageCoroutine);
        _damageCoroutine = null;
    }

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
        Debug.Log($"[WeaponController] Collision of {name} with {other.gameObject.name}\ndamageable: {damageable != null}\ncanDealDamage: {canDealDamage}", this);
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
        yield return _wffu;
        
        canDealDamage = true;
        _damageCoroutine = null;
    }
    
    public void DealDamage(IDamagable target)
    {
        if (!canDealDamage)
        {
            Debug.Log($"[WeaponController] Cannot deal damage to {target} yet.", this);
            return;
        }
        Debug.Log($"[WeaponController] Dealing damage to {target}, coroutine: {(_damageCoroutine == null ? "correctly is null" : "incorrectly is not null")}", this);
        _damageCoroutine ??= StartCoroutine(HandleDealingDamage(target));
    }
    
    
}

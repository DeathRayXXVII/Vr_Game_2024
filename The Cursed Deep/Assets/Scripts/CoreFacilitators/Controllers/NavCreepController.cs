using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using ZPTools.Interface;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavAgentBehavior))]
[RequireComponent(typeof(HealthBehavior))]
public class NavCreepController : MonoBehaviour, IDamageDealer
{
    public CreepData creepData;
    
    private NavAgentBehavior _agentBehavior;
    private HealthBehavior _health;
    
    private WaitForFixedUpdate _wffu;

    private void Awake()
    {
        _health = GetComponent<HealthBehavior>();
        _wffu = new WaitForFixedUpdate();
        StartCoroutine(Setup());
    }
    
    private IEnumerator Setup()
    {
        var attempts = 0;
        while (!_agentBehavior && attempts < 5)
        {
            _agentBehavior = GetComponent<NavAgentBehavior>();
            attempts++;
            yield return _wffu;
        }
        
        if (_agentBehavior)
        {
            _agentBehavior.SetSpeed(creepData.speed);
            _agentBehavior.SetRadius(creepData.radius);
            _agentBehavior.SetHeight(creepData.height);
        } else {
#if UNITY_EDITOR
            Debug.LogError("NavAgentBehavior not found in " + name, this);
#endif
        }
        
        _health.maxHealth = creepData.health;
        _health.health = creepData.health;
    }
    
    public void StopMovement()
    {
        _agentBehavior.StopMovement();
    }

    private void OnCollisionEnter(Collision other)
    {
        var damageable = other.gameObject.GetComponent<IDamagable>();
        if (damageable != null) { DealDamage(damageable); }
    }

    public float damage
    {
        get => creepData.damage;
        set => creepData.damage = value;
    }

    public float health
    {
        get => creepData.health;
        set => creepData.health = value;
    }
    
    public void DealDamage(IDamagable target) => target.TakeDamage(this);
}

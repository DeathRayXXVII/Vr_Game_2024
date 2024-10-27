using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using ZPTools.Interface;
using static ZPTools.Utility.UtilityFunctions;

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
            // yield return _agentBehavior.Setup();
            
            _agentBehavior.SetSpeed(creepData.speed);
            _agentBehavior.SetRadius(creepData.radius);
            _agentBehavior.SetHeight(creepData.height);
        } else {
#if UNITY_EDITOR
            Debug.LogError("NavAgentBehavior not found in " + name, this);
#endif
        }
        
        _health.maxHealth = health;
        _health.health = health;
    }
    
    public void StopMovement() => _agentBehavior.StopMovement();

    private void OnCollisionEnter(Collision other)
    {
        var damageable = GetInterfaceComponent<IDamagable>(other.gameObject);
        // if(damageable is not { canReceiveDamage: true }) return;
        Debug.Log($"Collision detected with: {other.gameObject}\nDealing damage to Damageable: {other.gameObject.name}, from DamageDealer: {this}", this);
        DealDamage(damageable);
    }

    private WaitForSeconds _damageWait;
    private Coroutine _damageCoroutine;
    [SerializeField] private float damageCooldown = 3f;
    private IEnumerator ExecuteDamage(float amount)
    {
        // canReceiveDamage = false;
        // Debug.Log($"Applying damage: {amount} to {gameObject.name}", this);
        // ShowDamage(amount.ToString());
        // if (amount > -1) amount *= -1;
        // AddAmountToHealth(amount);
        //
        // yield return _damageWait;
        // canReceiveDamage = true;
        // _damageCoroutine = null;
        yield return null;
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

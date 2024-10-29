using UnityEngine;
using UnityEngine.Events;
using ZPTools.Interface;
using Random = UnityEngine.Random;

public class HealthBehavior : MonoBehaviour, IDamagable
{
    public UnityEvent onHealthGained, onHealthLost, onThreeQuarterHealth, onHalfHealth, onQuarterHealth, onHealthDepleted;
    
    [SerializeField] [InspectorReadOnly] private float _currentHealth;
    [SerializeField] private FloatData currentHealthData;
    private float _previousCheckHealth;
    [SerializeField] private FloatData _maxHealth;
    [SerializeField] private GameObject floatingText;
    [SerializeField] private GameObject textPivot;
    private bool _isDead;

    public float health
    {
        get => !currentHealthData ? _currentHealth : currentHealthData;
        set
        {
            if (currentHealthData) currentHealthData.value = value;
            _currentHealth = value;
        }
    }

    public float maxHealth
    {
        get => _maxHealth;
        set => _maxHealth.value = value;
    }

    // private void Awake() => _damageWait = new WaitForSeconds(damageCooldown);

    private void OnEnable() => _isDead = false;

    private void Start()
    {
        if (!_maxHealth) _maxHealth = ScriptableObject.CreateInstance<FloatData>();
        if (maxHealth <= 0) maxHealth = 1;
        health = maxHealth;
    }
    
    private void CheckHealthEvents()
    {
        if (health > maxHealth) health = maxHealth;
        if (Mathf.Approximately(health, maxHealth)) return;
        if (health < 0) health = 0;
        
        if (health <= maxHealth * 0.75f && health >= maxHealth * 0.5f) onThreeQuarterHealth.Invoke();
        else if (health <= maxHealth * 0.5f && health >= maxHealth * 0.25f) onHalfHealth.Invoke();
        else if (health <= maxHealth * 0.25f && health > maxHealth * 0) onQuarterHealth.Invoke();
        else if (health <= 0)
        {
            _isDead = true;
            onHealthDepleted.Invoke();
        }
    }
    
    public void SetHealth(float newHealth)
    {
        if (_isDead) return;
        health = newHealth;
        CheckHealthEvents();
    }
    
    public void SetMaxHealth(float newMax) => maxHealth = newMax;

    public void AddAmountToHealth(float amount)
    {
        if (_isDead) return;
        var previousHealth = health;
        health += amount;
        if (health - previousHealth > 0) onHealthGained.Invoke();
        else if (health >= 0 && previousHealth > 0) onHealthLost.Invoke();
        CheckHealthEvents();
    }
    public void TakeDamage(IDamageDealer dealer)
    {
        if (_isDead) return;
        var amount = dealer.damage;
        
        ShowDamage(amount.ToString());
        // Debug.Log($"Applying damage: {amount} to {name}\nPrevious Health: {health} | New Health: {health + (amount > -1 ? amount * -1 : amount)}", this);
        if (amount > -1) amount *= -1;
        AddAmountToHealth(amount);
    }
    
    private void ShowDamage(string text)
    {
        if (!floatingText) return;
        var textObj = Instantiate(floatingText, textPivot.transform.position, Quaternion.identity);
        string[] states = {"DamageTextVariant1", "DamageTextVariant2", "DamageTextVariant3", "DamageTextVariant4"};
        var randomState = states[Random.Range(0, states.Length)];
        floatingText.GetComponent<Animator>().Play(randomState); 
        textObj.GetComponentInChildren<TextMesh>().text = text;
    }
}
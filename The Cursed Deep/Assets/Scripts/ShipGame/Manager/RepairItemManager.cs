using UI.DialogueSystem;
using UnityEngine;

public class RepairItemManager : MonoBehaviour
{
    [SerializeField] private IntData _repairCost;
    [SerializeField] private DialoguePurchaseHandler _purchaseHandler;
    
    [SerializeField] private BoolData _fullHealthBool;
    [SerializeField] private FloatData _currentHealth;
    [SerializeField] private FloatData _maxHealth;
    [SerializeField] private float _minHealth = 3f;
    
    private float repairAmount => _maxHealth - _currentHealth;

    private const float DEFAULT_BASE = 4f;
    private const float LOG_SCALAR = 10000f;
    
    private const float DEFAULT_GROWTH = 2f;
    private const float MAX_GROWTH = 3f;
    private const float GROWTH_SCALAR = 0.04f;
    
    private const float MIN_BASE = 2f;
    private const float BASE_SCALAR = 0.01f;

    private float CalculateGrowthFactor()
    {
        // Growth factor scales with max health but is capped at MAX_GROWTH
        return Mathf.Min(DEFAULT_GROWTH + _maxHealth * GROWTH_SCALAR, MAX_GROWTH);
    }

    private float CalculateBaseCost()
    {
        // Logarithm base decreases with max health but is clamped to MIN_BASE
        return Mathf.Max(DEFAULT_BASE - _maxHealth * BASE_SCALAR, MIN_BASE);
    }

    private float CalculateBaseCostScalar()
    {
        // Scalar factor based on max health
        return _maxHealth / _minHealth;
    }

    private int CalculateRepairCost()
    {
        var growthFactor = CalculateGrowthFactor();
        var baseCost = CalculateBaseCost();
        var baseCostScalar = CalculateBaseCostScalar();

        var exponentialCost = Mathf.Pow(repairAmount, growthFactor);
        
        var costAdjustment = 
            Mathf.Log(_maxHealth * growthFactor * LOG_SCALAR, baseCost) * baseCostScalar;

        return Mathf.FloorToInt(exponentialCost + costAdjustment);
    }

    public void PerformRepair()
    {
        _currentHealth.Set(_maxHealth);
        UpdateRepairCost();
    }


    private void UpdateRepairCost()
    {
        _fullHealthBool.Set(_currentHealth >= _maxHealth);
        
        if (_fullHealthBool.value) return;
        
        _repairCost.Set(CalculateRepairCost());
        if (_purchaseHandler) _purchaseHandler.cost = _repairCost.value;
    }
    
    private void Awake()
    {
        UpdateRepairCost();
    }
}

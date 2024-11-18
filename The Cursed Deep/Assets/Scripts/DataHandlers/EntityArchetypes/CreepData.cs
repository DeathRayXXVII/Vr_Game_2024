using System;
using System.Collections.Generic;
using UnityEngine;
using ZPTools.Interface;

[CreateAssetMenu (fileName = "CreepData", menuName = "Data/Entity/CreepData")]
public class CreepData : ScriptableObject, INeedButton
{
    [SerializeField] private string _unitName, _type;

    [Header("Stats Containers")]
    [Tooltip("Original health of the creep before any modifiers.")]
    [SerializeField] private FloatData _health;
    [Tooltip("Amount of damage the creep deals.")]
    [SerializeField] private FloatData _damage;
    [Tooltip("NavMeshAgent speed of the creep.")]
    [SerializeField] private FloatData _speed;
    
    [Tooltip("Monetary value of the creep.")]
    [SerializeField] private IntData _bounty;
    [Tooltip("Score value of the creep.")]
    [SerializeField] private IntData _score;
    
    [Header("Base Stats")]
    [Tooltip("NavMeshAgent speed of the creep. The base value will be used only if the corresponding FloatData is not set.")]
    [SerializeField] private float _baseSpeed;
    [Tooltip("NavMeshAgent height of the creep.")]
    [SerializeField] private float _height;
    [Tooltip("NavMeshAgent radius of the creep.")]
    [SerializeField] private float _radius;

    [Tooltip("Original health of the creep before any modifiers. The base value will be used only if the corresponding IntData is not set.")]
    [SerializeField] private int _baseHealth;
    [Tooltip("Amount of damage the creep deals. The base value will be used only if the corresponding IntData is not set.")]
    [SerializeField] private int _baseDamage;
    [Tooltip("Monetary value of the creep. The base value will be used only if the corresponding IntData is not set.")]
    [SerializeField] private int _baseBounty;
    [Tooltip("Score value of the creep. The base value will be used only if the corresponding IntData is not set.")]
    [SerializeField] private int _baseScore;
    
    [Header("Current Stats")]
    [SerializeField] [ReadOnly] private float _currentHealth;
    [SerializeField] [ReadOnly] private float _currentDamage;
    [SerializeField] [ReadOnly] private float _currentSpeed;
    [SerializeField] [ReadOnly] private float _currentHeight;
    [SerializeField] [ReadOnly] private float _currentRadius;
    [SerializeField] [ReadOnly] private int _currentBounty;
    [SerializeField] [ReadOnly] private int _currentScore;
    
    [Header("Total Stats")]
    [SerializeField] [ReadOnly] private int totalKilled, totalSpawned, totalEscaped;

    private void OnEnable()
    {
        if (!_health) _currentHealth = _baseHealth;
        if (!_damage) _currentDamage = _baseDamage;
        if (!_speed) _currentSpeed = _baseSpeed;
        if (!_bounty) _currentBounty = _baseBounty;
        if (!_score) _currentScore = _baseScore;
    }
    
    public string unitName => _unitName;
    public string type => _type;
    public float height => _height;
    public float radius => _radius;
    public float health
    {
        get => _health ? _health.value : _currentHealth;
        set
        {
            if (_health) _health.value = value;
            else _currentHealth = value;
        }
    }
    public float damage
    {
        get => _damage ? _damage.value : _currentDamage;
        set
        {
            if (_damage) _damage.value = value;
            else _currentDamage = value;
        }
    }
    public float speed
    {
        get => _speed ? _speed.value : _currentSpeed;
        set
        {
            if (_speed) _speed.value = value;
            else _currentSpeed = value;
        }
    }
    public int bounty
    {
        get => _bounty ? _bounty.value : _currentBounty;
        set
        {
            if (_bounty) _bounty.value = value;
            else _currentBounty = value;
        }
    }
    public int score 
    {
        get => _score ? _score.value : _currentScore;
        set
        {
            if (_score) _score.value = value;
            else _currentScore = value;
        }
    }
    
    
    public void IncrementKilledTotal() => totalKilled++;
    public void IncrementSpawnedTotal() => totalSpawned++;
    public void IncrementEscapedTotal() => totalEscaped++;
    public void DecrementKilledTotal() => totalKilled--;
    public void DecrementSpawnedTotal() => totalSpawned--;
    public void DecrementEscapedTotal() => totalEscaped--;

    public void ResetValues()
    {
        totalKilled = 0;
        totalSpawned = 0;
        totalEscaped = 0;
    }
    
    public List<(Action, string)> GetButtonActions()
    {
        return new List<(Action, string)>
        {
            (ResetValues, "Reset Values")
        };
    }
}
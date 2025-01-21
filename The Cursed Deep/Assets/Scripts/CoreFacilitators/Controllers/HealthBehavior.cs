using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using ZPTools.Interface;
using static ZPTools.Utility.UtilityFunctions;
using Random = UnityEngine.Random;

public class HealthBehavior : MonoBehaviour, IDamagable
{
    private struct TextPoolObject
    {
        private readonly GameObject _textObject;
        private readonly TextMesh _textMesh;
        private readonly Color _originalColor;
        private readonly Color _endColor;
        private readonly HealthBehavior _owner;
        private const float AnimationTime = 2f;

        public TextPoolObject(GameObject newTextObject, HealthBehavior owner)
        {
            _textObject = newTextObject;
            _textMesh = AdvancedGetComponent<TextMesh>(_textObject, false);
            _originalColor = _textMesh.color;
            _endColor = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0);
            _owner = owner;
            _textObject.transform.SetParent(_owner.transform);
            
            _textPosition = Vector3.zero;
        }
        
        private Vector3 _textPosition;

        public void SetLocation(Vector3 location)
        {
            if (!_textObject)
            {
                Debug.LogError("Text Object is null.", _owner);
                return;
            }
            if (!_mainCamera) _mainCamera = Camera.main;
            _textPosition = location;
        }
        
        public void ActivateText(string displayText = "")
        {
            if(_textMesh) _textMesh.text = displayText;
            _textObject.SetActive(true);
            _owner.StartCoroutine(WaitForAnimationToEnd());
        }
        
        private Vector3 GetRandomUpwardVector() => new(Random.Range(-1f, 1f), 1, 0);

        private IEnumerator WaitForAnimationToEnd()
        {
            var time = Time.deltaTime;
            var upVector = GetRandomUpwardVector();
            
            while (time < AnimationTime)
            {
                time += Time.deltaTime;
                ;
                _textObject.transform.position = _textPosition + upVector * Time.deltaTime;
                _textObject.transform.rotation = _mainCamera.transform.rotation;
                _textMesh.color = Color.Lerp(_originalColor, _endColor, time / AnimationTime);
                yield return null;
            }
            _textObject.SetActive(false);
        }

        public void SetActive(bool isActive) => _textObject.SetActive(isActive);
        public bool activeInHierarchy => _textObject.activeInHierarchy;
    }
    
    [Header("Health Events")]
    public UnityEvent onHealthGained;
    public UnityEvent onHealthLost;
    public UnityEvent onThreeQuarterHealth;
    public UnityEvent onHalfHealth;
    public UnityEvent onQuarterHealth;
    public UnityEvent onHealthDepleted;
    
    [Header("Health Variables")]
    [SerializeField] [ReadOnly] private float _currentHealth;
    [SerializeField] private FloatData currentHealthData;
    private float _previousCheckHealth;
    [SerializeField] private FloatData _maxHealth;
    
    [Header("Damage Text Variables")]
    [SerializeField] private bool _showDamageDealt;
    [SerializeField] private GameObject _damageTextPrefab;
    private static Camera _mainCamera;
    private List<TextPoolObject> _textObjectPool;
    private Vector3 _textOrigin;
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
    
    private bool _isDead;

    private void OnEnable() 
    {
        _isDead = false;
        if (!_maxHealth) _maxHealth = ScriptableObject.CreateInstance<FloatData>();
        if (maxHealth <= 0) maxHealth = 1;
        if (health <= 0 || health > maxHealth) health = maxHealth;
        _currentHealth = health;
    }

    private void Awake()
    {
        if (!_damageTextPrefab) _showDamageDealt = false;
        _mainCamera ??= Camera.main;
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

    private TextPoolObject GetTextPoolObject()
    {
        _textObjectPool ??= new List<TextPoolObject>();
        foreach (var textObj in _textObjectPool.Where(textObj => !textObj.activeInHierarchy))
        {
            textObj.SetLocation(_textOrigin);
            return textObj;
        }
        var newTextPoolObject = new TextPoolObject(Instantiate(_damageTextPrefab), this);
        newTextPoolObject.SetLocation(_textOrigin);
        newTextPoolObject.SetActive(false);
        _textObjectPool.Add(newTextPoolObject);
        return newTextPoolObject;
    }
    
    public void TakeDamage(IDamageDealer dealer)
    {
        if (_isDead) return;
        var amount = dealer.damage;
        Debug.Log($"{name} took {amount} damage from {dealer}");
        
        if(_showDamageDealt)
        {
            _textOrigin = dealer.hitPoint;
            ShowDamage(amount.ToString());
        }
        if (amount > -1) amount *= -1;
        AddAmountToHealth(amount);
    }
    
    private void ShowDamage(string text)
    {
        if (!_showDamageDealt) return;
        var textObj = GetTextPoolObject();
        textObj.ActivateText(text);
    }
}
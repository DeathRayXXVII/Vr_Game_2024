using System.Collections;
using UI.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class DialoguePurchaseHandler : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField, ReadOnly] private int _currentPlayerCoins;
    [SerializeField] private IntData playerCoins;
    [SerializeField, ReadOnly] private int _cost;
    [SerializeField] private UpgradeData upgradeData;
    [SerializeField] private bool increaseUpgradeLevelOnPurchase = true;
    [SerializeField] private BoolData noMoreStockBool;
    [SerializeField] private DialogueData mainDialogue;
    [SerializeField] private DialogueData emptyStockDialogue;
    [SerializeField] private DialogueActivator _activator;
    [SerializeField] private DialogueResponseEvents responseEvents;
    [SerializeField] private ResponseHandler responseHandler;
    [SerializeField] public DialogueUI dialogueUI;
    [SerializeField] private UnityEvent onPurchase;
    
    private readonly WaitForFixedUpdate _waitFixed = new();

    private int currentPlayerCoins
    {
        get
        {
            _currentPlayerCoins = playerCoins.value;
            return _currentPlayerCoins;
        }
    }

    public int cost
    {
        private get
        {
            _cost = upgradeData != null ? (int)upgradeData.upgradeCost : _cost > 0 ? _cost : 0;
            return _cost;
        }
        set => _cost = value;
    }
    
    private void OnValidate()
    {
#if UNITY_EDITOR
        _cost = cost;
        _currentPlayerCoins = playerCoins ? currentPlayerCoins : 0;
#endif
    }
    
    private void Start()
    {
        CheckStock();
    }
    
    public void CheckStock()
    {
        var hasStock = noMoreStockBool == null || noMoreStockBool.value;
        
        Debug.Log($"Max level reached: {hasStock} for {id}", this);
        _activator?.UpdateDialogueObject(hasStock ? emptyStockDialogue : mainDialogue);
    }
    
    public string Id => id;
    private bool _handlingPurchase;
    public void Purchase(Response response)
    {
        if (_handlingPurchase) return;
        if (playerCoins >= cost)
        {
            _handlingPurchase = true;
            StartCoroutine(PerformPurchase());
            ContinueDialogue(response.PurchaseDialogue);
            CheckStock();
        }
        else
        {
            ContinueDialogue(response.DialogueData);
        }
    }
    
    private IEnumerator PerformPurchase()
    {
        playerCoins -= cost;
        
        onPurchase.Invoke();
        yield return _waitFixed;
        var hasUpgrade = upgradeData != null;
        
        Debug.Log($"Performing purchase for {id} with cost {cost} and has upgrade: {hasUpgrade}", this); 
        if (increaseUpgradeLevelOnPurchase && hasUpgrade)
        {
            upgradeData.IncreaseUpgradeLevel();
            Debug.Log($"Increased upgrade level for {id} to {upgradeData.upgradeLevel}", this);
        }
        
        _handlingPurchase = false;
    }
    
    private void ContinueDialogue(DialogueData dialogueData)
    {
        dialogueUI.ShowDialogue(dialogueData);
    }
}
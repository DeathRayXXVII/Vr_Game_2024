using System.Collections;
using UI.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class DialoguePurchaseHandler : MonoBehaviour
{
    [SerializeField] private string _id;
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
    
    public void CheckStock()
    {
        var hasStock = noMoreStockBool == null || noMoreStockBool.value;
        
        if (_activator == null)
        {
            Debug.LogError($"[ERROR] DialogueActivator is null on '{name}' DialoguePurchaseHandler.", this);
            return;
        }
        
        _activator.UpdateDialogueObject(hasStock ? emptyStockDialogue : mainDialogue);
    }
    
    public string id => _id;
    private bool _handlingPurchase;
    public void Purchase(Response response)
    {
        if (_handlingPurchase) return;
        if (playerCoins >= cost)
        {
            _handlingPurchase = true;
            StartCoroutine(PerformPurchase());
            ContinueDialogue(response.PurchaseDialogue);
        }
        else
        {
            ContinueDialogue(response.DialogueData);
        }
    }
    
    private IEnumerator PerformPurchase()
    {
        playerCoins -= cost;
        
        yield return _waitFixed;
        var hasUpgrade = upgradeData != null;
        
        if (increaseUpgradeLevelOnPurchase && hasUpgrade)
        {
            var upgradeLevel = upgradeData.upgradeLevel;
            upgradeData.IncreaseUpgradeLevel();
            yield return new WaitUntil(() => upgradeData.upgradeLevel > upgradeLevel);
        }
        
        onPurchase.Invoke();
        yield return _waitFixed;
        
        CheckStock();
        yield return _waitFixed;
        
        _handlingPurchase = false;
    }
    
    private void ContinueDialogue(DialogueData dialogueData)
    {
        dialogueUI.ShowDialogue(dialogueData);
    }
}
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

    private int cost
    {
        get
        {
            _cost = (int)upgradeData.upgradeCost;
            return _cost;
        }
    }
    
    private void OnValidate()
    {
#if UNITY_EDITOR
        _cost = upgradeData ? cost : 0;
        _currentPlayerCoins = playerCoins ? currentPlayerCoins : 0;
#endif
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
        }
        else
        {
            Debug.Log("Not enough coins");
            ContinueDialogue(response.DialogueData);
        }
    }
    
    private IEnumerator PerformPurchase()
    {
        playerCoins -= cost;
        
        onPurchase.Invoke();
        yield return _waitFixed;
        var hasUpgrade = upgradeData != null;
        if (increaseUpgradeLevelOnPurchase && hasUpgrade)
        {
            upgradeData.IncreaseUpgradeLevel();
        }

        if (!hasUpgrade)
        {
            Debug.LogWarning("[WARNING] No upgrade data found",this);
        }
        
        _handlingPurchase = false;
    }
    
    private void ContinueDialogue(DialogueData dialogueData)
    {
        dialogueUI.ShowDialogue(dialogueData);
    }
}
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
    [SerializeField] private DialogueResponseEvents responseEvents;
    [SerializeField] private ResponseHandler responseHandler;
    [SerializeField] public DialogueUI dialogueUI;
    [SerializeField] private UnityEvent onPurchase;

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
    public void Purchase(Response response)
    {
        if (playerCoins >= cost)
        {
            Debug.Log($"{playerCoins} - {cost} coins, {playerCoins - cost} coins left");
            playerCoins -= cost;
            ContinueDialogue(response.PurchaseDialogue);
            onPurchase.Invoke();
            
        }
        else
        {
            Debug.Log("Not enough coins");
            ContinueDialogue(response.DialogueData);
        }
    }
    
    private void ContinueDialogue(DialogueData dialogueData)
    {
        dialogueUI.ShowDialogue(dialogueData);
    }
}
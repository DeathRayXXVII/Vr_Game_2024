using UI.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;

public class DialoguePurcheseHandler : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField] private IntData playerCoins;
    [SerializeField] private int cost;
    [SerializeField] private DialogueResponseEvents responseEvents;
    [SerializeField] private ResponseHandler responseHandler;
    [SerializeField] public DialogueUI dialogueUI;
    [SerializeField] private UnityEvent onPurchase;
    
    public string Id => id;
    public void Purchase(Response response)
    {
        if (playerCoins >= cost)
        {
            Debug.Log($"- {cost} coins");
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
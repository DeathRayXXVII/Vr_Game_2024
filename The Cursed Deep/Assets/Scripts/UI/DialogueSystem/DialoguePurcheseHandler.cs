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
    public void Purchase(DialogueData dialogueData)
    {
        if (playerCoins >= cost)
        {
            Debug.Log($"- {cost} coins");
            playerCoins -= cost;
            ContinueDialogue(dialogueData);
            onPurchase.Invoke();
            
        }
        else
        {
            Debug.Log("Not enough coins");
            ContinueDialogue(dialogueData);
        }
    }
    
    private void ContinueDialogue(DialogueData dialogueData)
    {
        Debug.Log("Continuing dialogue");
        dialogueUI.ShowDialogue(dialogueData);
    }
}
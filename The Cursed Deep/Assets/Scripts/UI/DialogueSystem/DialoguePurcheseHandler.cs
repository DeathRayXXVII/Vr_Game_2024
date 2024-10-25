using UI.DialogueSystem;
using UnityEngine;

public class DialoguePurcheseHandler : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField] private IntData playerCoins;
    [SerializeField] private int cost;
    [SerializeField] private DialogueData successDialogue, failDialogue;
    [SerializeField] private DialogueResponseEvents responseEvents;
    [SerializeField] private ResponseHandler responseHandler;
    [SerializeField] public DialogueUI dialogueUI;
    
    public string Id => id;
    
    
    public void Purchase()
    {
        if (playerCoins >= cost)
        {
            Debug.Log($"- {cost} coins");
            playerCoins -= cost;
            ContinueDialogue(successDialogue);
            
        }
        else
        {
            Debug.Log("Not enough coins");
            ContinueDialogue(failDialogue);
        }
    }
    
    private void ContinueDialogue(DialogueData dialogueData)
    {
        Debug.Log("Continuing dialogue");
        dialogueUI.ShowDialogue(dialogueData);
    }
}
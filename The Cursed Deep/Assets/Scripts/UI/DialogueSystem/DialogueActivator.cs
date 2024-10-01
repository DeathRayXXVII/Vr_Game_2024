using UnityEngine;
using UnityEngine.Events;

public class DialogueActivator : MonoBehaviour, IInteractable
{
    public ID id;
    [SerializeField] private DialogueData dialogueData;
    
    public void UpdateDialogueObject(DialogueData dialogueData)
    {
        this.dialogueData = dialogueData;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDBehavior idBehavior) && idBehavior.id == id)
        {
            if (other.TryGetComponent(out PlayerDialogueActivator player))
            {
                player.interactable = this;
                //player.dialogueUI.ShowDialogue(dialogueData);
            }
        }
        // if (other.CompareTag("Player") && other.TryGetComponent(out PlayerDialogueActivator player))
        // {   
        //     //player.interactable = this;
        //     //player.dialogueUI.ShowDialogue(dialogueData);
        //     other.GetComponent<DialogueUI>().ShowDialogue(dialogueData);
        // }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IDBehavior idBehavior) && idBehavior.id == id)
        {
            if (other.TryGetComponent(out PlayerDialogueActivator player))
            {
                player.dialogueUI.typewriterEffect.Stop();
                player.dialogueUI.CloseDialogueBox();
                player.interactable = null;
            }
        }
        
        /*if (other.CompareTag("Player") && other.TryGetComponent(out PlayerDialogueActivator player))
        {
            if (player.interactable is DialogueActivator dialogueActivator && dialogueActivator == this)
            {
                //player.interactable = null;
                //player.dialogueUI.typewriterEffect.Stop();
                //player.dialogueUI.CloseDialogueBox();
                other.GetComponent<DialogueUI>().typewriterEffect.Stop();
                other.GetComponent<DialogueUI>().CloseDialogueBox();
            }
            
        }*/
    }

    public void Interact(PlayerDialogueActivator player)
    {
        foreach (DialogueResponseEvents responseEvents in GetComponents<DialogueResponseEvents>())
        {
            if (responseEvents.DialogueData == dialogueData)
            {
                player.dialogueUI.AddResponseEvents(responseEvents.Events);
                break;
            }
        }
        player.dialogueUI.ShowDialogue(dialogueData);
    }
}

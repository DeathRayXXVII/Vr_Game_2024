using UnityEngine;

public class DialogueActivator : MonoBehaviour, IInteractable
{
    public ID id;
    [SerializeField] private DialogueData dialogueData;
    public PlayerDialogueActivator playerActivator;
    
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
            }
        }
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
    }

    public void OnTrigger()
    {
        if (playerActivator)
        {
            playerActivator.interactable = this;
        }
    }
    
    public void OffTrigger()
    {
        if (!playerActivator) return;
        //playerActivator.dialogueUI.typewriterEffect.Stop();
        //playerActivator.dialogueUI.CloseDialogueBox();
        playerActivator.interactable = null;
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

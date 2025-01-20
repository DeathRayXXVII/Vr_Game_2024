using UI.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;

public class DialogueActivator : MonoBehaviour, IInteractable
{
    public ID id;
    [SerializeField] private GameAction action;
    [SerializeField] private DialogueData dialogueData;
    public PlayerDialogueActivator playerActivator;
    [SerializeField] private UnityEvent onInteract;
    
    public bool interactionAllowed { get; set; } = true;
    
    public void UpdateDialogueObject(DialogueData dData)
    {
        if (dData == null) return;
        this.dialogueData = dData;
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
                player.dialogueUI.CloseDialogueBox(dialogueData);
                player.interactable = null;
            }
        }
    }

    public void OnTrigger()
    {
        if (playerActivator && interactionAllowed)
        {
            playerActivator.interactable = this;
        }
    }
    
    public void OffTrigger()
    {
        if (!playerActivator) return;
        playerActivator.interactable = null;
    }

    public void Interact(PlayerDialogueActivator player)
    {
        if (!interactionAllowed) return;
        onInteract.Invoke();
        if (dialogueData.locked) return;
        dialogueData.Activated();
        foreach (DialogueResponseEvents responseEvents in GetComponents<DialogueResponseEvents>())
        {
            if (responseEvents.DialogueData)
            {
                player.dialogueUI.AddResponseEvents(responseEvents.Events);
                break;
            }
        }
        player.dialogueUI.ShowDialogue(dialogueData);
        dialogueData.FirstDialogueEvent(action);
    }
}
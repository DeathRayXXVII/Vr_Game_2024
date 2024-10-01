using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerDialogueActivator : MonoBehaviour
{
    [SerializeField] private InputActionReference interactAction;
    [Header("Dialogue System")]
    [SerializeField] private DialogueUI dialogue;
    public DialogueUI dialogueUI => dialogue;
    public IInteractable interactable {get; set;}
    
    [SerializeField] private UnityEvent onInteract;

    public void Update()
    {
        if (dialogueUI.IsOpen) return;
        if (interactAction.action.triggered)
        {
            interactable?.Interact(this);
            onInteract.Invoke();
        }
            
    }
}
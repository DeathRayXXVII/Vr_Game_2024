using UnityEngine;

public class PlayerDialogueActivator : MonoBehaviour
{
    [SerializeField] private DialogueUI dialogue;
    public DialogueUI dialogueUI => dialogue;
    public IInteractable interactable {get; set;}

    public void Update()
    {
        interactable?.Interact(this);
    }
}
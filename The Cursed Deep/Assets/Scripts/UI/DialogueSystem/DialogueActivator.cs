using UnityEngine;

public class DialogueActivator : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueData dialogueData;
    public bool requireinput;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {   
            if (!requireinput)
            {
                other.GetComponent<DialogueUI>().ShowDialogue(dialogueData);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!requireinput)
            {
                other.GetComponent<DialogueUI>().typewriterEffect.Stop();
                other.GetComponent<DialogueUI>().CloseDialogueBox();
            }
        }
    }

    public void Interact(GameObject player)
    {
        foreach (DialogueResponseEvents responseEvents in GetComponents<DialogueResponseEvents>())
        {
            if (responseEvents.DialogueData == dialogueData)
            {
                player.GetComponent<DialogueUI>().AddResponseEvents(responseEvents.Events);
                break;
            }
        }

        player.GetComponent<DialogueUI>().ShowDialogue(dialogueData);
    }
}

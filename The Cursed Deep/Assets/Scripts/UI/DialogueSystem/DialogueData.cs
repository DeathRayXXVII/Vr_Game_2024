using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    [SerializeField] [TextArea] private string[] dialogue;
    [SerializeField] private Response[] responses;
    [SerializeField] private UnityEvent onDialogueStart, onDialogueEnd;
    public string[] Dialogue => dialogue;
    
    public bool hasResponses => responses is { Length: > 0 };
    public Response[] Responses => responses;
    
    public void StartDialogue()
    {
        onDialogueStart.Invoke();
    }
    
    public void EndDialogue()
    {
        onDialogueEnd.Invoke();
    }
    
}
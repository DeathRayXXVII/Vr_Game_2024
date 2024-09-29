using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    [SerializeField] [TextArea] private string[] dialogue;
    [SerializeField] private Response[] responses;
    public string[] Dialogue => dialogue;
    
    public bool hasResponses => responses is { Length: > 0 };
    public Response[] Responses => responses;
}
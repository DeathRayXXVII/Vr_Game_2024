using UnityEngine;
using UnityEngine.Events;

namespace UI.DialogueSystem
{
    [CreateAssetMenu(menuName = "Dialogue/DialogueData")]
    public class DialogueData : ScriptableObject
    {
        [Header("Dialogue Data")]
        [SerializeField] private string dialogueName;
        [SerializeField] private GameAction firstAction, lastAction;
        public GameAction purchaseAction;
        [SerializeField] [TextArea] private string[] dialogue;
        [SerializeField] private Response[] responses;
        [SerializeField] private UnityEvent onTrigger, firstTrigger, lastTrigger;

        public string[] Dialogue => dialogue;

        public bool hasResponses => responses is { Length: > 0 };
        public Response[] Responses => responses;

        private void OnEnable()
        {
            if (firstAction == null) return;
            firstAction.Raise += FirstDialogueEvent;
            
            if (lastAction == null) return;
            lastAction.Raise += LastDialogueEvent;
        }

        public void DialogueEvent(GameAction _)
        {
            onTrigger.Invoke();
        }
        
        public void FirstDialogueEvent(GameAction _)
        {
            firstTrigger.Invoke();
        }
        
        public void LastDialogueEvent(GameAction _)
        {
            lastTrigger.Invoke();
        }
    }
}
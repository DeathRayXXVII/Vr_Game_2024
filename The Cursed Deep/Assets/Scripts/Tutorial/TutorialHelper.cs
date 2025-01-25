using UnityEngine;
using UnityEngine.Events;


namespace Tutorial
{
    public class TutorialHelper : MonoBehaviour
    {
        [System.Serializable]
        private struct TutorialData
        {
            public BoolData _tutorialIsActive;
            [System.Serializable]
            public struct Actions
            {
                public string actionName;
                public UnityEvent onActionEvent;
            }
            
            public Actions[] actions;
            
            public void PerformAction(string actionName)
            {
                Debug.Log($"Performing action {actionName} because {_tutorialIsActive.name} is active.");
                foreach (var action in actions)
                {
                    Debug.Log($"Checking action {action.actionName}.");
                    if (action.actionName != actionName) continue;
                    Debug.Log($"Valid action found, performing action {actionName}.");
                    action.onActionEvent.Invoke();
                    return;
                }
            }
        }
        
        [SerializeField] private TutorialData[] _tutorialData;
        
        public void PerformTutorialAction(string actionName)
        {
            foreach (var tutorial in _tutorialData)
            {
                if (!tutorial._tutorialIsActive) continue;
                tutorial.PerformAction(actionName);
            }
        }
    }
}
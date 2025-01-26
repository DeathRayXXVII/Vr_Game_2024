using UnityEngine;
using UnityEngine.Events;


namespace Tutorial
{
    public class TutorialHelper : MonoBehaviour
    {
        public bool allowDebug;
        
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
            
            public void PerformAction(string actionName, Object context = null, bool debugging = false)
            {
                if (debugging)
                    Debug.Log($"[INFO] {_tutorialIsActive.name} is active, performing action {actionName}.", context);
                foreach (var action in actions)
                {
                    if (debugging)
                        Debug.Log($"[INFO] Checking action {action.actionName}.", context);
                    if (action.actionName != actionName) 
                        continue;
                    
                    if (debugging)
                        Debug.Log($"[INFO] Valid action found, performing action {actionName}.", context);
                    action.onActionEvent.Invoke();
                    break;
                }
            }
        }
        
        [SerializeField] private TutorialData[] _tutorialData;

        public void PerformTutorialAction(string actionName)
        {
            foreach (var tutorial in _tutorialData)
            {
                if (!tutorial._tutorialIsActive) 
                    continue;
                
                tutorial.PerformAction(actionName, this, allowDebug);
                break;
            }
        }
    }
}
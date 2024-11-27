using UnityEngine;

namespace UI.DialogueSystem
{
    public class DialogueDataHelper : MonoBehaviour
    {
        [SerializeField] private DialogueData[] _dialogueData;
        
        public void LockAllLockableDialogues()
        {
            foreach (var dialogue in _dialogueData)
            {
                if (!dialogue.playOnlyOncePerGame) continue;
                dialogue.SetLocked(true);
            }
        }
    }
}

using System.Linq;
using UI.DialogueSystem;
using UnityEngine;

namespace Tutorial
{
    public class TutorialHelper : MonoBehaviour
    {
        [SerializeField] private DialogueData[] _dialogueData;
        [SerializeField] private AudioShotData[] _audioData;

        public void LockAllLockableDialogueAndAudio()
        {
            LockAllLockableDialogues();
            LockAllLockableAudio();
        }

        public void LockAllLockableDialogues()
        {
            foreach (var dialogue in _dialogueData.Where(dialogue => dialogue.playOnlyOncePerGame))
            {
                dialogue.locked = true;
            }
        }

        public void LockAllLockableAudio()
        {
            foreach (var audioData in _audioData)
            {
                foreach (var audioShot in audioData.audioShots.Where(audioShot => audioShot.playOnlyOncePerGame))
                {
                    audioShot.SetLocked(true);
                }
            }
        }
    }
}

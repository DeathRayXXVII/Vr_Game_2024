using System;
using System.Linq;
using UI.DialogueSystem;
using UnityEngine;

namespace Tutorial
{
    public class TutorialHelper : MonoBehaviour
    {
        [SerializeField] private DialogueData[] _dialogueData;
        [SerializeField] private AudioShotData[] _audioData;
        
        private bool ValidateArray(System.Array data)
        {
            return data != null && data.Length != 0;
        }

        public void LockAllLockableDialogueAndAudio()
        {
            LockAllLockableDialogues();
            LockAllLockableAudio();
        }

        public void LockAllLockableDialogues()
        {
            if (!ValidateArray(_dialogueData))
            {
                return;
            }
            
            foreach (var dialogue in _dialogueData.Where(dialogue => dialogue.playOnlyOncePerGame))
            {
                dialogue.locked = true;
            }
        }

        public void LockAllLockableAudio()
        {
            if (!ValidateArray(_audioData))
            {
                return;
            }
            
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

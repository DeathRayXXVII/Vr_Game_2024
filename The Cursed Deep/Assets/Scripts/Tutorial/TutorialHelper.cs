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
        [SerializeField] private BoolData[] _tutorialData;
        
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
        
        public void DeactivateTutorialData() => SetActiveTutorialData(-1);
        
        public void SetActiveTutorialData(int activeTutorialIndex)
        {
            if (!ValidateArray(_tutorialData))
            {
                return;
            }
            
            for (var i = 0; i < _tutorialData.Length; i++)
            {
                _tutorialData[i].value = i == activeTutorialIndex;
            }
        }

        private void OnDisable()
        {
            DeactivateTutorialData();
        }
    }
}

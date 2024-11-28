using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ZPTools.Interface;
using ZPTools.Utility;

namespace UI.DialogueSystem
{
    [CreateAssetMenu(menuName = "Dialogue/DialogueData")]
    public class DialogueData : ScriptableObject, IResetOnNewGame, INeedButton
    {
        public bool playOnlyOncePerGame;
        [SerializeField, ReadOnly] private bool _locked;
        
        public void ResetToNewGameValues(int tier = 2)
        {
            if (tier < 2) return;
            _locked = false;
        }

        public bool locked
        {
            get => playOnlyOncePerGame && _locked;
            set => _locked = playOnlyOncePerGame && value;
        }
        
        public void SetLocked(bool lockState)
        {
            if (lockState) playOnlyOncePerGame = true;
            _locked = lockState;
        }
        
        // Will only be true if playOnlyOncePerGame is true
        public void Activated() => locked = true;
        
        [Header("Dialogue Data")]
        [SerializeField] private string dialogueName;
        [SerializeField] private GameAction firstAction, lastAction;
        
        [SerializeField] private StringFactory[] dialogue;
        [SerializeField] private Response[] responses;
        [SerializeField] private UnityEvent firstTrigger, lastTrigger;
        
        public string[] Dialogue
        {
            get
            {
                string[] dialogueStrings = new string[dialogue.Length];
                for (int i = 0; i < dialogue.Length; i++)
                {
                    dialogueStrings[i] = dialogue[i].FormattedString;
                }
                return dialogueStrings;
            }
        }
        
        public int length => dialogue.Length;
        public int lastDialogueIndex => length - 1;
        
        public bool hasResponses => responses is { Length: > 0 };
        public Response[] Responses => responses;

        // private void OnEnable()
        // {
        //     if (firstAction != null) firstAction.RaiseEvent += FirstDialogueEvent;
        //     if (lastAction != null) lastAction.RaiseEvent += LastDialogueEvent;
        // }
        //
        // private void OnDisable()
        // {
        //     if (firstAction != null) firstAction.RaiseEvent -= FirstDialogueEvent;
        //     if (lastAction != null) lastAction.RaiseEvent -= LastDialogueEvent;
        // }
        
        public void FirstDialogueEvent(GameAction _) => firstTrigger?.Invoke();
        public void LastDialogueEvent() => LastDialogueEvent(null);
        public void LastDialogueEvent(GameAction _) => lastTrigger?.Invoke();
        
        public List<(System.Action, string)> GetButtonActions()
        {
            return new List<(System.Action, string)> { (() => {locked = false;}, "Set Locked to False") };
        }
    }
}
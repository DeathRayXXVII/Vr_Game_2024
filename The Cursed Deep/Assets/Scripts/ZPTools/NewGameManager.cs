using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZPTools.Interface;
using static ZPTools.Utility.UtilityFunctions;

namespace ZPTools
{
    public class NewGameManager : MonoBehaviour, INeedButton
    {
        [SerializeField] private GameAction _toNewGameSceneAction;
        public void SetToSceneAction(GameAction action) => _toNewGameSceneAction = action;
        
        private static void ExecuteResetToNewGameValues(IResetOnNewGame obj, int tier = 10) => obj.ResetToNewGameValues(tier);
        public void ResetToNewGame(int tier) => StartCoroutine(ResetToNewGameCoroutine(tier));
        
        private IEnumerator ResetToNewGameCoroutine(int tier)
        {
            var actionCompleted = PerformActionOnInterface((IResetOnNewGame resetObj) =>
                ExecuteResetToNewGameValues(resetObj, tier));
            
            yield return new WaitUntil(() => actionCompleted);
            
            if (Application.isPlaying) _toNewGameSceneAction?.RaiseAction();
        }
        public List<(Action, string)> GetButtonActions()
        {
            return new List<(Action, string)> { (() => ResetToNewGame(2), "Reset to New Game") };
        }
    }
}

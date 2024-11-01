using System;
using System.Collections.Generic;
using UnityEngine;
using ZPTools.Interface;
using static ZPTools.Utility.UtilityFunctions;

namespace ZPTools
{
    public class NewGameManager : MonoBehaviour, INeedButton
    {
        private static void ExecuteResetToNewGameValues(IResetOnNewGame obj, int tier = 2) => obj.ResetToNewGameValues(tier);
        public static void ResetToNewGame(int tier) => PerformActionOnInterface((IResetOnNewGame resetObj) => ExecuteResetToNewGameValues(resetObj, tier));

        public List<(Action, string)> GetButtonActions()
        {
            return new List<(Action, string)> { (() => ResetToNewGame(2), "Reset to New Game") };
        }
    }
}

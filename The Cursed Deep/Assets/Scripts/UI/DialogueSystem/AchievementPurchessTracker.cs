using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class AchievementPurchessTracker : MonoBehaviour
{
    [SerializeField] private int indexCheck;
    [SerializeField] private UpgradeData upgradeData;
    [SerializeField] private GameAction everythingUnlockedAction;
    [SerializeField] private List<BoolData> everythingUnlockedCheck;
    [SerializeField] private UnityEvent onAchUnlock;
    
    public void CheckAchievement()
    {
        if (upgradeData.upgradeLevel >= indexCheck)
        {
            onAchUnlock.Invoke();
        }
    }
    public void EverythingUnlockedCheck()
    {
        if (everythingUnlockedCheck.TrueForAll(x => x.value))
        {
            everythingUnlockedAction.RaiseAction();
            Debug.Log("All values are true.");
        }
        else
        {
            Debug.Log("Not all values are true.");
        }
    }
}

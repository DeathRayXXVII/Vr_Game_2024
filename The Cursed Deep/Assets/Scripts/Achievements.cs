using UnityEngine;

// -- Once we implement Steamworks Uncomment the -- //
// -- following lines to enable Achievements for Steam -- // 
public class Achievements : MonoBehaviour
{
    [SerializeField] private bool isSteamEnabled;
    [SerializeField] private AchievementData achievementData;
    
    public void IsAchievementUnlocked(string id)
    {
        if (isSteamEnabled)
        {
            // var ach = new Steamworks.Data.Achievement(id);
            // Debug.Log($"Achievement {id} status : " + ach.State);
            // if (ach.State)
            // {
            //     Debug.Log("Achievement already unlocked");
            // }
            // else
            // {
            //     UnlockAchievement(id);
            // }
        }
        else
        {
            Debug.Log("Steam is not enabled");
            UnlockAchievement(id);
        }
        
    }

    private void UnlockAchievement(string id)
    {
        if (isSteamEnabled)
        {
            // var ach = new Steamworks.Data.Achievement(id);
            // ach.Trigger();
            // Debug.Log($"Achievement {id} unlocked");
        }
        var ach = achievementData.achievements.Find(achievement => achievement.iD == id);
        if (ach != null)
        {
            if (ach.isUnlocked) return;
            ach.isUnlocked = true;
            Debug.Log($"Achievement {id} unlocked");
        }
        else
        {
            Debug.Log($"Achievement {id} not found");
        }
        
    }
    
    /*public void ResetSteamAchievement(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        ach.Clear();
        Debug.Log($"Achievement {id} cleared");
    }*/
}

using System.Collections.Generic;
using UnityEngine;

namespace Achievements
{
    public class AchievementUIDisplay : MonoBehaviour
    {
        public RectTransform[] panelList;
        public List<AchievementUI> backlog = new List<AchievementUI>();
    
        public GameObject achievementTemplate;
        private AchievementManager achManager;
        

        private Transform GetCurrentPanel() => panelList[(int)achManager.display].transform;
    
        private void Start()
        {
            achManager = AchievementManager.Instance;
        }
        
        public void DisplayStart()
        {
            if (achManager.displayUI == null)
            {
                achManager.displayUI = this;
            }
        }
        
        public void RemoveDisplay()
        {
            achManager.displayUI = null;
        }
    
        public void ScheduleAchievementDisplay(int id)
        {
            var spawned = Instantiate(achievementTemplate).GetComponent<AchievementUI>();
            spawned.achDisplay = this;
            spawned.SetAchievement(achManager.achievementData.achievements[id], achManager.achievementData.achievements[id] as ProgressiveAchievement);
            if (GetCurrentPanel().childCount < achManager.achDisplayNum)
            {
                spawned.transform.SetParent(GetCurrentPanel(), false);
                spawned.gameObject.SetActive(true);
                spawned.StartTimer();
            }
            else
            {
                spawned.gameObject.SetActive(false);
                backlog.Add(spawned);
            }
        }
    
        public void CheckBacklog()
        {
            if (backlog.Count > 0)
            {
                backlog[0].transform.SetParent(GetCurrentPanel(), false);
                backlog[0].gameObject.SetActive(true);
                backlog[0].StartTimer();
                backlog.RemoveAt(0);
            }
        }
    }
}

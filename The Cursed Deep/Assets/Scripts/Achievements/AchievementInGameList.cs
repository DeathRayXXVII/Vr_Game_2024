using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Achievements
{
    public class AchievementInGameList : MonoBehaviour
    {
        [SerializeField] private GameObject achArea;
        [SerializeField] private GameObject achUI;
        [SerializeField] private GameObject achDisplay;
        //[SerializeField] private Dropdown achFilter;
        [SerializeField] private Text achCount;
        [SerializeField] private Text achPercent;
        //[SerializeField] private Scrollbar achScrollbar;
        [SerializeField] private List<GameObject> spawnPoints;

        private bool menuOpen = false;

        private void AddAchievements()
        {
            AchievementManager achManager = AchievementManager.Instance;
            int achievedCount = achManager.GetAchievementCount();

            achCount.text = "" + achievedCount + " / " + achManager.achievementData.achievements.Count;
            achPercent.text = "Complete (" + achManager.GetAchievementPercent() + "%)";

            for (int i = 0; i < achManager.achievementData.achievements.Count && i < spawnPoints.Count; i++)
            {
                AddAchievementToUI(achManager.achievementData.achievements[i], achManager.achievementData.achievements[i] as ProgressiveAchievement, spawnPoints[i]);
            }
        }

        private void AddAchievementToUI(Achievement ach, ProgressiveAchievement progAch, GameObject spawnPoint)
        {
            AchievementUI ui = Instantiate(achUI, spawnPoint.transform.position, Quaternion.identity).GetComponent<AchievementUI>();
            ui.SetAchievement(ach, progAch);
            ui.transform.SetParent(spawnPoint.transform);
            ui.transform.localScale = Vector3.one;
        }

        // public void ChangeFilter()
        // {
        //     AddAchievements(achFilter.options[achFilter.value].text);
        // }

        private void OpenMenu()
        {
            menuOpen = true;
            achDisplay.SetActive(menuOpen);
            AddAchievements();
        }

        private void CloseMenu()
        {
            menuOpen = false;
            achDisplay.SetActive(menuOpen);
        }

        public void ToggleMenu()
        {
            if (menuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }
    }
}
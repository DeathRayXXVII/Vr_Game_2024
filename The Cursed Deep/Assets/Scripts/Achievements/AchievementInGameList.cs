using UnityEngine;
using UnityEngine.UI;

namespace Achievements
{
    public class AchievementInGameList : MonoBehaviour
    {
        [SerializeField] private GameObject achArea;
        [SerializeField] private GameObject achUI;
        [SerializeField] private GameObject achDisplay;
        [SerializeField] private Dropdown achFilter;
        [SerializeField] private Text achCount;
        [SerializeField] private Text achPercent;
        [SerializeField] private Scrollbar achScrollbar;

        private bool menuOpen = false;

        private void AddAchievements(string filter)
        {
            AchievementManager achManager = AchievementManager.Instance;
            int achievedCount = achManager.GetAchievementCount();

            achCount.text = "" + achievedCount + " / " + achManager.achievementData.achievements.Count;
            achPercent.text = "Complete (" + achManager.GetAchievementPercent() + "%)";

            foreach (var ach in achManager.achievementData.achievements)
            {
                if (filter.Equals("All") || (filter.Equals("Achieved") && ach.isUnlocked) ||
                    (filter.Equals("Un-achieved") && !ach.isUnlocked))
                {
                    AddAchievementToUI(ach, ach as ProgressiveAchievement);
                }

                achScrollbar.value = 1;
            }
        }

        private void AddAchievementToUI(Achievement ach, ProgressiveAchievement progAch)
        {
            AchievementUI ui = Instantiate(achUI, new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<AchievementUI>();
            ui.SetAchievement(ach, progAch);
            ui.transform.SetParent(achArea.transform);
        }

        public void ChangeFilter()
        {
            AddAchievements(achFilter.options[achFilter.value].text);
        }

        private void OpenMenu()
        {
            menuOpen = true;
            achDisplay.SetActive(menuOpen);
            AddAchievements("All");
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
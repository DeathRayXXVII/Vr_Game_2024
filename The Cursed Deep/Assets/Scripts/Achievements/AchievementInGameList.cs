using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<string, AchievementUI> achiUIDict = new Dictionary<string, AchievementUI>();

        private void AddAchievements(string filter)
        {
            AchievementManager achManager = AchievementManager.Instance;
            int achievedCount = achManager.GetAchievementCount();

            achCount.text = "" + achievedCount + " / " + achManager.achievementData.achievements.Count;
            achPercent.text = "Complete (" + achManager.GetAchievementPercent() + "%)";

            foreach (var achievement in achManager.achievementData.achievements)
            {
                if (filter.Equals("All") || (filter.Equals("Achieved") && achievement.isUnlocked) || (filter.Equals("Un-achieved") && !achievement.isUnlocked))
                {
                    if (achiUIDict.TryGetValue(achievement.id, out var ui))
                    {
                        // Update existing UI element
                        ui.SetAchievement(achievement, achievement as ProgressiveAchievement);
                    }
                    else
                    {
                        // Create new UI element
                        ui = Instantiate(achUI, new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<AchievementUI>();
                        ui.SetAchievement(achievement, achievement as ProgressiveAchievement);
                        ui.transform.SetParent(achArea.transform, false);
                        ui.transform.localScale = Vector3.one;
                        ui.transform.localPosition = Vector3.zero;
                        achiUIDict[achievement.id] = ui;
                    }
                }
                else
                {
                    if (achiUIDict.TryGetValue(achievement.id, out var ui))
                    {
                        // Hide UI element if it doesn't match the filter
                        ui.gameObject.SetActive(false);
                    }
                }
            }

            // Show only the filtered achievements
            foreach (var kvp in achiUIDict)
            {
                var achievement = achManager.achievementData.achievements.FirstOrDefault(a => a.id == kvp.Key);
                if (achievement != null && ((filter.Equals("All")) ||
                    (filter.Equals("Achieved") && achievement.isUnlocked) ||
                    (filter.Equals("Un-achieved") && !achievement.isUnlocked)))
                {
                    kvp.Value.gameObject.SetActive(true);
                }
            }

            achScrollbar.value = 1;
        }

        private void AddAchievementToUI(Achievement ach, ProgressiveAchievement progAch)
        {
            AchievementUI ui = Instantiate(achUI, new Vector3(0f, 0f, 0f), Quaternion.identity).GetComponent<AchievementUI>();
            ui.SetAchievement(ach, progAch);
            ui.transform.SetParent(achArea.transform);
            achiUIDict[ach.id] = ui;
        }

        public void ChangeFilter()
        {
            AddAchievements(achFilter.options[achFilter.value].text);
        }

        private void OpenMenu()
        {
            menuOpen = true;
            achDisplay.SetActive(true);
        }

        private void CloseMenu()
        {
            menuOpen = false;
            achDisplay.SetActive(false);
            AddAchievements("All");
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
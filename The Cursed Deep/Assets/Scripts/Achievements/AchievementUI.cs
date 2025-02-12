using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Achievements
{
    public class AchievementUI : MonoBehaviour
    {
        [SerializeField] private new Text name;
        [SerializeField] private Text description, progress;
        [SerializeField] private Image icon, overlayIcon;
        [SerializeField] private Slider progressBar;
        [SerializeField] private GameObject hiddenOverlay;
        [SerializeField] private GameObject hiddenOverlay2;
        [SerializeField] private Text hiddenName;
        [HideInInspector] public AchievementUIDisplay achDisplay;
        [SerializeField] private UnityEvent onDisplay;
        
        private ProgressiveAchievement prog;
        private Achievement achievement;
        
        public void SetAchievement(Achievement ach, ProgressiveAchievement progAch)
        {
            if (ach.isHidden && !ach.isUnlocked && hiddenOverlay != null && hiddenOverlay2 != null && hiddenName != null)
            {
                hiddenOverlay.SetActive(true);
                hiddenOverlay2.SetActive(true);
                hiddenName.text = "? ? ?";
            }
            else
            {
                if (name != null)
                    name.text = ach.name;
                if (description != null)
                    description.text = ach.description;
                

                if (ach.lockedOverlay && !ach.isUnlocked)
                {
                    if (overlayIcon != null)
                    {
                        overlayIcon.gameObject.SetActive(true);
                        overlayIcon.sprite = ach.lockedIcon;
                        icon.sprite = ach.unlockedIcon;
                    }
                }
                else
                {
                    icon.sprite = ach.isUnlocked ? ach.unlockedIcon : ach.lockedIcon;
                }

                if (ach.isProgression)
                {
                    float currentProgress = AchievementManager.Instance.showProgress ? progAch.progress : (progAch.progressUpdate * ach.notify);
                    float displayProgress = ach.isUnlocked ? ach.goal : currentProgress;
                    
                    if (ach.isUnlocked)
                    {
                        //progress.text = ach.goal + ach.progressSuffix + " / " + ach.goal + ach.progressSuffix + " (Achieved)";
                        progress.text = "Achieved";
                    }
                    else
                    {
                        progress.text = displayProgress + ach.progressSuffix + " / " + ach.goal + ach.progressSuffix;
                    }
                    progressBar.value = displayProgress/ach.goal;
                }
                else
                {
                    progressBar.value = ach.isUnlocked ? 1 : 0;
                    progress.text = ach.isUnlocked ? "Achieved" : "Not Achieved";
                }
            }
            achievement = ach;
            prog = progAch;
            UpdateUI();
        }
        public void StartTimer()
        {
            StartCoroutine(Wait());
        }
        
        private IEnumerator Wait()
        {
            yield return new WaitForSeconds(AchievementManager.Instance.displayTime);
            onDisplay.Invoke();
            yield return new WaitForSeconds(0.1f);
            achDisplay.CheckBacklog();
            Destroy(gameObject);
        }
        private void UpdateUI()
        {
            name.text = achievement.name;
            if (description != null)
                description.text = achievement.description;
            if (prog != null)
            {
                progressBar.value = prog.progress / prog.goal;
            }
        }
    }
}

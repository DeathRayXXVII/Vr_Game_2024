using System.Collections;
using System.Reflection;
using UnityEngine;
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
        [SerializeField] private Text hiddenName;
        [HideInInspector] public AchievementUIDisplay achDisplay;
        
        private ProgressiveAchievement prog;
        private Achievement achievement;
        
        public void SetAchievement(Achievement ach, ProgressiveAchievement progAch)
        {
            if (ach.isHidden && !ach.isUnlocked)
            {
                hiddenOverlay.SetActive(true);
                hiddenName.text = "???";
            }
            else
            {
                name.text = ach.name;
                description.text = ach.description;

                if (ach.lockedOverlay && !ach.isUnlocked)
                {
                    overlayIcon.gameObject.SetActive(true);
                    overlayIcon.sprite = ach.lockedIcon;
                    icon.sprite = ach.unlockedIcon;
                }
                else
                {
                    icon.sprite = ach.isUnlocked ? ach.unlockedIcon : ach.lockedIcon;
                }

                if (ach.isProgression)
                {
                    float currentProgress = AchievementManager.Instance.showProgress ? progAch.progress : (progAch.progressUpdate * ach.notify);
                    float displayProgress = ach.isUnlocked ? ach.goal : currentProgress;
                    progressBar.maxValue = ach.goal;
                    
                    if (ach.isUnlocked)
                    {
                        progress.text = ach.goal + ach.progressSuffix + " / " + ach.goal + ach.progressSuffix + " (Achieved)";
                    }
                    else
                    {
                        progress.text = displayProgress + ach.progressSuffix + " / " + ach.goal + ach.progressSuffix;
                    }
                    progressBar.value = displayProgress;
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

        // public void UpdateProgress(float progs)
        // {
        //     if (prog != null)
        //     {
        //         prog.progress = progs;
        //         UpdateUI();
        //     }
        // }
      
        public void StartTimer()
        {
            Debug.Log("Starting timer");
            StartCoroutine(Wait());
        }
        
        private IEnumerator Wait()
        {
            yield return new WaitForSeconds(AchievementManager.Instance.displayTime);
            GetComponent<Animator>().SetTrigger("ScaleDown");
            Debug.Log("scaling down");
            yield return new WaitForSeconds(0.1f);
            achDisplay.CheckBacklog();
        }
        private void UpdateUI()
        {
            name.text = achievement.name;
            description.text = achievement.description;
            if (prog != null)
            {
                progressBar.value = prog.progress / prog.goal;
            }
        }
    }
}

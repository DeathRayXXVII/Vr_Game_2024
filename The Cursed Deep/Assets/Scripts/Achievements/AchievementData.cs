using System.Collections.Generic;
using UnityEngine;

namespace Achievements
{
    [CreateAssetMenu(fileName = "AchievementData", menuName = "Achievement Data")]
    public class AchievementData : ScriptableObject
    {
        [SerializeReference, SubclassSelector]
        public List<Achievement> achievements = new List<Achievement>();
    }

    [System.Serializable]
    public abstract class Achievement
    {
        public string id;
        public string name;
        public string description;
        public bool isUnlocked;
        public Sprite lockedIcon;
        public bool lockedOverlay;
        public Sprite unlockedIcon;
        public bool isHidden;
        public bool isProgression;
        public float goal;
        public float notify;
        public string progressSuffix;
        //public GameAction action;
    
        //private void OnEnable() => action.Raise += CheckProgress;
        //private void OnDisable() => action.Raise -= CheckProgress;

        //protected abstract void CheckProgress(GameAction _);
    }

    [System.Serializable]
    public class ProgressiveAchievement : Achievement
    {
        protected ProgressiveAchievement(float newProgress, bool newIsUnlocked)
        {
            progress = newProgress;
            isUnlocked = newIsUnlocked;
        }
        public ProgressiveAchievement() { }
    
        public float progress;
        public int progressUpdate;
    }
    /*public class FloatComparison : Achievement
    {
        public int targetValue;
        
        protected override void CheckProgress(GameAction _)
        {
            if (progress >= targetValue) return;
            progress++;
            if (progress >= targetValue)
            {
                isUnlocked = true;
                Debug.Log($"Achievement {iD} unlocked");
            }
        }
    }*/
}
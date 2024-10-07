using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "Achievement Data")]
public class AchievementData : ScriptableObject
{
    public List<Achievement> achievements;
}

[System.Serializable]
public class Achievement
{
    public string iD;
    public int progress;
    public string description;
    public bool isUnlocked;
}
using UnityEngine;

[CreateAssetMenu (fileName = "LevelData", menuName = "Data/ManagerData/LevelData")]
public class LevelData : ScriptableObject
{
    public IntData currentLevel;

    public int spawnValue => levels[currentLevel.value].spawnValue;
    public int spawnsPerLane => levels[currentLevel.value].spawnsPerLane;

    [System.Serializable]
    public struct Level
    {
        public int spawnsPerLane;
        public int spawnValue;
    }
    
    public Level[] levels;
}
using UnityEngine;

[CreateAssetMenu (fileName = "LevelData", menuName = "Data/ManagerData/LevelData")]
public class LevelData : ScriptableObject
{
    public IntData currentLevel;
    
    public int spawnsPerLane => levels[currentLevel.value].spawnsPerLane;
    public IntData currentSpawnValue;
    public int spawnValue => levels[currentLevel.value].spawnValue;
    
    [System.Serializable]
    public struct Level
    {
        public int spawnsPerLane;
        public int spawnValue;
    }
    
    public Level[] levels;

    private void OnValidate()
    {
        if (!currentLevel) Debug.LogError("Current Level is null. Please assign a value.", this);
        if (!currentSpawnValue) Debug.LogError("Current Spawn Value is null. Please assign a value.", this);
    }
}
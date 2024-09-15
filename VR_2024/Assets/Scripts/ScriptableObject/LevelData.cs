using UnityEngine;

[CreateAssetMenu (fileName = "LevelData", menuName = "Data/ManagerData/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelNum;
    public GameObject level;
    public int spawnHealth;
    public int spawnNum;
}
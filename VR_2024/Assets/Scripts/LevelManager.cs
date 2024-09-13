using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private ShipData shipData;
    public LevelData levelData;
    public int currentLevel;

    private void Start()
    {
        LoadLevel();
    }
    
    private void LoadLevel()
    {
        GameObject shipInstance = Instantiate(shipData.currentShip);
        foreach (GameObject cannonPosition in shipData.cannonPositions)
        {
            for (int i = 0; i < levelData.spawnNum; i++)
            {
                Instantiate(shipData.currentCannons, cannonPosition.transform.position, cannonPosition.transform.rotation, shipInstance.transform);
            }
        }
    }
    
    public void LoadLevelData(LevelData levelsData)
    {
        levelData = levelsData;
    }
}
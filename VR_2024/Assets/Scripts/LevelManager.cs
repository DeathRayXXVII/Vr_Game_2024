using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameCoreData coreData;
    public LevelData levelData;
    public int currentLevel;

    private void Start()
    {
        LoadLevel();
    }
    
    private void LoadLevel()
    {
        // GameObject shipInstance = Instantiate(shipData.currentShip);
    }
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "GameCoreData", menuName = "Data/ManagerData/GameCoreData")]
public class GameCoreData : ScriptableObject
{
    // Instancer that performs the instantiation of the ship
    // all other instancers and spawners are inside the instanced ship making them dependent on this instancer
    public InstancerData shipInstancerData;
    
    // Current health of the ship at max health
    public IntData shipMaxHealth;
    
    // Current health of the ship - Need this to keep track of the ship's health between levels
    public IntData shipCurrentHealth;
    
    // Current respawn time of player's ammo
    public FloatData ammoRespawnTime;
    
    // Current damage of player's ammo
    public IntData ammoDamage;
    
    // This is the current health of the enemy at max health
    //  No current enemy health as we do not need to keep track of the enemy's current health between levels
    public IntData enemyMaxHealth;

    [System.Serializable]
    public struct ShipSelection
    {
        public string selectionName;
        
        // Instantiates a cannon in every lane of the ship dependent on and used in the prefab below
        // Requires the cannon selection's prefab and offset
        public InstancerData cannonInstancerData;
        
        // Ship specific spawner data for ammo
        // Requires the ammo selection's prefab list
        public SpawnerData ammoSpawnerData;
        
        // Ship specific spawner data for enemies
        // Requires the enemy selection's prefab list
        public SpawnerData enemySpawnerData;
        
        // Prefab that determines all other data within this selection
        public PrefabData prefab;
        
        // Number of lanes in the ship
        public int numberOfLanes;
    }

    [System.Serializable]
    public struct CannonSelection
    {
        public string selectionName;
        
        // Prefab that determines all other data within this selection
        public PrefabData prefab;
        
        [System.Serializable]
        public struct CannonPrefabOffset
        {
            // Offset of the cannon from the positions predefined in the ship prefab
            public Vector3Data cannonOffset;
        }
        
        // Array of cannon prefab offsets
        // The index order need to match the offset for the corresponding ship in the ship selection order to work correctly
        public CannonPrefabOffset[] cannonOffsetsByShip;
        
        // Returns the cannon offset for the specific ship at given index
        public Vector3Data GetCannonOffset(int shipIndex)
        {
            return cannonOffsetsByShip[shipIndex].cannonOffset;
        }
    }

    [System.Serializable]
    public struct AmmoSelection
    {
        public string selectionName;
        
        // Prefab List that contains variants of a specific ammo type
        public PrefabDataList prefabVariantList;
    }

    [System.Serializable]
    public struct EnemySelection
    {
        public string selectionName;
        
        // Prefab List that contains variants of a specific enemy type
        public PrefabDataList prefabVariantList;
    }

    private int _currentShipIndex;
    public ShipSelection[] shipSelections;
    
    private int _currentCannonIndex;
    public CannonSelection[] cannonSelections;
    
    private int _currentAmmoIndex;
    public AmmoSelection[] ammoSelections;
    
    private int _currentSEnemyIndex;
    public EnemySelection[] enemySelections;
    
    
    // Public accessors for current selections
    public ShipSelection ship => shipSelections[_currentShipIndex];
    public CannonSelection cannon => cannonSelections[_currentCannonIndex];
    public AmmoSelection ammo => ammoSelections[_currentAmmoIndex];
    public EnemySelection enemy => enemySelections[_currentSEnemyIndex];
    
    public int currentShipIndex { get; set; }
    public int currentCannonIndex { get; set; }
    public int currentAmmoIndex { get; set; }
    
    // Current cannon prefab offset based on cannon prefab and ship prefab if ordered correctly in cannon selection's offset array
    public Vector3Data currentCannonOffset => cannonSelections[_currentCannonIndex].GetCannonOffset(currentShipIndex);
}

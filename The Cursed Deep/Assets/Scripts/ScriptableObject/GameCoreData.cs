using UnityEngine;

[CreateAssetMenu (fileName = "GameCoreData", menuName = "Data/ManagerData/GameCoreData")]
public class GameCoreData : ScriptableObject
{
    // Instancer that performs the instantiation of the ship
    // all other instancers and spawners are inside the instanced ship making them dependent on this instancer
    public InstancerData shipInstancerData;
    
    // Current health of the ship at max health
    [SerializeField] private IntData shipMaxHealth;
    
    // Current health of the ship - Need this to keep track of the ship's health between levels
    [SerializeField] private IntData shipCurrentHealth;
    
    // Current respawn time of player's ammo
    [SerializeField] private FloatData ammoRespawnTime;
    
    // Current player's ammo damage
    [SerializeField] private WeaponData ammoDamage;
    
    // This is the current health of the enemy at max health
    //  No current enemy health as we do not need to keep track of the enemy's current health between levels
    [SerializeField] private IntData enemyMaxHealth;

    [System.Serializable]
    public struct ShipSelection
    {
        [SerializeField] private string selectionName;
        
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
        [SerializeField] private string selectionName;
        
        // Prefab that determines all other data within this selection
        public PrefabData prefab;
        
        [System.Serializable]
        public struct CannonPrefabOffset
        {
            public string targetShip;
            
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
        [SerializeField] private string selectionName;
        
        // Prefab List that contains variants of a specific ammo type
        public PrefabDataList prefabVariantList;
    }

    [System.Serializable]
    public struct EnemySelection
    {
        [SerializeField] private string selectionName;
        
        // Prefab List that contains variants of a specific enemy type
        public PrefabDataList prefabVariantList;
    }

    [SerializeField] [ReadOnly] private int currentShipIndex;
    [SerializeField] private ShipSelection[] shipSelections;
    
    [SerializeField] [ReadOnly] private int currentCannonIndex;
    [SerializeField] private CannonSelection[] cannonSelections;
    
    [SerializeField] [ReadOnly] private int currentAmmoIndex;
    [SerializeField] private AmmoSelection[] ammoSelections;
    
    [SerializeField] [ReadOnly] private int currentSEnemyIndex;
    [SerializeField] private EnemySelection[] enemySelections;
    
    
    // Public accessors for current selections
    private ShipSelection ship => shipSelections[shipIndex];
    private CannonSelection cannon => cannonSelections[currentCannonIndex];
    private AmmoSelection ammo => ammoSelections[currentAmmoIndex];
    private EnemySelection enemy => enemySelections[currentSEnemyIndex];

    public int shipIndex
    {
        get => currentShipIndex;
        set
        {
            if (shipSelections == null || shipSelections.Length == 0)
            {
                Debug.LogError("shipSelections is not initialized or is empty.", this);
                return;
            }

            // Ternary setter clamps the value between 0 and the length of the ship array
            currentShipIndex = value < 0 ? 0 : value > shipSelections.Length - 1 ? shipSelections.Length - 1 : value;
            shipInstancerData.SetPrefabData(ship.prefab);
            ship.cannonInstancerData.SetPrefabOffset(cannonPrefabOffset);
        }
    }

    public int cannonIndex
    {
        get => currentCannonIndex;
        set
        {
            if (cannonSelections == null || cannonSelections.Length == 0)
            {
                Debug.LogError("cannonSelections is not initialized or is empty.", this);
                return;
            }
            
            // Ternary setter clamps the value between 0 and the length of the cannon array
            currentCannonIndex = value < 0 ? 0 : value > cannonSelections.Length - 1 ? cannonSelections.Length - 1 : value;
            ship.cannonInstancerData.SetPrefabData(cannon.prefab);
            ship.cannonInstancerData.SetPrefabOffset(cannonPrefabOffset);
        }
    }
    
    public int ammoIndex
    {
        get => currentAmmoIndex;
        set 
        {
            if (ammoSelections == null || ammoSelections.Length == 0)
            {
                Debug.LogError("ammoSelections is not initialized or is empty.", this);
                return;
            }
            
            // Ternary setter clamps the value between 0 and the length of the ammo array
            currentAmmoIndex = value < 0 ? 0 : value > ammoSelections.Length - 1 ? ammoSelections.Length - 1 : value;
            ship.ammoSpawnerData.SetPrefabDataList(ammo.prefabVariantList);
        }
    }

    public int enemyIndex
    {
        get => currentSEnemyIndex;
        set 
        {
            if (enemySelections == null || enemySelections.Length == 0)
            {
                Debug.LogError("enemySelections is not initialized or is empty.", this);
                return;
            }
            
            // Ternary setter clamps the value between 0 and the length of the enemy array
            currentSEnemyIndex = value < 0 ? 0 : value > enemySelections.Length - 1 ? enemySelections.Length - 1 : value;
            ship.enemySpawnerData.SetPrefabDataList(enemy.prefabVariantList);
        }
    }
    
    // Current cannon prefab offset based on cannon prefab and ship prefab if ordered correctly in cannon selection's offset array
    public Vector3Data cannonPrefabOffset => cannonSelections[currentCannonIndex].GetCannonOffset(shipIndex);

    private void OnValidate()
    {
        if (!shipInstancerData) Debug.LogError("Ship Instancer Data is null. Please assign a value.", this);
        if (!shipMaxHealth) Debug.LogError("Ship Max Health is null. Please assign a value.", this);
        if (!shipCurrentHealth) Debug.LogError("Ship Current Health is null. Please assign a value.", this);
        if (!ammoRespawnTime) Debug.LogError("Ammo Respawn Time is null. Please assign a value.", this);
        if (!ammoDamage) Debug.LogError("Ammo Damage is null. Please assign a value.", this);
        if (!enemyMaxHealth) Debug.LogError("Enemy Max Health is null. Please assign a value.", this);
    }

    public void Reset()
    {
        shipIndex = 0;
        cannonIndex = 0;
        ammoIndex = 0;
        enemyIndex = 0;
    }
}

#if UNITY_EDITOR
using UnityEditor;
#endif

using ShipGame.ScriptObj;
using UnityEngine;

namespace ShipGame.Inventory
{
    [CreateAssetMenu (fileName = "CoreData", menuName = "Data/ManagerData/CoreData")]
    public class CoreData : ScriptableObject
    {
        [SerializeField] private GameGlobals gameGlobals;
        [SerializeField] private LevelData levelData;

        public int currentLevel
        {
            get => levelData.currentLevel;
            set => levelData.currentLevel.value = value;
        }
        public int spawnTotal => levelData.spawnCount * ship.laneCount;
        
        
        // Instancer that performs the instantiation of the ship
        // all other instancers and spawners are inside the instanced ship making them dependent on this instancer
        public InstancerData shipInstancerData;

        [SerializeField] [InspectorReadOnly] private int currentShipIndex;
        [SerializeField] private ShipSelection[] shipSelections;
        
        [SerializeField] [InspectorReadOnly] private int currentCannonIndex;
        [SerializeField] private CannonSelection[] cannonSelections;
        
        [SerializeField] [InspectorReadOnly] private int currentAmmoIndex;
        [SerializeField] private AmmoSelection[] ammoSelections;
        
        [SerializeField] [InspectorReadOnly] private int currentEnemyIndex;
        [SerializeField] private EnemySelection[] enemySelections;
        
        // Public accessors for current selections
        private ShipSelection ship => shipSelections[shipIndex];
        private CannonSelection cannon => cannonSelections[currentCannonIndex];
        private AmmoSelection ammo => ammoSelections[currentAmmoIndex];
        private EnemySelection enemy => enemySelections[currentEnemyIndex];

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
                
                // Index clamped between 0 and the length of the ship array
                currentShipIndex = Mathf.Clamp(value, 0, shipSelections.Length - 1);
                
                // Pass the prefab to the ship's instancer
                shipInstancerData.SetPrefabData(ship.prefab);
                
                // Update the current cannon prefab's offset to correspond with the new ship prefab
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
                
                // Index clamped between 0 and the length of the cannon array
                currentCannonIndex = Mathf.Clamp(value, 0, cannonSelections.Length - 1);
                
                // Pass the prefab to the ship's cannon instancer with its correct offset
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
                
                // Index clamped between 0 and the length of the ammo array
                currentAmmoIndex = Mathf.Clamp(value, 0, ammoSelections.Length - 1);
                
                // Pass the prefab list to the ship's ammo spawner
                ship.ammoSpawnerData.SetPrefabDataList(ammo.prefabVariantList);
            }
        }

        public int enemyIndex
        {
            get => currentEnemyIndex;
            set 
            {
                if (enemySelections == null || enemySelections.Length == 0)
                {
                    Debug.LogError("enemySelections is not initialized or is empty.", this);
                    return;
                }
                
                // Index clamped between 0 and the length of the enemy array
                currentEnemyIndex = Mathf.Clamp(value, 0, enemySelections.Length - 1);
                ship.enemySpawnerData.SetPrefabDataList(enemy.prefabVariantList);
            }
        }
        
        // Current cannon prefab offset based on cannon prefab and ship prefab if ordered correctly in cannon selection's offset array
        public Vector3Data cannonPrefabOffset => cannonSelections[currentCannonIndex].GetCannonOffset(shipIndex);

        private void OnValidate()
        {
            if (!shipInstancerData) Debug.LogError("Ship Instancer Data is null. Please assign a value.", this);
            if (!gameGlobals) Debug.LogError("Game Globals is null. Please assign a value.", this);
            if (!levelData) Debug.LogError("Level Data is null. Please assign a value.", this);
        }

        public void Reset()
        {
            shipIndex = 0;
            cannonIndex = 0;
            ammoIndex = 0;
            enemyIndex = 0;
        }
        
        public void LevelCompleted()
        {
            // Increment the current level
            currentLevel++;
        }
    }
}
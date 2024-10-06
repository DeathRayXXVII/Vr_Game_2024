using ShipGame.ScriptObj;
using UnityEngine;

namespace ShipGame.Inventory
{
    [CreateAssetMenu (fileName = "CoreData", menuName = "Data/ManagerData/CoreData")]
    public class CoreData : ScriptableObject
    {
        [SerializeField] private GameGlobals gameGlobals;
        [SerializeField] private LevelData levelData;

        private int currentLevel
        {
            get => levelData.currentLevel;
            set => levelData.currentLevel.value = value;
        }
        
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
#if UNITY_EDITOR
                    Debug.LogError("shipSelections is not initialized or is empty.", this);
#endif
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
#if UNITY_EDITOR
                    Debug.LogError("cannonSelections is not initialized or is empty.", this);
#endif
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
#if UNITY_EDITOR
                    Debug.LogError("ammoSelections is not initialized or is empty.", this);
#endif
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
#if UNITY_EDITOR
                    Debug.LogError("enemySelections is not initialized or is empty.", this);
#endif
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
#if UNITY_EDITOR
            if (!shipInstancerData) Debug.LogError("Ship Instancer Data is null. Please assign a value.", this);
            if (!gameGlobals) Debug.LogError("Game Globals is null. Please assign a value.", this);
            if (!levelData) Debug.LogError("Level Data is null. Please assign a value.", this);
#endif
        }
        
        public void LevelCompleted() => currentLevel++;
        public void LevelFailed() => ResetGameValues();

        private void ResetGameValues()
        {
            gameGlobals.ResetToNewGameValues();
            currentLevel = 0;
            shipIndex = 0;
            cannonIndex = 0;
            ammoIndex = 0;
            enemyIndex = 0;
        }
        
        private void RandomizeEnemySelection() => currentEnemyIndex = Random.Range(0, enemySelections.Length);
        
        public void SetGameVariables()
        {
            RandomizeEnemySelection();
            
            gameGlobals.shipHealth.value = ship.health + gameGlobals.upgradeHealthLevel;
            gameGlobals.ammoDamage.damage = cannon.damage + ammo.damage;
            
            gameGlobals.enemyLaneActiveLimit.value = levelData.laneActiveLimit;
            gameGlobals.enemySpawnCount.value = levelData.spawnCount * ship.laneCount;
            gameGlobals.spawnRateMin.value = levelData.spawnRateMin;
            gameGlobals.spawnRateMax.value = levelData.spawnRateMax;
            
            enemy.SetHealth(levelData.spawnBaseHealth + enemy.selectionHealth);
            enemy.SetDamage(levelData.spawnBaseDamage + enemy.selectionDamage);
            enemy.SetSpeed(enemy.selectionSpeed);
            enemy.SetBounty(levelData.spawnBounty + enemy.selectionBounty);
            enemy.SetScore(levelData.spawnScore + enemy.selectionScore);
            
#if UNITY_EDITOR
            Debug.Log(
                "-----Game Variables-----\n" +
                $"Player Speed: {gameGlobals.playerSpeed}\n" +
                $"Player Score: {gameGlobals.playerScore}\n" +
                "\n" +
                $"Ship Index: {shipIndex}\n" +
                $"Ship Health: {gameGlobals.shipHealth}\n" +
                "\n" +
                $"Ammo Index: {ammoIndex}\n" +
                $"Ammo Damage: {gameGlobals.ammoDamage.damage}\n" +
                $"Ammo Respawn Time: {gameGlobals.ammoRespawnTime}\n" +
                "\n" +
                $"Cannon Index: {cannonIndex}\n" +
                "\n" +
                $"Enemy Index: {enemyIndex}\n" +
                $"Lane Active Limit: {gameGlobals.enemyLaneActiveLimit}\n" +
                $"Spawn Rate MIN: {gameGlobals.spawnRateMin}\n" +
                $"Spawn Rate MAX: {gameGlobals.spawnRateMax}\n" +
                $"Enemy Spawn Count: {gameGlobals.enemySpawnCount}\n" +
                $"Enemy Health: {enemy.health}\n" +
                $"Enemy Damage: {enemy.damage}\n" +
                $"Enemy Speed: {enemy.speed}\n" +
                $"Enemy Bounty: {enemy.bounty}\n" +
                $"Enemy Score: {levelData.spawnScore}\n" +
                "\n"
            );
#endif
        }
    }
}
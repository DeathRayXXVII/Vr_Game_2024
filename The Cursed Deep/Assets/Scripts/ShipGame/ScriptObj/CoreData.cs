using System;
using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu (fileName = "CoreData", menuName = "Data/ManagerData/CoreData")]
    public class CoreData : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] internal bool allowDebug;
#endif
        [SerializeField] private GameGlobals gameGlobals;
        [SerializeField] private LevelData levelData;
        [SerializeField] private ShipData ship;
        [SerializeField] private CannonData cannon;
        [SerializeField] private AmmoData ammo;
        [SerializeField] private EnemyData enemy;

        private int currentLevel
        {
            get => levelData.currentLevel;
            set 
            {
                levelData.currentLevel = value;
                SetLevelData();
            }
        }
        
        public InstancerData shipInstancerData => ship.shipInstancerData;

        public int shipIndex
        {
            get => ship.selectionIndex;
            set
            {
                ship.selectionIndex = value;
                SetShipData();
            }
        }
        
        // Current cannon prefab offset based on cannon prefab and ship prefab if ordered correctly in cannon selection's offset array
        private Vector3Data cannonPrefabOffset => cannon.GetCannonOffset(shipIndex);
        public int cannonIndex
        {
            get => cannon.selectionIndex;
            set
            {
                cannon.selectionIndex = value;
                SetCannonData();
            }
        }
        
        public int ammoIndex
        {
            get => ammo.selectionIndex;
            set 
            {
                ammo.selectionIndex = value;
                SetAmmoData();
            }
        }

        public int enemyIndex
        {
            get => ammo.selectionIndex;
            set 
            {
                ammo.selectionIndex = value;
                SetEnemyData();
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!ship) Debug.LogError("Ship Selections is null. Please assign a value.", this);
            if (!gameGlobals) Debug.LogError("Game Globals is null. Please assign a value.", this);
            if (!levelData) Debug.LogError("Level Data is null. Please assign a value.", this);
#endif
        }
        
        public void LevelCompleted() => currentLevel++;
        public void LevelFailed() => ResetGameValues();

        public void ResetGameValues()
        {
            gameGlobals.ResetToNewGameValues();
            currentLevel = 1;
            shipIndex = 0;
            cannonIndex = 0;
            ammoIndex = 0;
            enemyIndex = 0;
        }
        
        private void SetPlayerData()
        {
            gameGlobals.playerSpeed.value = 2.0f;
        }
        
        private void SetShipData()
        {
            gameGlobals.shipHealth.value = ship.health; // + gameGlobals.upgradeHealth;
            gameGlobals.enemySpawnCount.value = levelData.spawnCount * ship.numberOfLanes;
            ship.SetCannonPrefabData(cannon.prefab);
            ship.SetAmmoSpawnCount();
        }
        
        private void SetLevelData()
        {
            gameGlobals.enemyLaneActiveLimit.value = levelData.laneActiveLimit;
            gameGlobals.enemySpawnCount.value = levelData.spawnCount * ship.numberOfLanes;
            gameGlobals.spawnRateMin.value = levelData.spawnRateMin;
            gameGlobals.spawnRateMax.value = levelData.spawnRateMax;
            
            enemy.SetHealth(levelData.spawnBaseHealth + enemy.selectionHealth);
            enemy.SetDamage(levelData.spawnBaseDamage + enemy.selectionDamage);
            enemy.SetBounty(levelData.spawnBounty + enemy.selectionBounty);
            enemy.SetScore(levelData.spawnScore + enemy.selectionScore);
        }
        
        private void SetAmmoData()
        {
            gameGlobals.ammoDamage.damage = cannon.damage + ammo.damage; // + gameGlobals.upgradeDamage;
            gameGlobals.ammoRespawnRate.value = ammo.respawnRate;
            
            // Pass the prefab list to the ship's ammo spawner
            ship.SetAmmoPrefabDataList(ammo.prefabList);
        }
        
        private void SetCannonData()
        {
            gameGlobals.ammoDamage.damage = cannon.damage + ammo.damage; // + gameGlobals.upgradeDamage;
                
            // Pass the prefab to the ship's cannon instancer with its correct offset
            ship.SetCannonPrefabData(cannon.prefab);
            ship.SetCannonPrefabOffset(cannonPrefabOffset);
        }
        
        private void SetEnemyData()
        {
            // Pass the prefab list to the ship's enemy spawner
            ship.SetEnemyPrefabDataList(enemy.prefabList);
            
            enemy.SetHealth(levelData.spawnBaseHealth + enemy.selectionHealth);
            enemy.SetDamage(levelData.spawnBaseDamage + enemy.selectionDamage);
            enemy.SetSpeed(enemy.selectionSpeed);
            enemy.SetBounty(levelData.spawnBounty + enemy.selectionBounty);
            enemy.SetScore(levelData.spawnScore + enemy.selectionScore);
        }

        private void Setup(bool attemptedReload)
        {
            if (attemptedReload)
            {
                levelData.LoadOnStartup();
                ship.LoadOnStartup();
                ammo.LoadOnStartup();
                cannon.LoadOnStartup();
                enemy.LoadOnStartup();
            }
            SetPlayerData();
            SetShipData();
            SetLevelData();
            SetAmmoData();
            SetCannonData();
            SetEnemyData();
        }

        public void Setup()
        {
            try
            {
                Setup(false);
            }
            catch (IndexOutOfRangeException e)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Attempted to load game data and got an index out of range exception. Attempting to initialize data and reload. Error: " + e.Message);
#endif
                Setup(true);
            }
            
#if UNITY_EDITOR
            if (allowDebug)
            {
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
                    $"Ammo Respawn Time: {gameGlobals.ammoRespawnRate}\n" +
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
            }
#endif
        }
    }
}
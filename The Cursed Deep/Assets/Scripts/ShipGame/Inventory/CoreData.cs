using ShipGame.ScriptObj;
using UnityEngine;

namespace ShipGame.Inventory
{
    [CreateAssetMenu (fileName = "CoreData", menuName = "Data/ManagerData/CoreData")]
    public class CoreData : ScriptableObject
    {
        [SerializeField] private GameGlobals gameGlobals;
        [SerializeField] private LevelData levelData;
        [SerializeField] private ShipData ship;
        [SerializeField] private CannonData cannon;
        [SerializeField] private AmmoData ammo;
        [SerializeField] private EnemyData enemy;

        private int currentLevel
        {
            get => levelData.currentLevel;
            set => levelData.currentLevel.value = value;
        }
        
        public InstancerData shipInstancerData => ship.shipInstancerData;

        public int shipIndex
        {
            get => ship.selectionIndex;
            set
            {
                ship.selectionIndex = value;
                ship.SetCannonPrefabData(cannon.prefab);
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
                
                // Pass the prefab to the ship's cannon instancer with its correct offset
                ship.SetCannonPrefabData(cannon.prefab);
                ship.SetCannonPrefabOffset(cannonPrefabOffset);
            }
        }
        
        public int ammoIndex
        {
            get => ammo.selectionIndex;
            set 
            {
                ammo.selectionIndex = value;
                
                // Pass the prefab list to the ship's ammo spawner
                ship.SetAmmoPrefabDataList(ammo.prefabList);
            }
        }

        public int enemyIndex
        {
            get => ammo.selectionIndex;
            set 
            {
                ammo.selectionIndex = value;
                
                // Pass the prefab list to the ship's enemy spawner
                ship.SetEnemyPrefabDataList(enemy.prefabList);
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

        private void ResetGameValues()
        {
            gameGlobals.ResetToNewGameValues();
            currentLevel = 0;
            shipIndex = 0;
            cannonIndex = 0;
            ammoIndex = 0;
            enemyIndex = 0;
        }
        
        
        public void SetGameVariables()
        {
            enemy.RandomizeEnemySelection();

            gameGlobals.shipHealth.value = ship.health; // + gameGlobals.upgradeHealth;
            gameGlobals.ammoDamage.damage = cannon.damage + ammo.damage;
            
            gameGlobals.enemyLaneActiveLimit.value = levelData.laneActiveLimit;
            gameGlobals.enemySpawnCount.value = levelData.spawnCount * ship.numberOfLanes;
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
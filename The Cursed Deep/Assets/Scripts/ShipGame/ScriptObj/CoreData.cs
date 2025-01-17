using UnityEngine;
using ZPTools.Interface;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu (fileName = "CoreData", menuName = "Data/ManagerData/CoreData")]
    public class CoreData : ScriptableObject, IResetOnNewGame
    {
        [SerializeField] internal bool allowDebug;
        [SerializeField] private GameAction _playerInitializePositionAction;
        [SerializeField] private GameGlobals gameGlobals;
        [SerializeField] private LevelData levelData;
        [SerializeField] private ShipData ship;
        [SerializeField] private CannonData cannon;
        [SerializeField] private AmmoData ammo;
        [SerializeField] private EnemyData enemy;
        [SerializeField] private BossData boss;

        public GameAction playerInitializePositionAction
        {
            get => _playerInitializePositionAction;
            set => _playerInitializePositionAction = value;
        }

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
            get => gameGlobals.FightingBoss() ? boss.selectionIndex : enemy.selectionIndex;
            set 
            {
                if (gameGlobals.FightingBoss())
                {
                    boss.selectionIndex = value;
                }
                else
                {
                    enemy.selectionIndex = value;
                }
                SetEnemyData(gameGlobals.FightingBoss());
            }
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!gameGlobals) Debug.LogError("[ERROR] Game Globals is null. Please assign a value.", this);
            if (!levelData) Debug.LogError("[ERROR] Level Data is null. Please assign a value.", this);
            if (!ship) Debug.LogError("[ERROR] Ship Selections is null. Please assign a value.", this);
            if (!cannon) Debug.LogError("[ERROR] Cannon Selections is null. Please assign a value.", this);
            if (!ammo) Debug.LogError("[ERROR] Ammo Selections is null. Please assign a value.", this);
            if (!enemy) Debug.LogError("[ERROR] Enemy Selections is null. Please assign a value.", this);
#endif
        }

        private void OnEnable()
        {
            levelData.LoadError += LoadLevelData;
            ship.LoadError += LoadShipData;
            cannon.LoadError += LoadCannonData;
            ammo.LoadError += LoadAmmoData;
            enemy.LoadError += LoadEnemyData;
            boss.LoadError += LoadEnemyData;
        }

        private void OnDisable()
        {
            levelData.LoadError -= LoadLevelData;
            ship.LoadError -= LoadShipData;
            cannon.LoadError -= LoadCannonData;
            ammo.LoadError -= LoadAmmoData;
            enemy.LoadError -= LoadEnemyData;
            boss.LoadError -= LoadEnemyData;
        }
        
        public void LevelCompleted()
        {
            currentLevel++;
            // SetEnemyData();
#if UNITY_EDITOR
            PrintGameVariables("Called From LevelCompleted");
#endif
        }
        public void LevelFailed() => ResetToNewGameValues();

        public void ResetToNewGameValues(int tier = 1)
        {
            if (tier < 1) return;
            gameGlobals.ResetGameVariables();
            
            currentLevel = 1;
            shipIndex = 0;
            cannonIndex = 0;
            ammoIndex = 0;
            enemyIndex = 0;
        }
        
        private void SetPlayerData()
        {
            gameGlobals.SetPlayerSpeed();
        }
        
        private void SetShipData()
        {
            gameGlobals.SetShipHealth(ship.health);
            gameGlobals.SetEnemySpawnCount(levelData.spawnCount, ship.numberOfLanes);
                
            ship.SetCannonPrefabData(cannon.prefab);
            ship.SetAmmoSpawnCount();
        }
        
        private void SetLevelData()
        {
            gameGlobals.SetSpawnLaneActiveLimit(levelData.laneActiveLimit);
            gameGlobals.SetEnemySpawnCount(levelData.spawnCount, ship.numberOfLanes);
            gameGlobals.SetSpawnRates(levelData.spawnRateMin, levelData.spawnRateMax);
            
            enemy.SetHealth(levelData.spawnBaseHealth + enemy.selectionHealth);
            enemy.SetDamage(levelData.spawnBaseDamage + enemy.selectionDamage);
            enemy.SetBounty(levelData.spawnBounty + enemy.selectionBounty);
            enemy.SetScore(levelData.spawnScore + enemy.selectionScore);
        }
        
        private void SetAmmoData()
        {
            gameGlobals.SetPlayerDamage(cannon.damage, ammo.damage);
            gameGlobals.SetAmmoRespawnRate(ammo.respawnRate);
            
            // Pass the prefab list to the ship's ammo spawner
            ship.SetAmmoPrefabDataList(ammo.prefabList);
        }
        
        private void SetCannonData()
        {
            gameGlobals.SetPlayerDamage(cannon.damage, ammo.damage);
                
            // Pass the prefab to the ship's cannon instancer with its correct offset
            ship.SetCannonPrefabData(cannon.prefab);
            ship.SetCannonPrefabOffset(cannonPrefabOffset);
        }
        
        private void SetEnemyData(bool isBoss)
        {
            if (isBoss)
            {
                // Pass the prefab list to the ship's enemy spawner
                ship.SetEnemyPrefabDataList(boss.prefabList);

                boss.SetHealth(levelData.spawnBaseHealth + boss.selectionHealth);
                boss.SetDamage(levelData.spawnBaseDamage + boss.selectionDamage);
                boss.SetSpeed(levelData.spawnBaseDamage + boss.selectionSpeed);
                boss.SetBounty(levelData.spawnBounty + boss.selectionBounty);
                boss.SetScore(levelData.spawnScore + boss.selectionScore);
            }
            else
            {
                // Pass the prefab list to the ship's enemy spawner
                ship.SetEnemyPrefabDataList(enemy.prefabList);

                enemy.SetHealth(levelData.spawnBaseHealth + enemy.selectionHealth);
                enemy.SetDamage(levelData.spawnBaseDamage + enemy.selectionDamage);
                enemy.SetSpeed(enemy.selectionSpeed);
                enemy.SetBounty(levelData.spawnBounty + enemy.selectionBounty);
                enemy.SetScore(levelData.spawnScore + enemy.selectionScore);
            }
        }

        public void HandleEnemyDefeated()
        {
            if (!gameGlobals)
            {
                Debug.LogError("[ERROR] GameGlobals is null. Please assign a value.", this);
                return;
            }
            var enemyIsBoss = gameGlobals.FightingBoss();
            
            gameGlobals.playerCoins += enemyIsBoss ? boss.bounty: enemy.bounty;
            gameGlobals.playerScore += enemyIsBoss ? boss.score: enemy.score;
            
            gameGlobals.UpdateCoinVisual();
            gameGlobals.UpdateEnemyCountVisual();
        }

        private void LoadLevelData() => levelData.LoadOnStartup();
        private void LoadShipData() => ship.LoadOnStartup();
        private void LoadAmmoData() => ammo.LoadOnStartup();
        private void LoadCannonData() => cannon.LoadOnStartup();
        private void LoadEnemyData()
        {
            if(gameGlobals.FightingBoss())
            {
                boss.LoadOnStartup();
                return;
            }
            enemy.LoadOnStartup();
        }
        private void LoadAllData()
        {
            LoadLevelData();
            LoadShipData();
            LoadAmmoData();
            LoadCannonData();
            LoadEnemyData();
        }

        private void Setup(bool attemptingReload)
        {
            if (attemptingReload)
            {
                LoadAllData();
                return;
            }
            SetPlayerData();
            SetShipData();
            SetLevelData();
            SetAmmoData();
            SetCannonData();
            SetEnemyData(gameGlobals.FightingBoss());
        }

        public void Setup()
        {
            try
            {
                Setup(false);
            }
#if UNITY_EDITOR
            catch (System.IndexOutOfRangeException e)
            {
                if (allowDebug) Debug.LogWarning(
                    "[WARNING] Attempted to load game data and got an index out of range exception. Attempting to initialize data and reload. Error: " +
                    e.Message);
#else
            catch (System.IndexOutOfRangeException)
            {
#endif
                Setup(true);
            }
#if UNITY_EDITOR
            catch (System.NullReferenceException e)
            {
                if (allowDebug) Debug.LogWarning(
                    "[WARNING] Attempted to load game data and got a null reference exception. Attempting to initialize data and reload. Error: " +
                    e.Message);
#else
            catch (System.NullReferenceException)
            {
#endif
                Setup(true);
            }
#if UNITY_EDITOR
            catch (System.Exception e)
            {
                if (allowDebug) Debug.LogWarning(
                    "[WARNING] Attempted to load game data and got an unexpected exception type. Attempting to initialize data and reload. Error: " +
                    e.Message);
#else
            catch (System.Exception)
            {
#endif
                Setup(true);
            }
#if UNITY_EDITOR
            if (allowDebug) Debug.Log("[DEBUG] Successfully setup game data.", this);
            PrintGameVariables("Called From Setup");
#endif
        }

        private void PrintGameVariables(string header = "")
        {
#if UNITY_EDITOR
            if (allowDebug)
            {
                Debug.Log(
                    "[DEBUG] -----Game Variables-----\n" +
                    $"{(header == "" ? "" : $" ---{header}--- \n")}" +
                    $"\n___ Level Index [{currentLevel}] ___\n" +
                    $"Player Speed: {gameGlobals.playerSpeed}\n" +
                    $"Player Score: {gameGlobals.playerScore}\n" +
                    $"\n___ Ship Index [{shipIndex}] ___\n" +
                    $"Ship Health: {gameGlobals.shipHealth}\n" +
                    $"\n___ Ammo Index: [{ammoIndex}] ___\n" +
                    $"Ammo Damage: {gameGlobals.playerDamage}\n" +
                    $"Ammo Respawn Time: {gameGlobals.ammoRespawnRate}\n" +
                    $"\n___ Cannon Index: [{cannonIndex}] ___\n" +
                    $"\n___ Enemy Index: [{enemyIndex}] ___\n" +
                    $"Lane Active Limit: {gameGlobals.enemyLaneActiveLimit}\n" +
                    $"Spawn Rate MIN: {gameGlobals.spawnRateMin}\n" +
                    $"Spawn Rate MAX: {gameGlobals.spawnRateMax}\n" +
                    $"Enemy Spawn Count: {gameGlobals.enemySpawnCount}\n" +
                    $"Enemy Health: {enemy.health}\n" +
                    $"Enemy Damage: {enemy.damage}\n" +
                    $"Enemy Speed: {enemy.speed}\n" +
                    $"Enemy Bounty: {enemy.bounty}\n" +
                    $"Enemy Score: {enemy.score}\n" +
                    "\n"
                    , this
                );
            }
#endif
            // ignored if not in editor
        }
    }
}
using UnityEngine;
using ZPTools.Interface;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "GameGlobals", menuName = "Data/ManagerData/GameGlobals")]
    public class GameGlobals : ScriptableObject, IResetOnNewGame
    {
        [Header ("New Game Base Values:")]
        public float playerSpeedBase;
        public float ammoRespawnRateBase;
        
        [Header("Player Data:")]
        public FloatData playerSpeed;
        public IntData playerScore;
        public IntData playerMoney;
        
        [Header("Ship Data:")] 
        public FloatData shipHealth;
        
        [Header("Ammo Data:")]
        public WeaponData ammoDamage;
        public FloatData ammoRespawnRate;

        [Header("Enemy Data:")]
        public IntData enemySpawnCount;
        public IntData enemyLaneActiveLimit;
        public FloatData spawnRateMin;
        public FloatData spawnRateMax;
        
        [Header("Upgrade Data:")]
        public FloatData upgradeDamage;
        public FloatData upgradeHealth;
        public FloatData upgradeSpeed;
        public FloatData ammoRespawnTimeUpgrade;
        
        private void OnEnable()
        {
            System.Diagnostics.Debug.Assert(playerSpeed != null, "Player Speed is null");
            System.Diagnostics.Debug.Assert(playerScore != null, "Player Score is null");
            System.Diagnostics.Debug.Assert(playerMoney != null, "Player Money is null");
            System.Diagnostics.Debug.Assert(shipHealth != null, "Ship Health is null");
            System.Diagnostics.Debug.Assert(ammoDamage != null, "Ammo Damage is null");
            System.Diagnostics.Debug.Assert(ammoRespawnRate != null, "Ammo Respawn Rate is null");
            System.Diagnostics.Debug.Assert(enemySpawnCount != null, "Enemy Spawn Count is null");
            System.Diagnostics.Debug.Assert(enemyLaneActiveLimit != null, "Enemy Lane Active Limit is null");
            System.Diagnostics.Debug.Assert(spawnRateMin != null, "Spawn Rate Min is null");
            System.Diagnostics.Debug.Assert(spawnRateMax != null, "Spawn Rate Max is null");
            // System.Diagnostics.Debug.Assert(damageUpgradeLevel != null, "Damage Upgrade Level is null");
            // System.Diagnostics.Debug.Assert(upgradeDamage != null, "Upgrade Damage is null");
            // System.Diagnostics.Debug.Assert(healthUpgradeLevel != null, "Health Upgrade Level is null");
            // System.Diagnostics.Debug.Assert(upgradeHealth != null, "Upgrade Health is null");
            // System.Diagnostics.Debug.Assert(speedUpgradeLevel != null, "Speed Upgrade Level is null");
            // System.Diagnostics.Debug.Assert(upgradeSpeed != null, "Upgrade Speed is null");
            // System.Diagnostics.Debug.Assert(upgradeAmmoRespawnTimeLevel != null, "Upgrade Ammo Respawn Time is null");
            // System.Diagnostics.Debug.Assert(ammoRespawnTimeUpgrade != null, "Ammo Respawn Time Upgrade is null");
            
            if (playerSpeedBase <= 0) playerSpeedBase = 1;
            if (playerSpeed.value < playerSpeedBase) playerSpeed.Set(playerSpeedBase);
            
            if (playerScore.value < 0) playerScore.Set(0);
            if (playerMoney.value < 0) playerMoney.Set(0);
            
            if (shipHealth.value < 0) shipHealth.Set(0);
            
            if (ammoDamage.damage < 1) ammoDamage.damage = 1;
            if (ammoRespawnRateBase < 1) ammoRespawnRateBase = 1;
            if (ammoRespawnRate.value < 0) ammoRespawnRate.Set(ammoRespawnRateBase);
            
            if (enemySpawnCount.value < 0) enemySpawnCount.Set(0);
            if (enemyLaneActiveLimit.value < 1) enemyLaneActiveLimit.Set(1);
            
            if (spawnRateMin.value < 1) spawnRateMin.Set(1);
            if (spawnRateMax.value < 1) spawnRateMax.Set(1);
            
            // if (damageUpgradeLevel.value < 0) damageUpgradeLevel.Set(0);
            // if (upgradeDamage.value < 0) upgradeDamage.Set(0);
            //
            // if (healthUpgradeLevel.value < 0) healthUpgradeLevel.Set(0);
            // if (upgradeHealth.value < 0) upgradeHealth.Set(0);
            //
            // if (speedUpgradeLevel.value < 0) speedUpgradeLevel.Set(0);
            // if (upgradeSpeed.value < 0) upgradeSpeed.Set(0);
            //
            // if (upgradeAmmoRespawnTimeLevel.value < 0) upgradeAmmoRespawnTimeLevel.Set(0);
            // if (ammoRespawnTimeUpgrade.value < 0) ammoRespawnTimeUpgrade.Set(0);
        }
        
        public void ResetToNewGameValues(int tier = 1)
        {
            if (tier < 1) return;
            playerSpeed.Set(playerSpeedBase);
            playerScore.Set(0);
            playerMoney.Set(0);
            
            shipHealth.Set(0);
            
            ammoDamage.damage = 1;
            ammoRespawnRate.Set(ammoRespawnRateBase);
            
            enemySpawnCount.Set(0);
            enemyLaneActiveLimit.Set(1);
            spawnRateMin.Set(1);
            spawnRateMax.Set(1);
            
            // Upgrades have their own upgradeData scriptable object that handles their reset
        }
    }
}

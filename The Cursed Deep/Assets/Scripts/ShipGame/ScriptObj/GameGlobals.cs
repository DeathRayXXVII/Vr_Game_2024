using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "GameGlobals", menuName = "Data/ManagerData/GameGlobals")]
    public class GameGlobals : ScriptableObject
    {
        [Header("Game Data:")]
        public FloatData playerSpeed;
        public IntData playerScore;
        
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
        
        // [Header("Upgrade Data:")]
        // public FloatData upgradeHealth;
        // public FloatData upgradeDamage;
        // public FloatData upgradeSpeed;
        // public IntData upgradeAmmoRespawnTime;
        
        public void ResetToNewGameValues()
        {
            // playerSpeed.Set(0);
            // playerScore.Set(0);
            //
            // upgradeHealth.Set(0);
            // upgradeSpeedLevel.Set(0);
            // upgradeCannonDamageLevel.Set(0);
            // upgradeAmmoDamageLevel.Set(0);
            // upgradeAmmoRespawnTimeLevel.Set(0);
        }
    }
}

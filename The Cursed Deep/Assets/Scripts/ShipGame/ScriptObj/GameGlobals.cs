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
        public IntData shipHealth;
        
        [Header("Ammo Data:")]
        public WeaponData ammoDamage;
        public FloatData ammoRespawnTime;

        [Header("Enemy Data:")]
        public IntData enemySpawnCount;
        public IntData enemyLaneActiveLimit;
        public FloatData spawnRateMin;
        public FloatData spawnRateMax;
        
        [Header("Upgrade Data:")]
        public IntData upgradeHealthLevel;
        public IntData upgradeSpeedLevel;
        public IntData upgradeCannonDamageLevel;
        public IntData upgradeAmmoDamageLevel;
        public IntData upgradeAmmoRespawnTimeLevel;
        
        // All missing new game values not included below are handled elsewhere 
        [Header("New Game Values:")]
        [SerializeField] private int _playerSpeed;
        [SerializeField] private float _ammoRespawnTime;
        
        public void ResetToNewGameValues()
        {
            playerSpeed.Set(_playerSpeed);
            playerScore.Set(0);
            ammoRespawnTime.Set(_ammoRespawnTime);
            
            upgradeHealthLevel.Set(0);
            upgradeSpeedLevel.Set(0);
            upgradeCannonDamageLevel.Set(0);
            upgradeAmmoDamageLevel.Set(0);
            upgradeAmmoRespawnTimeLevel.Set(0);
        }
    }
}

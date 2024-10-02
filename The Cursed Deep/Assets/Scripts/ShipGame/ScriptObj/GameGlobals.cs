using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "GameGlobals", menuName = "Data/ManagerData/GameGlobals")]
    public class GameGlobals : ScriptableObject
    {
        [Header("Game Data:")]
        [SerializeField] private FloatData playerSpeed;
        [SerializeField] private IntData playerScore;
        
        [Header("Upgrade Data:")]
        public IntData upgradeHealthLevel;
        public IntData upgradeSpeedLevel;
        public IntData upgradeCannonDamageLevel;
        public IntData upgradeAmmoDamageLevel;
        public IntData upgradeAmmoRespawnTimeLevel;
        
        [Header("Ship Data:")] 
        public IntData shipHealth;
        
        [Header("Ammo Data:")]
        public WeaponData ammoDamage;
        [SerializeField] private FloatData ammoRespawnTime;

        [Header("Enemy Data:")]
        public IntData enemySpawnCount;
        public IntData enemyLaneActiveLimit;
        public IntData enemyHealth;
        public IntData enemyDamage;
        public IntData enemySpeed;
        public IntData enemyBounty;
        public IntData enemyScore;
        
        // All missing new game values not included below are handled elsewhere 
        [Header("New Game Values:")]
        [SerializeField] private int _speed;
        [SerializeField] private float _ammoRespawnTime;
        
        public void ResetToNewGameValues()
        {
            playerSpeed.Set(_speed);
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

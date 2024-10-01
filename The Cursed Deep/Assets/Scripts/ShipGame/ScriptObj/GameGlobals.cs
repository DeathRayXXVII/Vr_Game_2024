using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "GameGlobals", menuName = "Data/ManagerData/GameGlobals")]
    public class GameGlobals : ScriptableObject
    {
        [Header("Game Data:")]
        [SerializeField] private IntData playerMaxHealth;
        [SerializeField] private IntData playerHealth;
        [SerializeField] private IntData playerDamage;
        [SerializeField] private IntData playerSpeed;
        
        [Header("Ship Data:")] 
        [SerializeField] private IntData shipMaxHealth;
        [SerializeField] private IntData shipHealth;
        
        [Header("Cannon Data:")]
        [SerializeField] private IntData cannonDamage;
        
        [Header("Ammo Data:")]
        [SerializeField] private WeaponData ammoDamage;
        [SerializeField] private FloatData ammoRespawnTime;

        [Header("Enemy Data:")]
        [SerializeField] private IntData enemyHealth;
        [SerializeField] private IntData enemyLaneActiveLimit;
    }
}

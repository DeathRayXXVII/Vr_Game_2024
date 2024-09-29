using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "GameGlobals", menuName = "Data/ManagerData/GameGlobals")]
    public class GameGlobals : ScriptableObject
    {
        [Header("Ship Data")] 
        [SerializeField] private IntData shipMaxHealth;
        [SerializeField] private IntData shipCurrentHealth;  // Need this to keep track of the ship's health between levels
        
        [Header("Cannon Data")]
        [SerializeField] private IntData cannonDamage;
        
        [Header("Ammo Data")]
        [SerializeField] private WeaponData ammoDamage;
        [SerializeField] private FloatData ammoRespawnTime;

        [Header("Enemy Data")]
        [SerializeField] private IntData enemyHealth;
        [SerializeField] private IntData enemyLaneActiveLimit;
    }
}

using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "GameGlobals", menuName = "Data/ManagerData/GameGlobals")]
    public class GameGlobals : ScriptableObject
    {
        /* Each of the following variables are used to store the current state of the game
         * Logic:
         * player speed = speedUpgrade[upgradeIndex] (Speed is set by the UpgradeData script)
         * player damage = cannonUpgrade[upgradeLevel] + ammoUpgrade[upgradeLevel] + damageUpgrade[upgradeLevel]
         * player score = playerScore
         * player coins = playerMoney
         *
         * shipHealth (player health) = shipData[selection] + healthUpgrade[upgradeLevel]
         *
         * ammo respawn rate = ammoRespawnRate
         *
         * enemy spawn count = levelData[level].enemySpawnCount
         * enemy lane active limit = levelData[level].enemyLaneActiveLimit
         * spawn rate min = levelData[level].spawnRateMin
         * spawn rate max = levelData[level].spawnRateMax
         */

        [Header("Gameplay Control:")] 
        [SerializeField] private BoolData _levelSelected;

        [SerializeField] private BoolData _fightingBoss;

        public bool IsLevelSelected() => _levelSelected;
        public bool FightingBoss() => _fightingBoss;

        [Header("Player Data:")] 
        [SerializeField] private FloatData _playerSpeed;

        [SerializeField] private UpgradeData _speedUpgrade;
        [SerializeField] private WeaponData _playerDamage;
        [SerializeField] private IntData _playerScore;
        [SerializeField] private IntData _playerCoins;

        public float playerSpeed
        {
            get => _playerSpeed ?? 0f;
            private set => _playerSpeed.Set(value);
        }

        private float speedUpgrade => (float)_speedUpgrade.upgradeValue;

        public void SetPlayerSpeed() => playerSpeed = speedUpgrade;

        public float playerDamage
        {
            get => _playerDamage ? _playerDamage.damage : 0f;
            private set => _playerDamage.damage = value;
        }

        public void SetPlayerDamage(float cannonDamage, float ammoDamage)
        {
            playerDamage = cannonDamage + ammoDamage + damageUpgrade;
        }

        public int playerScore
        {
            get => _playerScore ?? 0;
            set => _playerScore?.Set(value);
        }

        public int playerCoins
        {
            get => _playerCoins ?? 0;
            set => _playerCoins?.Set(value);
        }

        [Header("Ship Data:")] 
        [SerializeField] private FloatData _currentHealth;
        [SerializeField] private FloatData _shipHealth;
        [SerializeField] private UpgradeData _healthUpgrade;

        public float currentHealth
        { get => _currentHealth ?? 0f; private set => _currentHealth.Set(value); }

        public float shipHealth
        { get => _shipHealth ?? 0f; private set => _shipHealth.Set(value); }

        private float healthUpgrade => (float)_healthUpgrade.upgradeValue;

        public void SetShipHealth(float health, bool updateVisuals = true)
        {
            shipHealth = health + healthUpgrade;
            currentHealth = shipHealth;
            
            if(updateVisuals) UpdateHealthVisual();
        }

        [Header("Cannon & Ammo Data:")] 
        [SerializeField] private UpgradeData _damageUpgrade;

        [SerializeField] private UpgradeData _ammoRespawnUpgrade;
        [SerializeField] private FloatData _ammoRespawnRate;

        public float ammoRespawnRate
        { get => _ammoRespawnRate ?? 0f; private set => _ammoRespawnRate.Set(value); }

        private float ammoRespawnUpgrade => (float)_ammoRespawnUpgrade.upgradeValue;
        public void SetAmmoRespawnRate(float rate) => ammoRespawnRate = rate + ammoRespawnUpgrade;

        private float damageUpgrade => (float)_damageUpgrade.upgradeValue;

        [Header("Enemy Data:")]
        [SerializeField] private IntData _enemySpawnCount;

        [SerializeField] private IntData _enemyLaneActiveLimit;
        [SerializeField] private FloatData _spawnRateMin;
        [SerializeField] private FloatData _spawnRateMax;
            
        public int enemySpawnCount
        { get => _enemySpawnCount ?? 0; private set => _enemySpawnCount.Set(value); }

        public void SetEnemySpawnCount(int baseCount, int laneCount)
        {
            enemySpawnCount = baseCount * laneCount;
            UpdateEnemyCountVisual();
        }

        public int enemyLaneActiveLimit
        { get => _enemyLaneActiveLimit ?? 0; private set => _enemyLaneActiveLimit.Set(value);}

        public void SetSpawnLaneActiveLimit(int limit) => enemyLaneActiveLimit = limit;

        public float spawnRateMin
        { get => _spawnRateMin ?? 0f; private set => _spawnRateMin.Set(value); }

        public float spawnRateMax
        { get => _spawnRateMax ?? 0f; private set => _spawnRateMax.Set(value); }

        public void SetSpawnRates(float min, float max)
        {
            spawnRateMin = min;
            spawnRateMax = max;
        }
        
        [Header("Game Actions:")] 
        [SerializeField] private GameAction _healthVisualUpdateAction;
        [SerializeField] private GameAction _coinVisualUpdateAction;
        [SerializeField] private GameAction _enemyCountVisualUpdateAction;
        
        public void UpdateHealthVisual() => _healthVisualUpdateAction?.RaiseAction();
        public void UpdateCoinVisual() => _coinVisualUpdateAction?.RaiseAction();
        public void UpdateEnemyCountVisual() => _enemyCountVisualUpdateAction?.RaiseAction();
        
        public void ResetGameVariables()
        {
            System.Diagnostics.Debug.Assert(_levelSelected, "Level Selected is null");
            System.Diagnostics.Debug.Assert(_fightingBoss, "Fighting Boss is null");
            _levelSelected.Set(0);
            _fightingBoss.Set(0);

            System.Diagnostics.Debug.Assert(_playerScore, "Player Score is null");
            System.Diagnostics.Debug.Assert(_playerCoins, "Player Money is null");
            _playerScore.Set(0);
            _playerCoins.Set(0);

            System.Diagnostics.Debug.Assert(_playerSpeed, "Player Speed is null");
            System.Diagnostics.Debug.Assert(_speedUpgrade, "Speed Upgrade is null");
            System.Diagnostics.Debug.Assert(_playerDamage, "Player Damage is null");
            _playerDamage.damage = 1f;
            
            
            System.Diagnostics.Debug.Assert(_shipHealth, "Ship Health is null");
            System.Diagnostics.Debug.Assert(_healthUpgrade, "Health Upgrade is null");

            System.Diagnostics.Debug.Assert(_damageUpgrade, "Damage Upgrade is null");
            _damageUpgrade.SetUpgradeLevel(0);
            
            System.Diagnostics.Debug.Assert(_ammoRespawnRate, "Ammo Respawn Rate is null");

            System.Diagnostics.Debug.Assert(_enemySpawnCount, "Enemy Spawn Count is null");
            System.Diagnostics.Debug.Assert(_enemyLaneActiveLimit, "Enemy Lane Active Limit is null");
            System.Diagnostics.Debug.Assert(_spawnRateMin, "Spawn Rate Min is null");
            System.Diagnostics.Debug.Assert(_spawnRateMax, "Spawn Rate Max is null");
        }
    }
}

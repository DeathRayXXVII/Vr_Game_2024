using UnityEngine;
using UnityEngine.Events;

public class PooledObjectBehavior : MonoBehaviour
{
    public UnityEvent onSpawn;
    
    private SpawnManager _spawnManager;
    private SpawnerData.Spawner _spawner;
    private bool _allowDebug;
    private bool _spawned;
    private bool _justInstantiated;
    private bool _respawnTriggered;
    private bool _beingDestroyed;
    
    [SerializeField] private FloatData timeToRespawn;

    private void Awake() => _justInstantiated = true;

    public void Setup(SpawnManager spawnManager, ref SpawnerData.Spawner spawner, ref bool allowDebug)
    {
        _spawnManager = spawnManager;
        _spawner = spawner;
        _allowDebug = allowDebug;
    }

    public void TriggerRespawn()
    {
        if (_respawnTriggered) return;
        _respawnTriggered = true;
        
        if (!_spawnManager)
        {
            Debug.LogWarning($"SpawnManager is null {name} SpawnedObjectBehavior.", this);
            return;
        }
        _spawnManager.SetSpawnDelay(timeToRespawn ? timeToRespawn : 1);
        
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (_justInstantiated)
        {
            _justInstantiated = false;
            return;
        }
        _spawned = true;
        _respawnTriggered = false;
        onSpawn.Invoke();
    }

    public void InvalidateDeath()
    {
        _spawned = false;
        _spawnManager?.NotifyPoolObjectInvalidDeath(ref _spawner);
    }

    private void OnDisable()
    {
        if (!_spawned || _beingDestroyed) return;
        
        if (_allowDebug)
            Debug.Log($"OnDisable of {name} called from {_spawner.spawnerID}. Respawning: {_respawnTriggered}", this);
        
        _spawnManager?.NotifyPoolObjectDisabled(ref _spawner, _respawnTriggered);
        _spawned = false;
    }

    private void OnDestroy()
    {
        // Stops errors from being thrown on closing the game        
        _beingDestroyed = true;
        _spawned = false;
    }
}
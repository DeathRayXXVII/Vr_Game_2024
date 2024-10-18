using UnityEngine;
using UnityEngine.Events;

public class PooledObjectBehavior : MonoBehaviour
{
    public UnityEvent onSpawn;
    
    private SpawnManager _spawnManager;
    private SpawnerData.Spawner _spawner;
    private bool _allowDebug, _spawned, _justInstantiated, _respawnTriggered, _beingDestroyed;
    
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
            Debug.LogWarning($"SpawnManager is null {name} SpawnedObjectBehavior.");
            return;
        }
        _spawnManager.SetSpawnDelay(timeToRespawn ? timeToRespawn : 1);
        _spawnManager.NotifyPoolObjectDisabled(ref _spawner);
        
        _spawnManager.StartSpawn(1, true);
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
        _spawnManager.NotifyPoolObjectInvalidDeath(ref _spawner);
    }

    private void OnDisable()
    {
        if (_beingDestroyed) return;
        if (_allowDebug) Debug.Log($"OnDisable of {name} called from {_spawner.spawnerID}.");
        if (!_spawned || _respawnTriggered) return;
        _spawnManager.NotifyPoolObjectDisabled(ref _spawner);
        _spawned = false;
    }

    private void OnDestroy()
    {
        // Stops errors from being thrown on closing the game        
        _beingDestroyed = true;
        _spawned = false;
    }
}
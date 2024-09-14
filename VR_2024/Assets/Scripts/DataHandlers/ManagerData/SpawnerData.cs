using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SpawnManager")]
[CreateAssetMenu (fileName = "SpawnerData", menuName = "Data/ManagerData/SpawnerData")]
public class SpawnerData : ScriptableObject
{
    [SerializeField] private bool allowDebug;
    
    public IntData activeCount;
    public PrefabDataList prefabList;

    [System.Serializable]
    public class Spawner
    {
        public string spawnerID;
        public int activeLimit = 1;
        public TransformData spawnLocation, pathingTarget;
        private int _currentSpawnCount;

        public int GetAliveCount() { return _currentSpawnCount; }
        public void IncrementCount() { _currentSpawnCount++; }
        public void DecrementCount() { _currentSpawnCount--; }
        public void ResetCount() { _currentSpawnCount = 0; }
    }
    
    public List<Spawner> spawners = new();
    private readonly List<Spawner> _availableSpawners = new();

    private void Awake()
    {
        if (activeCount == null) Debug.LogError("Missing IntData for activeCount on SpawnerData" + name);
        if (prefabList == null) Debug.LogError("Missing PrefabDataList for prefabList on SpawnerData" + name);
    }

    public void ResetSpawnerData()
    {
        activeCount.value = 0;
        foreach (var spawner in spawners)
        {
            spawner.ResetCount();
        }
    }

    internal Spawner GetInactiveSpawner()
    {
        if (allowDebug) Debug.Log($"Checking for available spawners.\nPotential Spawners: {spawners.Count}");
        if (spawners.Count <= 0) return null;
        _availableSpawners.Clear();
        
        foreach (var spawner in spawners)
        {
            var currentSpawnerActiveCount = spawner.GetAliveCount();
            if (currentSpawnerActiveCount < spawner.activeLimit)
            {
                _availableSpawners.Add(spawner);
            }
            if (allowDebug) Debug.Log($"Spawner: {spawner.spawnerID} has {currentSpawnerActiveCount} active.");
        }
        if (allowDebug) Debug.Log($"Found {_availableSpawners.Count} Available Spawners.");
        var output = (_availableSpawners.Count == 0) ? null: _availableSpawners[Random.Range(0, _availableSpawners.Count)];
        if (allowDebug) Debug.Log($"Selected Spawner: {output?.spawnerID}.");
        return output;
    }
    
    public void HandleSpawnRemoval(ref Spawner spawnerID)
    {
        if (allowDebug) Debug.Log($"Handling removal of spawn from {spawnerID.spawnerID}.");
        foreach (var spawner in spawners)
        {
            if (spawner != spawnerID) continue;
            spawner.DecrementCount();
        }
    }
}
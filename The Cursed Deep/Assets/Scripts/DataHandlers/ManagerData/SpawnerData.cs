using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SpawnManager")]
[CreateAssetMenu (fileName = "SpawnerData", menuName = "Data/ManagerData/SpawnerData")]
public class SpawnerData : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField] private bool allowDebug;
#endif
    
    public IntData numToSpawn;
    public IntData activeCount;
    public IntData globalLaneActiveLimit;
    public PrefabDataList prefabList;

    public int spawnCount
    {
        get
        {
            if (!numToSpawn)
            {
#if UNITY_EDITOR
                Debug.Log($"numToSpawn is null on {name}. Creating new IntData with value {spawners.Count}.", this);
#endif
                numToSpawn = CreateInstance<IntData>();
                numToSpawn.value = spawners.Count;
            }
            return numToSpawn.value;
        }
        set => numToSpawn.value = value;
    }

    [System.Serializable]
    public class Spawner
    {
        public string spawnerID;
        public int laneActiveLimitAdjustment;
        public TransformData spawnLocation, pathingTarget;
        private int _currentSpawnCount;
        
        public int GetActiveLimit(int globalActiveLimit) { return globalActiveLimit + laneActiveLimitAdjustment; }
        public int GetAliveCount() { return _currentSpawnCount; }
        public void IncrementCount() { _currentSpawnCount++; }
        public void DecrementCount() { _currentSpawnCount--; }
        public void ResetCount() { _currentSpawnCount = 0; }
    }
    
    public List<Spawner> spawners = new();
    private readonly List<Spawner> _availableSpawners = new();

    private void Awake()
    {
#if UNITY_EDITOR
        if (!activeCount) Debug.LogError("Missing IntData for activeCount on SpawnerData" + name, this);
        if (!prefabList) Debug.LogError("Missing PrefabDataList for prefabList on SpawnerData" + name, this);
#endif
    }
    public void ResetSpawnerData()
    {
        activeCount.value = 0;
        foreach (var spawner in spawners)
        {
            spawner.ResetCount();
        }
    }
    
    public void SetPrefabDataList(PrefabDataList data)
    {
        prefabList = data;
    }
    
    internal int GetSpawnerActiveLimit(Spawner spawner)
    {
        return spawner.GetActiveLimit(globalLaneActiveLimit.value);
    }

    internal Spawner GetInactiveSpawner()
    {
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Checking for available spawners.\nPotential Spawners: {spawners.Count}", this);
#endif
        if (spawners.Count <= 0) return null;
        _availableSpawners.Clear();
        
        foreach (var spawner in spawners)
        {
            var currentSpawnerActiveCount = spawner.GetAliveCount();
            if (currentSpawnerActiveCount < GetSpawnerActiveLimit(spawner))
            {
                _availableSpawners.Add(spawner);
            }
#if UNITY_EDITOR
            if (allowDebug) Debug.Log($"Spawner: {spawner.spawnerID} has {currentSpawnerActiveCount} active.", this);
#endif
        }
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Found {_availableSpawners.Count} Available Spawners.", this);
#endif
        var output = (_availableSpawners.Count == 0) ? null: _availableSpawners[Random.Range(0, _availableSpawners.Count)];
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Selected Spawner: {output?.spawnerID}.", this);
#endif
        return output;
    }
    
    public void HandleSpawnRemoval(ref Spawner spawnerID)
    {
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Handling removal of spawn from {spawnerID.spawnerID}.");
#endif
        foreach (var spawner in spawners)
        {
            if (spawner != spawnerID) continue;
            spawner.DecrementCount();
        }
    }
}
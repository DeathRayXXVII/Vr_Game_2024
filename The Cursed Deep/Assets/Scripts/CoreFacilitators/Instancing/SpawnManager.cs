using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static ZPTools.Utility.UtilityFunctions;
using ZPTools.Interface;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour, INeedButton
{
    private bool _destroying, _pooling;
    private bool allowDebug => spawnerData.allowDebug;

    public SpawnerData spawnerData;
    
    private GameObject _parentObject;
    private PrefabDataList _prefabSet;
    private List<GameObject> _pooledObjects;
    
    private int _poolSize;
    private int poolSize
    {
        get
        {
            var totalPoolSize = 0;
            foreach (var spawner in spawnerData.spawners)
            {
                totalPoolSize += spawnerData.GetSpawnerActiveLimit(spawner);
            }

            return totalPoolSize;
        }
    }

    private float _spawnDelay = 0.5f;

    private WaitForSecondsRealtime _waitForSpawnDelay;
    private readonly WaitForSeconds _waitLoadBuffer = new(0.5f);
    private readonly WaitForSeconds _immediateLoadBuffer = new(0.1f);
    private readonly WaitForFixedUpdate _wffu = new();
    private Coroutine _lateStartRoutine,
        _delaySpawnRoutine,
        _spawnRoutine,
        _poolCreationRoutine,
        _spawnWaitingRoutine;

    [SerializeField] private UnityEvent onSpawn, onSpawningComplete, onFinalSpawnDefeated;

    private void Awake()
    {
        spawnerData.ResetSpawnerData();
        _parentObject = new GameObject($"SpawnedObjects_{name}");

        _poolSize = poolSize;
        
        SetSpawnDelay();
        spawnerData.GenerateSpawnRates();

#if UNITY_EDITOR
        if (!spawnerData)
        {
            Debug.LogError("SpawnerData not found in " + name, this);
            return;
        }
#endif
        spawnerData.currentTotalCountToSpawn = spawnerData.originalTotalCountToSpawn;
        _prefabSet = spawnerData.prefabList;
        _poolCreationRoutine ??= StartCoroutine(DelayPoolCreation());
    }

    private IEnumerator DelayPoolCreation()
    {
        if (_pooling) yield break;
        _pooling = true;
        yield return _waitLoadBuffer;
        _parentObject.transform.SetParent(transform);
        yield return _wffu;
        ProcessPool();
        yield return _wffu;
        _poolCreationRoutine = null;
        _pooling = false;
    }
    
    private bool usePriority => spawnerData.usePriority;

    private void ProcessPool()
    {
        _pooledObjects ??= new List<GameObject>();
        var iterationCount = _poolSize - _pooledObjects.Count;
        if (iterationCount <= 0) return;
        
        var totalPriority = _prefabSet.GetPriority();

        for (var i = 0; i < iterationCount; i++)
        {
            var randomNumber = Random.Range(0, totalPriority);
            var sum = 0;
            foreach (var _ in _prefabSet.prefabDataList)
            {
                var objData = _prefabSet.GetRandomPrefabData();
                sum += objData.priority;
                if (randomNumber >= sum && usePriority) continue;
                var obj = Instantiate(objData.prefab);
                AddToPool(obj);
                break;
            }
        }
    }

    private void AddToPool(GameObject obj)
    {
        var spawnBehavior = obj.GetComponent<PooledObjectBehavior>();
        if (!spawnBehavior) obj.AddComponent<PooledObjectBehavior>();

        _pooledObjects.Add(obj);
        obj.transform.SetParent(_parentObject.transform);
        obj.SetActive(false);
    }

    private void SetSpawnDelay() => _waitForSpawnDelay = new WaitForSecondsRealtime(_spawnDelay);
    public void SetSpawnDelay(float newDelay)
    {
        newDelay = ToleranceCheck(_spawnDelay, newDelay);
        if (newDelay < 0) return;
        _spawnDelay = newDelay;
        _waitForSpawnDelay = new WaitForSecondsRealtime(_spawnDelay);
        SetSpawnDelay();
    }
    
    public void StartSpawn(int amount, bool asynchronous = false)
    {
        if (_spawnRoutine != null || amount < 1) return;
        spawnerData.originalTotalCountToSpawn = amount;
        spawnerData.currentTotalCountToSpawn = amount;
        StartSpawn(asynchronous);
    }

    public void StartSpawn(bool asynchronous)
    {
        if (asynchronous)
        {
            if (spawnerData.currentTotalCountToSpawn <= 0) spawnerData.currentTotalCountToSpawn = spawnerData.originalTotalCountToSpawn;
            StartCoroutine(DelaySpawn(true));
        }
        else StartSpawn();
    }
    
    public void StartSpawn()
    {
        if (_spawnRoutine != null) return;
        if (spawnerData.currentTotalCountToSpawn <= 0) spawnerData.currentTotalCountToSpawn = spawnerData.originalTotalCountToSpawn;
        _delaySpawnRoutine ??= StartCoroutine(DelaySpawn());
    }

    public void ImmediateSpawn()
    {
        if (_spawnRoutine != null) return;
        spawnerData.spawnedCount = 0;
        _spawnRoutine ??= StartCoroutine(Spawn(true));
    }
    
    public void StopSpawn()
    {
        if (_spawnRoutine == null) return;
        StopCoroutine(_spawnRoutine);
        spawnerData.spawningComplete = true;
        _spawnRoutine = null;
    }

    private IEnumerator DelaySpawn(bool asynchronous = false)
    {
        spawnerData.spawnedCount = 0;
        spawnerData.GenerateSpawnRates();
        yield return _wffu;
        yield return _waitForSpawnDelay;
        if (!asynchronous) _spawnRoutine ??= StartCoroutine(Spawn());
        else
        {
            StartCoroutine(Spawn());
            yield break;
        }
        yield return _wffu;
        _delaySpawnRoutine = null;
    }
    
    private IEnumerator Spawn(bool immediate = false)
    {
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Starting Spawn on {name} with data from {spawnerData.name}: {spawnerData.canSpawn}", this);
        if (allowDebug) Debug.Log($"currentTotalCountToSpawn: {spawnerData.currentTotalCountToSpawn}\noriginalTotalCountToSpawn: {spawnerData.originalTotalCountToSpawn}\n" +
                                  $"spawnedCount: {spawnerData.spawnedCount}\nactiveCount: {spawnerData.activeCount}\namountLeftToSpawn: {spawnerData.amountLeftToSpawn}", this);
#endif
        yield return _waitLoadBuffer;
        
        while(_pooling) yield return _waitLoadBuffer;
        
        while (spawnerData.canSpawn)
        {
            var waitTime = immediate ? (YieldInstruction) _immediateLoadBuffer : spawnerData.GetWaitForSpawnRate();
            SpawnerData.Spawner spawner = spawnerData.GetInactiveSpawner();
            
#if UNITY_EDITOR
            if (allowDebug)
            {
                Debug.Log($"Spawning Info...\nTotal Spawns Currently Active Count: {spawnerData.activeCount}\nTotal To Spawn: {spawnerData.originalTotalCountToSpawn}\nNum Left: {spawnerData.amountLeftToSpawn}\n" +
                          $"PoolSize: {_poolSize}\nPooledObjects: {_pooledObjects.Count}\nspawners: {spawnerData.spawners.Count}\nspawnRate: {waitTime}", this);
                Debug.Log((spawner == null) ? "No Spawners Available" : $"Spawner: {spawner.spawnerID}", this);
            }
#endif
            yield return _wffu;
            
            if (spawner == null)
            {
#if UNITY_EDITOR
                if (allowDebug) Debug.Log($"All Spawners Active... Killing Process, {spawnerData.amountLeftToSpawn} spawns waiting for next spawn cycle.", this);
#endif          
                break;
            }
            GameObject spawnObj = FetchFromPool();
            _poolSize = _pooledObjects.Count;
            
            if (!spawnObj)
            {
                _poolSize++;
                ProcessPool();
                continue;
            }
            var navBehavior = spawner.pathingTarget ? spawnObj.GetComponent<NavAgentBehavior>(): null;
            
#if UNITY_EDITOR
            if (spawner.pathingTarget && !navBehavior) Debug.LogError($"No NavAgentBehavior found on {spawnObj} though a pathingTarget was found in ProcessSpawnedObject Method");
#endif
            Transform objTransform = spawnObj.transform;
            PooledObjectBehavior spawnBehavior = spawnObj.GetComponent<PooledObjectBehavior>();
            
#if UNITY_EDITOR
            if (!spawnBehavior) Debug.LogError($"No SpawnObjectBehavior found on {spawnObj} in ProcessSpawnedObject Method", this);
#endif
            var rb = spawnObj.GetComponent<Rigidbody>();
            
            spawnBehavior.Setup(this, ref spawner, ref spawnerData.allowDebug);
            
            if (rb)
            {
                if (!rb.isKinematic)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
            
#if UNITY_EDITOR
            if (allowDebug)
            {
                Debug.Log($"Retrieved Spawner: {spawner.spawnerID} with...\n Count: {spawner.GetAliveCount()} Limit: {spawnerData.GetSpawnerActiveLimit(spawner)}", this);
                Debug.Log($"Setting up {spawnObj} at Position [{spawner.spawnLocation.name}]: {spawner.spawnLocation.position}", this);
            }
#endif
            
            objTransform.position = spawner.spawnLocation.position;
            if (spawner.pathingTarget)
            {
                objTransform.rotation = Quaternion.LookRotation(spawner.pathingTarget.position - spawner.spawnLocation.position);
            }
            
            if (navBehavior) navBehavior.destination = spawner.pathingTarget.position;
            spawnObj.SetActive(true);
            if (navBehavior) navBehavior.Setup(spawner.pathingTarget.position);
            onSpawn.Invoke();
            spawnerData.HandleSuccessfulSpawn(ref spawner);
            yield return waitTime;
        }
        onSpawningComplete.Invoke();
        _spawnRoutine = null;
    }

    private GameObject FetchFromPool() => FetchFromList(_pooledObjects, obj => !obj.activeInHierarchy);
    
    private IEnumerator ProcessWaitingSpawns()
    {
        yield return _wffu;
        if (spawnerData.spawningComplete) yield break;
#if UNITY_EDITOR
        if (_spawnRoutine != null)
        {
            if (allowDebug) Debug.Log("Attempted restart spawning for waiting spawns, but a spawn routine is already running.", this);
        }     
#endif
        _spawnRoutine ??= StartCoroutine(Spawn());
        yield return _spawnDelay;
        _spawnWaitingRoutine = null;
    }
    
    public void NotifyPoolObjectDisabled(ref SpawnerData.Spawner spawner)
    {
        if (_destroying || _pooling) return;
        spawnerData.HandleSpawnRemoval(ref spawner);
        
#if UNITY_EDITOR
        // if (allowDebug)
            Debug.Log($"Notified of Death: passed {spawner.spawnerID} as spawnerID\nTotal active: {spawnerData.activeCount}\n" +
                      $"Currently spawning: {spawnerData.amountLeftToSpawn}\nDestroying: {_destroying}\nPooling: {_pooling}\n" +
                      $"Spawning Complete: {spawnerData.spawningComplete}", this);
#endif
        
        if (spawnerData.activeCount <= 0 && spawnerData.amountLeftToSpawn <= 0)
        {
#if UNITY_EDITOR
            if (allowDebug) Debug.Log($"{spawner.spawnerID} held the final spawn. Raising event.", this);
#endif
            onFinalSpawnDefeated.Invoke();
        }
        else
        {
            if (spawnerData.spawningComplete || _destroying) return;
            _spawnWaitingRoutine ??= StartCoroutine(ProcessWaitingSpawns());
        }
    }

    public void NotifyPoolObjectInvalidDeath(ref SpawnerData.Spawner spawnerID) => spawnerData.HandleSpawnRemoval(ref spawnerID, true);

    private void OnDisable() => _destroying = true;
    private void OnDestroy() => _destroying = true;

    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)> { (() => StartSpawn(spawnerData.originalTotalCountToSpawn), "Spawn") };
    }

}
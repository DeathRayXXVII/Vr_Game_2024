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
#if UNITY_EDITOR
    private bool allowDebug => spawnerData.allowDebug;
#endif

    [SerializeField] private UnityEvent onSpawn, onSpawningComplete, onFinalSpawnDefeated;

    public SpawnerData spawnerData;

    private float _spawnDelay = 0.5f;
    
    [HideInInspector] public int waitingCount;

    private int spawnCount
    {
        get => spawnerData.spawnCount;
        set => spawnerData.spawnCount = value;
    }
    
    public void IncrementSpawnCount() => spawnCount++;
    
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

    private List<GameObject> _pooledObjects;

    private WaitForSeconds _waitForSpawnDelay, _waitLoadBuffer;
    private WaitForFixedUpdate _wffu;
    private Coroutine _lateStartRoutine, _delaySpawnRoutine,_spawnRoutine,_poolCreationRoutine, _spawnWaitingRoutine;

    private PrefabDataList _prefabSet;
    private GameObject _parentObject;

    private int spawnedCount
    {
        get => spawnerData.activeCount.value;
        set => spawnerData.activeCount.Set(value);
    }

    private void Awake()
    {
        spawnerData.ResetSpawnerData();
        _parentObject = new GameObject($"SpawnedObjects_{name}");
        
        _wffu = new WaitForFixedUpdate();
        _waitLoadBuffer = new WaitForSeconds(1.0f);

        _poolSize = poolSize;
        
        SetSpawnDelay();
        spawnerData.SetSpawnRate();

#if UNITY_EDITOR
        if (!spawnerData)
        {
            Debug.LogError("SpawnerData not found in " + name, this);
            return;
        }
#endif
        
        _prefabSet = spawnerData.prefabList;
        _poolCreationRoutine ??= StartCoroutine(DelayPoolCreation());
    }

    private IEnumerator DelayPoolCreation()
    {
        _pooling = true;
        yield return _waitLoadBuffer;
        _parentObject.transform.SetParent(transform);
        yield return _wffu;
        ProcessPool();
        yield return _wffu;
        _poolCreationRoutine = null;
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
        
        _pooling = false;
    }

    private void AddToPool(GameObject obj)
    {
        var spawnBehavior = obj.GetComponent<PooledObjectBehavior>();
        if (!spawnBehavior) obj.AddComponent<PooledObjectBehavior>();

        _pooledObjects.Add(obj);
        obj.transform.SetParent(_parentObject.transform);
        obj.SetActive(false);
    }

    public void SetSpawnDelay(float newDelay)
    {
        newDelay = ToleranceCheck(_spawnDelay, newDelay);
        if (newDelay < 0) return;
        _spawnDelay = newDelay;
        _waitForSpawnDelay = new WaitForSeconds(_spawnDelay);
        SetSpawnDelay();
    }

    private void SetSpawnDelay()
    {
        _waitForSpawnDelay = new WaitForSeconds(_spawnDelay);
    }
    
    public void StartSpawn(int amount)
    {
        if (_spawnRoutine != null || waitingCount > 0) return;
        spawnCount = (amount > 0) ? amount : spawnCount;
        StartSpawn();
    }

    public void StartSpawn()
    {
        if (_spawnRoutine != null) return;
        if (spawnedCount > 0) spawnedCount = 0;
        _delaySpawnRoutine ??= StartCoroutine(DelaySpawn());
    }

    public void ImmediateSpawn()
    {
        if (_spawnRoutine != null) return;
        if (spawnedCount > 0) spawnedCount = 0;
        _spawnRoutine ??= StartCoroutine(Spawn(true));
    }
    
    public void StopSpawn()
    {
        if (_spawnRoutine == null) return;
        StopCoroutine(_spawnRoutine);
        _spawnRoutine = null;
    }

    private IEnumerator DelaySpawn()
    {
        spawnerData.SetSpawnRate();
        while(_poolCreationRoutine != null) yield return _wffu;
        yield return _wffu;
        yield return _waitForSpawnDelay;
        _spawnRoutine ??= StartCoroutine(Spawn());
        yield return _wffu;
        _delaySpawnRoutine = null;
    }
    
    private IEnumerator Spawn(bool immediate = false)
    {
        yield return _waitLoadBuffer;
        
        while(_pooling) yield return _waitLoadBuffer;
        
        while (spawnedCount < spawnCount)
        {
            var waitTime = immediate ? (YieldInstruction)_waitLoadBuffer : spawnerData.GetWaitSpawnRate();
            SpawnerData.Spawner spawner = spawnerData.GetInactiveSpawner();
            
#if UNITY_EDITOR
            if (allowDebug)
            {
                Debug.Log($"Spawning Info...\nTotal Spawns Currently Active Count: {spawnedCount}\nTotal To Spawn: {spawnCount}\nNum Left: {spawnCount-spawnedCount}\nPoolSize: {_poolSize}\nPooledObjects: {_pooledObjects.Count}\nspawners: {spawnerData.spawners.Count}\nspawnRate: {waitTime}", this);
                Debug.Log((spawner == null) ? "No Spawners Available" : $"Spawner: {spawner.spawnerID}", this);
            }
#endif
            
            yield return _wffu;
            
            if (spawner == null)
            {
                waitingCount = spawnCount - spawnedCount;
                spawnedCount = 0;
#if UNITY_EDITOR
                if (allowDebug) Debug.Log($"All Spawners Active... Killing Process, {waitingCount} spawns waiting for next spawn cycle.", this);
#endif
                yield break;
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
            spawner.IncrementCount();
            spawnedCount++;
            yield return waitTime;
        }
        onSpawningComplete.Invoke();
        _spawnRoutine = null;
    }

    private GameObject FetchFromPool() { return FetchFromList(_pooledObjects, obj => !obj.activeInHierarchy); }
    
    private IEnumerator ProcessWaitingSpawns()
    {
        yield return _wffu;
        if (waitingCount <= 0) yield break;
        StartSpawn(waitingCount);
        waitingCount = 0;
        _spawnWaitingRoutine = null;
    }
    
    public void NotifyPoolObjectDisabled(ref SpawnerData.Spawner spawnerID)
    {
        if (_destroying || _pooling) return;
        spawnerData.HandleSpawnRemoval(ref spawnerID);
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Notified of Death: passed {spawnerID} as spawnerID\nTotal active: {spawnerData.activeCount}", this);
#endif
        
        if (spawnerData.activeCount <= 0 && spawnCount - spawnedCount <= 0 && waitingCount <= 0)
        {
#if UNITY_EDITOR
            if (allowDebug) Debug.Log($"NOTIFIED: {spawnerID} WAS THE FINAL SPAWN", this);
#endif
            onFinalSpawnDefeated.Invoke();
        }
        else
        {
            if (waitingCount <= 0 || _destroying) return;
            _spawnWaitingRoutine ??= StartCoroutine(ProcessWaitingSpawns());
        }
    }

    private void OnDisable()
    {
        _destroying = true;
    }

    private void OnDestroy()
    {
        _destroying = true;
    }


    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)> { (() => StartSpawn(spawnCount), "Spawn") };
    }

}
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameCoreData coreData;
    [SerializeField] private LevelData levelData;
    [SerializeField] private GameAction initializeCannonAction, initializeAmmoAction;

    [SerializeField] private UnityEvent onInitializeLevel, onLevelInitialized;
    
    
    private int currentLevel => levelData.currentLevel.value;
    private int spawnCount => levelData.spawnsPerLane;

    private ObjectInstancer _shipInstancer;

    private void OnValidate()
    {
        if (!coreData) Debug.LogError("Core Data is missing. One must be provided.", this);
        if (!levelData) Debug.LogError("Level Data is missing. One must be provided.", this);
        if (!initializeCannonAction) Debug.LogError("Initialize Cannon Action is missing. One must be provided.", this);
        if (!initializeAmmoAction) Debug.LogError("Initialize Ammo Action is missing. One must be provided.", this);
    }

    private void Awake()
    {
        _shipInstancer = this.AddComponent<ObjectInstancer>();
        _shipInstancer.SetInstancerData(coreData.shipInstancerData);
    }
    
    private void Start()
    {
        onInitializeLevel.Invoke();
        StartCoroutine(InitializeLevelCoroutine());
    }

    private IEnumerator InitializeLevelCoroutine()
    {
        // Initialize the ship asynchronously using a coroutine
        yield return StartCoroutine(InitializeShipCoroutine());

        // Initialize the cannon and ammo only after the ship is done
        StartCoroutine(InitializeCannon());
        StartCoroutine(InitializeAmmo());

        onLevelInitialized.Invoke();
    }
    
    private IEnumerator InitializeShipCoroutine()
    {
        _shipInstancer.InstantiateObjects();
        yield return null;
    }

    private IEnumerator InitializeCannon()
    {
        initializeCannonAction.RaiseAction();
        yield return null;
    }

    private IEnumerator InitializeAmmo()
    {
        initializeAmmoAction.RaiseAction();
        yield return null;
    }
}
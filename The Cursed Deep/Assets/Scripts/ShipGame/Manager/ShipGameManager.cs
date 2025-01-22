using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using ShipGame.ScriptObj;
using Unity.VisualScripting;

namespace ShipGame.Manager
{
    public class ShipGameManager : GameManager
    {
        private readonly WaitForFixedUpdate _waitFixed = new();

        [SerializeField] private CoreData coreData;
        [SerializeField] private GameAction initializeCannonAction, initializeAmmoAction, levelCompleteAction, levelFailedAction;

        private ObjectInstancer _shipInstancer;
        
        public UnityEvent onInitializeLevel;
        public UnityEvent onLevelInitialized;
        public UnityEvent onLevelComplete;
        public UnityEvent onLevelFailed;

        private void OnValidate()
        {
            if (!coreData) Debug.LogError("Core Data is missing. One must be provided.", this);
            if (!initializeCannonAction) Debug.LogError("Initialize Cannon Action is missing. One must be provided.", this);
            if (!initializeAmmoAction) Debug.LogError("Initialize Ammo Action is missing. One must be provided.", this);
            if (!levelCompleteAction) Debug.LogError("Level Complete Action is missing. One must be provided.", this);
            if (!levelFailedAction) Debug.LogError("Level Failed Action is missing. One must be provided.", this);
        }

        protected override void Awake()
        {
            initialized = false;
            _shipInstancer = this.AddComponent<ObjectInstancer>();
            _shipInstancer.SetInstancerData(coreData.shipInstancerData);
            base.Awake();
        }
        
        private void OnEnable()
        {
            levelCompleteAction.RaiseEvent += LevelComplete;
            levelFailedAction.RaiseEvent += LevelFailed;
        }
        
        private void OnDisable()
        {
            levelCompleteAction.RaiseEvent -= LevelComplete;
            levelFailedAction.RaiseEvent -= LevelFailed;
        }
        
        protected override IEnumerator Initialize()
        {
            HandleBeforeInitialization();
            yield return _waitFixed;
            
            yield return StartCoroutine(InitializeLevelCoroutine());
            yield return _waitFixed;
            
            HandleTutorialInitialization();
            yield return _waitFixed;
            
            yield return StartCoroutine(HandleSceneBehaviorInitialization());
        
            initialized = true;
            _initCoroutine = null;
        }

        private IEnumerator InitializeLevelCoroutine()
        {
            onInitializeLevel.Invoke();
            yield return null;
            coreData.Setup();
            yield return new WaitUntil(() => coreData.setupComplete);
            
            // Force the ship to initialize first before the cannon and ammo
            yield return StartCoroutine(InitializeShipCoroutine());
            
            // Asynchronously Initialize the cannon and ammo only after the ship is done
            StartCoroutine(InitializeCannon());
            StartCoroutine(InitializeAmmo());
            yield return _waitFixed;

            onLevelInitialized.Invoke();
            yield return _waitFixed;
        }

        private IEnumerator InitializeShipCoroutine()
        {
            _shipInstancer.InstantiateObjects();
            yield return null;
            
            // Now that the ship is instantiated, initialize the trackers to update all associated transformData SOs
            yield return StartCoroutine(InitializeTrackers());
            yield return null;
        }

        private IEnumerator InitializeCannon()
        {
            initializeCannonAction.RaiseAction();
            yield return _waitFixed;
        }

        private IEnumerator InitializeAmmo()
        {
            initializeAmmoAction.RaiseAction();
            yield return _waitFixed;
        }

        protected override IEnumerator HandleSceneBehaviorInitialization()
        {
            coreData.playerInitializePositionAction?.RaiseAction();
            
            yield return StartCoroutine(base.HandleSceneBehaviorInitialization());
        }

        private void LevelComplete(GameAction action)
        {
            coreData.LevelCompleted();
            onLevelComplete.Invoke();
        }

        private void LevelFailed(GameAction action)
        {
            coreData.LevelFailed();
            onLevelFailed.Invoke();
        }
    }
}
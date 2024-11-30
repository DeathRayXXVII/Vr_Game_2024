using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneBehavior : MonoBehaviour
{
    [SerializeField] private bool allowDebug;
    
    [Tooltip("Scene Initialization Manager that will be used to track scene initialization.")]
    [SerializeField] private GameManager initializationManager;
    
    [Tooltip("TransformBehavior that will be used to set the player's position at the start of the scene.")]
    [SerializeField] private TransformBehavior playerTransform;
    
    [Tooltip("TransformData that will be used to set the player's position at the start of the scene.")]
    [SerializeField] private TransformData initialPlayerTransform;

    [Tooltip("Animator that will be used to transition between scenes.")]
    [SerializeField] private Animator transitionAnimator;
    
    [Tooltip("Should the transition animation be played when loading a scene?")]
    [SerializeField] private bool transitionOnLoad = true;
    
    [Tooltip("Should the game start after initialization is complete?")]
    [SerializeField] private bool startGameAfterTransition = true;
    
    [Tooltip("Name of the Animation trigger that will be used to transition INTO the scene.")]
    [SerializeField] private string transitionInTrigger = "TransitionIn";
    
    [Tooltip("Name of the Animation trigger that will be used to transition OUT OF the scene.")]
    [SerializeField] private string transitionOutTrigger = "TransitionOut";
    
    [Tooltip("Time in seconds to wait before transitioning INTO or OUT OF the scene, after the transition animation and buffer have finished.")]
    [SerializeField, SteppedRange(0, 10, 0.1f)] private float sceneLoadBuffer = 1f;
    
    [Tooltip("Time in seconds to wait before transitioning INTO the scene. Additive to sceneLoadBuffer.")]
    [SerializeField, SteppedRange(0, 10, 0.1f)] private float transitionInBuffer = 1f;
    [Tooltip("Time in seconds to wait before transitioning OUT OF the scene. Additive to sceneLoadBuffer.")]
    [SerializeField, SteppedRange(0, 10, 0.1f)] private float transitionOutBuffer = 1f;
    
    [Tooltip("UnityEvent that will be invoked before transitioning INTO the scene. Occurs after sceneLoadBuffer and before transitionInBuffer.")]
    [SerializeField] private UnityEvent beforeTransitionIn;
    [Tooltip("UnityEvent that will be invoked after transitioning INTO the scene.")]
    [SerializeField] private UnityEvent onTransitionInComplete;

    public void SetAnimator(AnimatorOverrideController animator)
    {
        if (transitionAnimator == null)
        {
            return;
        }
        transitionAnimator.runtimeAnimatorController = animator;
    }
    public string transitionInTriggerName { set => transitionInTrigger = value; }
    public string transitionOutTriggerName { set => transitionOutTrigger = value; }
    
    private bool hasTriggerIn => HasTrigger(transitionInTrigger);
    private bool hasTriggerOut => HasTrigger(transitionOutTrigger);
    
    private bool? isTransitioning => transitionAnimator == null ? null :
        transitionAnimator.IsInTransition(0) || transitionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;

    private WaitForFixedUpdate _waitFixed;
    private Coroutine _loadCoroutine;
    private Coroutine _transitionCoroutine;
    private Coroutine _initializeCoroutine;
    
    private void Awake()
    {
        initializationManager ??= FindObjectOfType<GameManager>();
    }
    
    private void Start()
    {
        Initialize();
    }
    
    private void OnDisable()
    {
        if (_loadCoroutine != null)
        {
            StopCoroutine(_loadCoroutine);
            _loadCoroutine = null;
        }
        if (_initializeCoroutine != null)
        {
            StopCoroutine(_initializeCoroutine);
            _initializeCoroutine = null;
        }
        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }
    }
    
    
    private bool HasTrigger(string triggerName)
    {
        if (transitionAnimator == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in transitionAnimator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
            {
                return true;
            }
        }
        return false;
    }
    
    private bool _isInitialized;
    private void Initialize()
    {
        if (_isInitialized) return;
        _initializeCoroutine ??= StartCoroutine(InitializeAndTransitionIn());
    }

    private IEnumerator InitializeAndTransitionIn()
    {
        // Wait until all trackers are initialized.
        if (_isInitialized)
        {
            yield break;
        }
        
        var hasInitManager = initializationManager != null;
        if (!hasInitManager)
        {
            Debug.LogError("Initialization Manager not set.", this);
        }
        
        // Wait until the initialization manager is done.
        var time = Time.time;
        while (hasInitManager && !initializationManager.initialized)
        {
            if (Time.time - time > 180)
            {
                Debug.LogError("Initialization Manager took too long to initialize.", this);
                hasInitManager = false;
                break;
            }
            
            yield return null;
        }
        yield return _waitFixed;
        
        // Wait until the player is positioned.
        yield return PositionPlayer();
        yield return _waitFixed;
        
        // Proceed with the transition.
// #if UNITY_EDITOR
        if (allowDebug) 
            Debug.Log($"Scene Initialization Complete.\nPerforming Transition in -- " +
                      $"{transitionAnimator != null && transitionOnLoad && hasTriggerIn}", this);
// #endif

        beforeTransitionIn.Invoke();
        yield return _waitFixed;
        
        if (transitionOnLoad && hasTriggerIn)
        {
            TransitionIntoScene();
            yield return new WaitUntil(() => _transitionCoroutine == null);
        }
        
// #if UNITY_EDITOR
        if (allowDebug && startGameAfterTransition) 
            Debug.Log($"Starting Game -- {hasInitManager && startGameAfterTransition}", this);
// #endif
        
        if (hasInitManager && startGameAfterTransition) initializationManager.StartGame();
        
        _isInitialized = true;
        _initializeCoroutine = null;
    }
    
    private bool PositionPlayer()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is null, cannot complete positioning player.", this);
            return false;
        }
        if (initialPlayerTransform == null)
        {
            Debug.LogError("Initial Player Transform is null, cannot complete positioning player.", this);
            return false;
        }
        
        playerTransform.SetTransform(initialPlayerTransform);
        playerTransform.SetStartTransform(initialPlayerTransform);
        
        return true;
    }
    
    public void TransitionIntoScene() => _transitionCoroutine ??= StartCoroutine(TransitionIn());
    
    private IEnumerator TransitionIn()
    {
        if (!transitionAnimator) yield break;
// #if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Transitioning into Scene, Time: {Time.time}", this);
// #endif
        
        yield return FixedUpdateBuffer(sceneLoadBuffer);
        
        if (hasTriggerOut) yield return ExecuteTransition(transitionInTrigger);
        
        yield return FixedUpdateBuffer(transitionInBuffer);

        onTransitionInComplete.Invoke();
        _transitionCoroutine = null;
    }

    
    public void RestartActiveScene() => LoadScene(SceneManager.GetActiveScene().name);
    
    public void LoadScene(string scene)
    {
        var sceneIndex = SceneUtility.GetBuildIndexByScenePath(scene);
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
// #if UNITY_EDITOR
            if (allowDebug) Debug.Log($"Attempting to load Scene: {scene}", this);
// #endif
            LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError($"Scene: '{scene}' not found.", this);
        }
    }
    
    public void LoadScene(int sceneIndex)
    {
        if (_loadCoroutine != null)
        {
            Debug.LogWarning("Scene loading already in progress.", this);
            return;
        }
// #if UNITY_EDITOR
        var scene = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
        if (!string.IsNullOrEmpty(scene) && allowDebug)
            Debug.Log($"Loading Scene: {scene}, Index: {sceneIndex}", this);
// #endif
        var asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        if (asyncLoad == null)
        {
            Debug.LogError($"Scene at Index: '{sceneIndex}' not found.", this);
            return;
        }
        _loadCoroutine ??= StartCoroutine(LoadSceneAsync(asyncLoad));
    }
    
    private IEnumerator LoadSceneAsync(AsyncOperation loadOperation)
    {
        loadOperation.allowSceneActivation = false;
        yield return BackgroundLoad(loadOperation);
        
        yield return FixedUpdateBuffer(transitionOutBuffer);
        
        if (transitionAnimator && hasTriggerOut) yield return ExecuteTransition(transitionOutTrigger);
        
        yield return FixedUpdateBuffer(sceneLoadBuffer);
        
        _loadCoroutine = null;
        loadOperation.allowSceneActivation = true;
    }
    
    private IEnumerator FixedUpdateBuffer(float waitTime = 5f)
    {
        var time = Time.time;
// #if UNITY_EDITOR
        var debugSpacer = 0;
        const int mod = 20;
// #endif

        var transitioning = isTransitioning ?? false;
        switch (waitTime)
        {
            case <= 0 when transitioning:
                yield break;
            case <= 0:
            {
                while (transitioning)
                {
// #if UNITY_EDITOR
                    if (allowDebug && debugSpacer++ % mod == 0)
                    {
                        Debug.Log($"Time: {Time.time}, Time - time: {Time.time - time}, Wait Time: {waitTime}, " +
                                  $"Transitioning: {isTransitioning}", this);
                    }
// #endif
                    transitioning = isTransitioning ?? false;
                    
                    yield return _waitFixed;
                }

                break;
            }
            default:
            {
                while (transitioning || Time.time - time < waitTime)
                {
// #if UNITY_EDITOR
                    if (allowDebug && debugSpacer++ % mod == 0)
                    {
                        Debug.Log($"Time: {Time.time}, Time - time: {Time.time - time}, Wait Time: {waitTime}, " +
                                  $"Transitioning: {isTransitioning}", this);
                    }
// #endif
                    transitioning = isTransitioning ?? false;
                    
                    yield return _waitFixed;
                }

                break;
            }
        }

// #if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Transition Complete, Time: {Time.time}", this);
// #endif
    }
    
    private IEnumerator ExecuteTransition(string transitionTrigger)
    {
        transitionAnimator.SetTrigger(transitionTrigger);
        yield return new WaitForSeconds(transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
    }
    
    private IEnumerator BackgroundLoad(AsyncOperation loadOperation)
    {
        while (!loadOperation.isDone && loadOperation.progress < 0.9f)
            yield return _waitFixed;
    }
}

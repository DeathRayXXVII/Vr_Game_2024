using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneBehavior : MonoBehaviour
{
    [SerializeField] private bool allowDebug;
    [Tooltip("Animator that will be used to transition between scenes.")]
    [SerializeField] private Animator transitionAnimator;
    
    [Tooltip("Should the transition animation be played when loading a scene?")]
    [SerializeField] private bool transitionOnLoad = true;
    
    [Tooltip("Name of the Animation trigger that will be used to transition INTO the scene.")]
    [SerializeField] private string transitionInTrigger = "TransitionIn";
    
    [Tooltip("Name of the Animation trigger that will be used to transition OUT OF the scene.")]
    [SerializeField] private string transitionOutTrigger = "TransitionOut";
    
    [Tooltip("Time in seconds to wait before transitioning INTO or OUT OF the scene.")]
    [SerializeField, SteppedRange(0, 10, 0.1f)] private float sceneLoadBuffer = 3f;
    
    [Tooltip("Time in seconds to wait before transitioning INTO the scene. Additive to sceneLoadBuffer.")]
    [SerializeField, SteppedRange(0, 10, 0.1f)] private float transitionInBuffer = 3f;
    [Tooltip("Time in seconds to wait before transitioning OUT OF the scene. Additive to sceneLoadBuffer.")]
    [SerializeField, SteppedRange(0, 10, 0.1f)] private float transitionOutBuffer = 3f;
    
    [SerializeField] private UnityEvent beforeTransitionIn;
    [SerializeField] private UnityEvent onTransitionInComplete;

    public void SetAnimator(AnimatorOverrideController animator) => transitionAnimator.runtimeAnimatorController = animator;
    public string transitionInTriggerName { set => transitionInTrigger = value; }
    public string transitionOutTriggerName { set => transitionOutTrigger = value; }
    
    private bool hasTriggerIn => HasTrigger(transitionInTrigger);
    private bool hasTriggerOut => HasTrigger(transitionOutTrigger);
    
    // Check if the transition animator is currently transitioning
    private bool isTransitioning => transitionAnimator.IsInTransition(0) ||
                                    transitionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
    
    private WaitForFixedUpdate _waitFixed;
    private Coroutine _loadCoroutine;
    private Coroutine _transitionCoroutine;

    private void Start()
    {
        if (!transitionAnimator) return;
        
        if (transitionOnLoad && hasTriggerIn) TransitionIntoScene();
    }
    
    
    private void OnDisable()
    {
        if (_loadCoroutine != null)
        {
            StopCoroutine(_loadCoroutine);
            _loadCoroutine = null;
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
    
    
    public void TransitionIntoScene() => _transitionCoroutine ??= StartCoroutine(TransitionIn());
    
    private IEnumerator TransitionIn()
    {
        if (!transitionAnimator) yield break;
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Transitioning into Scene, Time: {Time.time}", this);
#endif
        
        yield return FixedUpdateBuffer(sceneLoadBuffer);
        
        beforeTransitionIn.Invoke();
        
        yield return FixedUpdateBuffer(transitionInBuffer);
        
        if (hasTriggerOut) yield return ExecuteTransition(transitionInTrigger);
        yield return _waitFixed;

        onTransitionInComplete.Invoke();
        _transitionCoroutine = null;
    }

    
    public void RestartActiveScene() => LoadScene(SceneManager.GetActiveScene().name);
    
    public void LoadScene(string scene)
    {
        var sceneIndex = SceneUtility.GetBuildIndexByScenePath(scene);
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
#if UNITY_EDITOR
            if (allowDebug) Debug.Log($"Attempting to load Scene: {scene}", this);
#endif
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
#if UNITY_EDITOR
        var scene = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
        if (!string.IsNullOrEmpty(scene) && allowDebug)
            Debug.Log($"Loading Scene: {scene}, Index: {sceneIndex}", this);
#endif
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
        
        yield return FixedUpdateBuffer(sceneLoadBuffer);
        
        if (transitionAnimator && hasTriggerOut) yield return ExecuteTransition(transitionOutTrigger);
        yield return FixedUpdateBuffer(transitionOutBuffer);
        
        loadOperation.allowSceneActivation = true;
        _loadCoroutine = null;
    }
    
    private IEnumerator FixedUpdateBuffer(float waitTime = 5f)
    {
        var time = Time.time;
#if UNITY_EDITOR
        var debugSpacer = 0;
        const int mod = 20;
#endif
        while (isTransitioning || Time.time - time < waitTime)
        {
#if UNITY_EDITOR
            if (allowDebug && debugSpacer++ % mod == 0) 
                Debug.Log($"Time: {Time.time}, Time - time: {Time.time - time}, Wait Time: {waitTime}, " +
                          $"Transitioning: {isTransitioning}", this);
#endif
            yield return _waitFixed;
        }
#if UNITY_EDITOR
        if (allowDebug) Debug.Log($"Transition Complete, Time: {Time.time}", this);
#endif
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

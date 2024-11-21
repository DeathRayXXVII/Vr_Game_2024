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
    
    [Tooltip("Name of the trigger that will be used to transition into the scene.")]
    [SerializeField] private string transitionInTrigger = "TransitionIn";
    
    [Tooltip("Name of the trigger that will be used to transition out of the scene.")]
    [SerializeField] private string transitionOutTrigger = "TransitionOut";
    
    [SerializeField, SteppedRange(0, 10, 0.01f)] private float sceneLoadBuffer = 3.0f;
    [SerializeField, SteppedRange(1, 25, 1)] private int transitionCompleteBuffer = 5;
    
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
    
    private WaitForSeconds _waitSceneLoadBuffer;
    private WaitForFixedUpdate _waitFixed;
    private Coroutine _loadCoroutine;
    private Coroutine _transitionCoroutine;

    private void Start()
    {
        _waitSceneLoadBuffer = new WaitForSeconds(sceneLoadBuffer);
        if (!transitionAnimator) return;
        
        if (transitionOnLoad && hasTriggerIn) TransitionIntoScene();
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
        
        yield return _waitSceneLoadBuffer;
        
        beforeTransitionIn.Invoke();
        
        transitionAnimator.SetTrigger(transitionInTrigger);

        while (transitionAnimator.isInitialized && isTransitioning)
        {
            yield return null;
        }

        yield return FixedUpdateBuffer(transitionCompleteBuffer);
        onTransitionInComplete.Invoke();
        _transitionCoroutine = null;
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
        if (transitionAnimator) yield return ExecuteTransition();
        yield return FixedUpdateBuffer(20);
        loadOperation.allowSceneActivation = true;
        _loadCoroutine = null;
    }
    
    private IEnumerator FixedUpdateBuffer(int waitTime = 5)
    {
        var time = Time.time;
        while (isTransitioning && Time.time - time < waitTime)
        {
            yield return _waitFixed;
        }
    }
    
    private IEnumerator ExecuteTransition()
    {
        transitionAnimator.SetTrigger(transitionOutTrigger);
        yield return new WaitForSeconds(transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
    }
    
    private IEnumerator BackgroundLoad(AsyncOperation loadOperation)
    {
        while (!loadOperation.isDone && loadOperation.progress < 0.9f)
            yield return _waitFixed;
    }
}

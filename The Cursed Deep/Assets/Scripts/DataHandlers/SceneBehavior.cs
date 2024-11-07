using System.Collections;
using UnityEngine;
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

    public void SetAnimator(AnimatorOverrideController animator) => transitionAnimator.runtimeAnimatorController = animator;
    public string transitionInTriggerName { set => transitionInTrigger = value; }
    public string transitionOutTriggerName { set => transitionOutTrigger = value; }
    
    private readonly WaitForFixedUpdate _wait = new();
    private Coroutine _loadCoroutine;

    private void Start()
    {
        if (transitionOnLoad) StartCoroutine(TransitionIn());
    }
    
    private IEnumerator TransitionIn()
    {
        if (!transitionAnimator) yield break;
        
        while (transitionAnimator.isInitialized && transitionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
            yield return _wait;
        
        transitionAnimator.SetTrigger(transitionInTrigger);
    } 
    
    private void OnDisable()
    {
        if (_loadCoroutine == null) return;
        
        StopCoroutine(_loadCoroutine);
        _loadCoroutine = null;
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
        yield return FixedUpdateBuffer(5);
        loadOperation.allowSceneActivation = true;
    }
    
    private IEnumerator FixedUpdateBuffer(int frames)
    {
        for (var i = 0; i < frames; i++)
            yield return _wait;
    }
    
    private IEnumerator ExecuteTransition()
    {
        transitionAnimator.SetTrigger(transitionOutTrigger);
        yield return new WaitForSeconds(transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
    }
    
    private IEnumerator BackgroundLoad(AsyncOperation loadOperation)
    {
        while (!loadOperation.isDone && loadOperation.progress < 0.9f)
            yield return _wait;
    }
}

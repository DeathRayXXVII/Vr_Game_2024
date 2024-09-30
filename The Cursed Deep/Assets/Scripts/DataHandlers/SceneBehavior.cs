using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBehavior : MonoBehaviour
{
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
        {
            yield return _wait;
        }
        
        transitionAnimator.SetTrigger(transitionInTrigger);
    } 
    
    private void OnDisable()
    {
        if (_loadCoroutine == null) return;
        
        StopCoroutine(_loadCoroutine);
        _loadCoroutine = null;
    }
    
    public void LoadScene(string scene)
    {
        var sceneIndex = SceneUtility.GetBuildIndexByScenePath(scene);
        var sceneAt = SceneManager.GetSceneAt(sceneIndex);
        Debug.Log($"Scene: {sceneAt.name}, Index: {sceneAt.buildIndex}");
        Debug.Log($"Loading Scene: {scene}, Index: {sceneIndex}");
        if (sceneIndex >= 0)
        {
            LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError($"Scene: '{scene}' not found.", this);
        }
    }
    
    public void LoadScene(int sceneIndex)
    {
        if (transitionAnimator)
        {
            _loadCoroutine ??= StartCoroutine(LoadSceneWithTransitionAsync(sceneIndex));
        }
        else
        {
            _loadCoroutine ??= StartCoroutine(LoadSceneAsync(sceneIndex));
        }
    }
    
    public void RestartActiveScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }
    
    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        var asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        if (asyncLoad == null)
        {
            Debug.LogError($"Scene Index: '{sceneIndex}' not found.", this);
            yield break;
        }
        asyncLoad.allowSceneActivation = false;
        
        while (!asyncLoad.isDone && asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        asyncLoad.allowSceneActivation = true;
        _loadCoroutine = null;
    }
    
    private IEnumerator LoadSceneWithTransitionAsync(int sceneIndex)
    {
        var asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        if (asyncLoad == null)
        {
            Debug.LogError($"Scene Index: '{sceneIndex}' not found.", this);
            yield break;
        }
        asyncLoad.allowSceneActivation = false;
        
        while (!asyncLoad.isDone && asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        
        transitionAnimator.SetTrigger(transitionOutTrigger);
        yield return new WaitForSeconds(transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
        asyncLoad.allowSceneActivation = true;
        _loadCoroutine = null;
    }
}

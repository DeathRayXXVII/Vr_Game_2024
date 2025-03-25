using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using ZPTools;

public class SimpleSceneBehavior : MonoBehaviour
{
    [Tooltip("Animator that will be used to transition between scenes.")]
    [SerializeField] private ScreenManager screenManager;
    
    [SerializeField, SteppedRange(0, 10, 0.1f)] private float transitionOutDelay;
    
    [Tooltip("Additive time in seconds to wait before loading the scene.")]
    [SerializeField, SteppedRange(0, 10, 0.1f)] private float loadBuffer = 1f;
    
    private readonly WaitForFixedUpdate _waitFixed = new();
    private Coroutine _loadCoroutine;
    private Coroutine _initializeCoroutine;
    
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
    }
    
    private bool _sceneLoaded;
    public void LoadScene(string scene)
    {
        var sceneIndex = SceneUtility.GetBuildIndexByScenePath(scene);
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError($"[ERROR] Scene: '{scene}' not found.", this);
        }
    }
    
    public void LoadScene(int sceneIndex)
    {
        if (_loadCoroutine != null)
        {
            return;
        }
        
        var asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        try
        {
            asyncLoad!.allowSceneActivation = false;
        }
        catch (NullReferenceException)
        {
            Debug.LogError($"[ERROR] Scene at Index: '{sceneIndex}' not found.", this);
            return;
        }
        
        _sceneLoaded = false;
        _loadCoroutine ??= StartCoroutine(LoadAndTransitionOut(asyncLoad));
    }
    
    private IEnumerator LoadAndTransitionOut(AsyncOperation loadOperation)
    {
        StartCoroutine(BackgroundLoad(loadOperation));
        
        yield return StartCoroutine(FixedUpdateBuffer(transitionOutDelay));
        StartCoroutine(screenManager.TransitionOut());
        
        StartCoroutine(FixedUpdateBuffer(loadBuffer));
        
        yield return new WaitUntil(() => !_buffering && _sceneLoaded && !screenManager.isTransitioning);
        
        _loadCoroutine = null;
        loadOperation.allowSceneActivation = true;
    }
    
    private IEnumerator BackgroundLoad(AsyncOperation loadOperation)
    {
        while (!loadOperation.isDone && loadOperation.progress < 0.9f)
        {
            yield return _waitFixed;
        }
        
        _sceneLoaded = true;
    }
    
    private bool _buffering;
    private IEnumerator FixedUpdateBuffer(float waitTime = 5f)
    {
        _buffering = true;
        
        if (waitTime <= 0)
        {
            yield break;
        }
        
        var time = Time.time;
        float elapsedTime = 0;
        
        while (elapsedTime <= waitTime)
        {
            yield return _waitFixed;
            elapsedTime = Time.time - time;
        }
        
        yield return null;
        _buffering = false;
    }
}

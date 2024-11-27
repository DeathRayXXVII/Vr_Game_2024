using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    // Unity Engine Triggered Events
    public UnityEvent onAwake;
    public UnityEvent onStart;
    public UnityEvent onLateInit;
    
    // Editor Triggered Custom Events
    [SerializeField] private bool startGameAfterInit;
    public UnityEvent onGameStart;
    public UnityEvent onGameOver;
    public UnityEvent onGameWin;
    public UnityEvent onRestartGame;
    
    private readonly WaitForFixedUpdate _wffu = new();

    private void Awake()
    {
        onAwake.Invoke();
    }
    
    private void Start()
    {
        onStart.Invoke();
        StartCoroutine(LateInit());
    }
    
    private IEnumerator LateInit()
    {
        yield return _wffu;
        yield return _wffu;
        yield return _wffu;
        onLateInit.Invoke();
        
        yield return _wffu;
        yield return _wffu;
        yield return _wffu;
        if (startGameAfterInit) StartGame();
    }

    public void StartGame()
    {
        onGameStart.Invoke();
    }

    public void GameOver()
    {
        onGameOver.Invoke();
    }

    public void GameWin()
    {
        onGameWin.Invoke();
    }

    public void RestartGame()
    {
        onRestartGame.Invoke();
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    public UnityEvent onAwake, onStart, onLateInit, onGameStart, onGameOver, onGameWin, onRestartGame;
    private readonly WaitForFixedUpdate _wffu = new();

    private void Awake()
    {
        onAwake.Invoke();
    }
    
    private void Start()
    {
        Debug.Log("GameManager Start");
        onStart.Invoke();
        Debug.Log("GameManager Start Invoked");
        StartCoroutine(LateInit());
        Debug.Log("GameManager LateInit Started");
    }
    
    private IEnumerator LateInit()
    {
        yield return _wffu;
        Debug.Log("GameManager LateInit 1");
        yield return _wffu;
        Debug.Log("GameManager LateInit 2");
        yield return _wffu;
        Debug.Log("GameManager LateInit 3");
        onLateInit.Invoke();
        Debug.Log("GameManager LateInit Invoked");
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
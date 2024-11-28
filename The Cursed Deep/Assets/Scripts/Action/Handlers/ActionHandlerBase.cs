using UnityEngine;
using UnityEngine.Events;

public class ActionHandlerBase : MonoBehaviour
{
    public GameAction action;
    public UnityEvent response;

    private void OnEnable()
    {
        action.RaiseEvent += OnEventRaised;
    }

    private void OnDisable()
    {
        action.RaiseEvent -= OnEventRaised;
    }

    private void OnEventRaised(GameAction passedAction)
    {
        response.Invoke();
    }
}

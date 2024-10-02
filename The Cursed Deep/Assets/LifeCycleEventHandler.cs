using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LifeCycleEventHandler : MonoBehaviour
{
    private enum EventOptions
    {
        OnAwake,
        OnEnable,
        OnStart,
        OnLateStart,
        OnDisable,
        OnDestroy
    }

    [Tooltip("Select the lifecycle event to trigger actions.")]
    [SerializeField] private EventOptions selectedEvent;

    [Tooltip("Actions to perform when the selected event is triggered.")]
    [SerializeField] private UnityEvent eventActions;

    
    private void HandleEvent(EventOptions currentEvent)
    {
        if (selectedEvent == currentEvent)
        {
            eventActions?.Invoke();
        }
    }

    private void Awake()
    {
        HandleEvent(EventOptions.OnAwake);
    }

    private void OnEnable()
    {
        HandleEvent(EventOptions.OnEnable);
    }

    private void Start()
    {
        HandleEvent(EventOptions.OnStart);
        StartCoroutine(LateStart());
    }

    private void OnDisable()
    {
        HandleEvent(EventOptions.OnDisable);
    }

    private void OnDestroy()
    {
        HandleEvent(EventOptions.OnDestroy);
    }

    private IEnumerator LateStart()
    {
        yield return null;
        yield return null;
        yield return new WaitForEndOfFrame();
        HandleEvent(EventOptions.OnLateStart);
    }
}
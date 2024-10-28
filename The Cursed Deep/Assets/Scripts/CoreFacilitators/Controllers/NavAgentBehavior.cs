using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentBehavior : MonoBehaviour
{ 
    public UnityEvent onCreepReachedDestination;
    
    private readonly WaitForFixedUpdate _wffu = new();
    private NavMeshAgent _ai;
    public Vector3 destination;

    private void Awake()
    {
        _ai = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        StartEndPathCheck();
    }
    
    public void SetSpeed(float speed)
    {
#if UNITY_EDITOR
        if(!_ai) Debug.LogError("NavMeshAgent not found in " + name, this);
#endif
        _ai.speed = speed;
        Debug.Log($"Speed set to {speed}");
    }
    
    public void SetRadius(float radius){
#if UNITY_EDITOR
        if(!_ai) Debug.LogError("NavMeshAgent not found in " + name, this);
#endif
        _ai.radius = radius;
    }
    
    public void SetHeight(float height)
    {
#if UNITY_EDITOR
        if(!_ai) Debug.LogError("NavMeshAgent not found in " + name, this);
#endif
        _ai.height = height;
    }

    public void Setup(Vector3 dest)
    {
        destination = dest;
        if (!_ai) _ai = GetComponent<NavMeshAgent>();
#if UNITY_EDITOR
        if(!_ai) Debug.LogError("NavMeshAgent not found in " + name, this);
#endif
        if (_ai) _ai.SetDestination(destination);
    }

    private void StartEndPathCheck()
    {
        StartCoroutine(EndCheck());
    }

    private IEnumerator EndCheck()
    {
        while (true)
        {
            if (_ai.remainingDistance < 0.5f && _ai.hasPath)
            {
                onCreepReachedDestination.Invoke();
                yield break;
            }

            yield return _wffu;
        }
    }
    
    public void StopMovement()
    {
        _ai.isStopped = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MatchIDBehavior : IDBehavior
{
    [System.Serializable]
    public struct PossibleMatch
    {
        public ID id;
        public UnityEvent triggerEnterEvent, triggerExitEvent;
    }

    private enum TriggerType
    {
        Enter,
        Exit
    }
    
    private readonly WaitForFixedUpdate _wffu = new();
    [SerializeField] private bool allowDebug;
    [SerializeField] private List<PossibleMatch> triggerEnterMatches;

    // OnTriggerEnter and OnTriggerExit are called when one collider that is marked as a trigger enters or exits this or
    // the other object's non-trigger collider. Summary Each collider you want to have this match check occur on must have
    // an IDBehavior component and an ID assigned to it that corresponds to a match in the triggerEnterMatches list.
    // (This MatchIDBehavior works as an IDBehavior component) and a collider with the Is Trigger option checked.
    
    // NOTE: If you place this on a parent object with a rigidbody, and have child objects with colliders, 
    // all the child objects will trigger the events on the parent object. Allowing for a single instance of this script
    // to handle all the ID matches in a single object.
    private void OnTriggerEnter(Collider other)
    {
        IDBehavior idBehavior = other.GetComponent<IDBehavior>();
        if (!idBehavior) return;
        StartCoroutine(CheckId(idBehavior.id, triggerEnterMatches, TriggerType.Enter));
    }
    
    private void OnTriggerExit(Collider other)
    {
        IDBehavior idBehavior = other.GetComponent<IDBehavior>();
        if (!idBehavior) return;
        StartCoroutine(CheckId(idBehavior.id, triggerEnterMatches, TriggerType.Exit));
    }
    
    private IEnumerator CheckId(ID otherId, List<PossibleMatch> possibleMatches, TriggerType triggerType)
    {
        bool noMatch = true;
        foreach (var obj in possibleMatches)
        {
            if (otherId != obj.id) continue;
            noMatch = false;
            
            if (triggerType == TriggerType.Enter)
            {
                if (allowDebug) Debug.Log($"Triggering Enter Event on: '{this} (ID: {id})' with '(ID: {obj.id}'", this);
                obj.triggerEnterEvent.Invoke();
            }
            else if (triggerType == TriggerType.Exit)
            {
                if (allowDebug) Debug.Log($"Triggering Exit Event on: '{this} (ID: {id})' with '{obj.id}'", this);
                obj.triggerExitEvent.Invoke();
            }
            
            yield return _wffu;
        }

        if (noMatch && allowDebug)
        {
            Debug.Log($"No match found on: '{this} (ID: {id})' While checking for '{otherId}' in {possibleMatches.Count} possible matches.", this);
        }
    }
}
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
    public bool allowDebug;
    public List<PossibleMatch> triggerEnterMatches;

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
                if (allowDebug) Debug.Log($"Triggering Enter Event on: '{this} (ID: {id})' with '{obj.id}'");
                obj.triggerEnterEvent.Invoke();
            }
            else if (triggerType == TriggerType.Exit)
            {
                if (allowDebug) Debug.Log($"Triggering Exit Event on: '{this} (ID: {id})' with '{obj.id}'");
                obj.triggerExitEvent.Invoke();
            }
            
            yield return _wffu;
        }

        if (noMatch && allowDebug)
        {
            Debug.Log($"No match found on: '{this} (ID: {id})' While checking for '{otherId}' in {possibleMatches.Count} possible matches.");
        }
    }
}
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
        public UnityEvent triggerEvent, triggerExitEvent;
    }
    
    private readonly WaitForFixedUpdate _wffu = new();
    public bool debug;
    public List<PossibleMatch> triggerEnterMatches;

    private void OnTriggerEnter(Collider other)
    {
        IDBehavior idBehavior = other.GetComponent<IDBehavior>();
        if (idBehavior == null) return;
        StartCoroutine(CheckId(idBehavior.id, triggerEnterMatches));
    }
    
    private void OnTriggerExit(Collider other)
    {
        IDBehavior idBehavior = other.GetComponent<IDBehavior>();
        if (idBehavior == null) return;
        
        foreach (var obj in triggerEnterMatches)
        {
            if (idBehavior.id != obj.id) continue;
            if (debug) Debug.Log($"Match found on: '{this} (ID: {id})' with '{obj.id}' while checking for '{idBehavior.id}'");
            obj.triggerExitEvent.Invoke();
            return;
        }
    }
    
    private IEnumerator CheckId(ID otherId, List<PossibleMatch> possibleMatches)
    {
        bool noMatch = true;
        foreach (var obj in possibleMatches)
        {
            if (otherId != obj.id) continue;
            if (debug) Debug.Log($"Match found on: '{this} (ID: {id})' with '{obj.id}' while checking for '{otherId}'");
            noMatch = false;
            obj.triggerEvent.Invoke();
            yield return _wffu;
        }

        if (noMatch && debug)
        {
            Debug.Log($"No match found on: '{this} (ID: {id})' While checking for '{otherId}' in {possibleMatches.Count} possible matches.");
        }
    }
}
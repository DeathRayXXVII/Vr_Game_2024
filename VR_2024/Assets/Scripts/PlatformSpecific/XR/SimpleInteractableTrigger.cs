using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class SimpleInteractableTrigger : MonoBehaviour, INeedButton
{
    public UnityEvent onInteractionPerformed;
    public UnityEvent onInteractionEnded;

    private void OnEnable()
    {
        GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().selectEntered.AddListener(_ => OnInteractionPerformed());
        GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().selectExited.AddListener(_ => OnInteractionEnded());
        
    }
    
    private void OnDisable()
    {
        GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().selectEntered.RemoveListener(_ => OnInteractionPerformed());
        GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().selectExited.RemoveListener(_ => OnInteractionEnded());
    }
    
    private void OnInteractionPerformed()
    {
        onInteractionPerformed.Invoke();
    }
    
    private void OnInteractionEnded()
    {
        onInteractionEnded.Invoke();
    }


    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)> { (() => onInteractionPerformed.Invoke(), "Perform Interaction") };
    }

}
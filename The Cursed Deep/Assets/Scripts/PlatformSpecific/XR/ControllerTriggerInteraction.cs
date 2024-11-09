using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


[RequireComponent(typeof(XRSimpleInteractable))]
public class ControllerTriggerInteraction : MonoBehaviour
{
    private XRSimpleInteractable _interactable;

    public UnityEvent onTriggerPressed, onTriggerReleased;

    private void OnEnable()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        
        _interactable.activated.AddListener(_ => TriggerPressed());
        _interactable.deactivated.AddListener(_ => TriggerReleased());
    }
    
    private void OnDisable()
    {
        _interactable.activated.RemoveListener(_ => TriggerPressed());
        _interactable.deactivated.RemoveListener(_ => TriggerReleased());
    }

    private void TriggerPressed() => onTriggerPressed?.Invoke();

    private void TriggerReleased() => onTriggerReleased?.Invoke();
}
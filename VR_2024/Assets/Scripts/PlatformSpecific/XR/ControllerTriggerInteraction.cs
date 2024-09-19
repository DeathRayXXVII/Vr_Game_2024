using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class ControllerTriggerInteraction : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _interactable;

    public UnityEvent onTriggerDown, onTriggerUp;

    private void OnEnable()
    {
        _interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        _interactable.activated.AddListener(_ => Perform());
        _interactable.deactivated.AddListener(_ => Stop());
    }
    
    private void OnDisable()
    {
        _interactable.activated.RemoveListener(_ => Perform());
        _interactable.deactivated.RemoveListener(_ => Stop());
    }

    private void Perform()
    {
        onTriggerDown?.Invoke();
    }

    private void Stop()
    {
        onTriggerUp?.Invoke();
    }
}
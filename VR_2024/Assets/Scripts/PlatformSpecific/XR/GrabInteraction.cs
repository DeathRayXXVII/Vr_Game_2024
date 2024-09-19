using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class GrabInteraction : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _interactable;

    public bool toggleGrabbersMeshVisibility;
    public bool canGrab { get; set; }

    public UnityEvent onGrab, onRelease;

    private void OnEnable()
    {
        _interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        _interactable.selectEntered.AddListener(Grab);
        _interactable.selectExited.AddListener(Release);
        
        canGrab = true;
    }
    
    private void OnDisable()
    {
        _interactable.selectEntered.RemoveListener(Grab);
        _interactable.selectExited.RemoveListener(Release);
    }
    
    private void OnDestroy()
    {
        _interactable.selectEntered.RemoveListener(Grab);
        _interactable.selectExited.RemoveListener(Release);
    }

    private void Grab(SelectEnterEventArgs arg)
    {
        var interactorType = arg.interactorObject.GetType();
        if (interactorType == typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor) || interactorType == typeof(SocketMatchInteractor)) return;
        if (toggleGrabbersMeshVisibility) ToggleVis(false, arg.interactorObject.transform);
        if (!canGrab) return;
        HandleInteractionEvent(true);
    }

    private void Release(SelectExitEventArgs arg)
    {
        var interactorType = arg.interactorObject.GetType();
        if (interactorType == typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor) || interactorType == typeof(SocketMatchInteractor)) return;
        if (toggleGrabbersMeshVisibility) ToggleVis(true, arg.interactorObject.transform);
        HandleInteractionEvent(false);
    }

    private void ToggleVis(bool on, Component interactor)
    {
        var meshBehavior = interactor.GetComponent<InteractorMeshBehavior>();
        if (meshBehavior == null) return;
        if (on) meshBehavior.Show();
        else meshBehavior.Hide();
    }
    

    private void HandleInteractionEvent(bool grabbing)
    {
        if (grabbing)
        {
            onGrab?.Invoke();
        }
        else
        {
            onRelease?.Invoke();
        }
    }
}
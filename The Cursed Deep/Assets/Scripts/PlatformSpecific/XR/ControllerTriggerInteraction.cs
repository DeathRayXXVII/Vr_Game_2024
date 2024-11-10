using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using NearFarInteractor = UnityEngine.XR.Interaction.Toolkit.Interactors.NearFarInteractor;


[RequireComponent(typeof(XRSimpleInteractable))]
public class ControllerTriggerInteraction : MonoBehaviour
{
    private XRSimpleInteractable _interactable;
    private IXRHoverInteractable _hoverInteractable;
    private object _rayInteractor;
        
    private readonly WaitForSeconds _waitOneSecond = new(1f);
    private readonly WaitForEndOfFrame _waitEndOfFrame = new();
    private Coroutine _hoverEnterCoroutine;
    private Coroutine _hoverExitCoroutine;
    
    public UnityEvent onTriggerPressed, onTriggerReleased;

    private void OnEnable()
    {
        _interactable = GetComponent<XRSimpleInteractable>();
        
        _interactable.hoverEntered.AddListener(ObjectHoverEnter);
        _interactable.hoverExited.AddListener(_ => ObjectHoverExit());
        
        _interactable.activated.AddListener(_ => TriggerPressed());
        // _interactable.deactivated.AddListener(_ => TriggerReleased());
    }
    
    private void OnDisable()
    {
        _interactable.hoverEntered.AddListener(ObjectHoverEnter);
        _interactable.hoverExited.AddListener(_ => ObjectHoverExit());
        
        _interactable.activated.RemoveListener(_ => TriggerPressed());
        // _interactable.deactivated.RemoveListener(_ => TriggerReleased());
    }
    
    private void ObjectHoverEnter(HoverEnterEventArgs args) 
    {
        if (!TrySetRayInteractor(args.interactorObject)) return;
        
        if (_hoverEnterCoroutine != null) return;
        // Debug.Log($"Hovering over {args.interactableObject}, with {args.interactorObject.GetType()} interactor");
        _hoverInteractable = args.interactableObject;
        _hoverEnterCoroutine = StartCoroutine(HoverEnterCoroutine());
    }

    private void ObjectHoverExit() => _hoverExitCoroutine ??= StartCoroutine(HoverExitCoroutine());
    
    private bool TrySetRayInteractor(object interactorObject)
    {
        var interactorObjectType = interactorObject.GetType();
        if (interactorObjectType == typeof(XRRayInteractor))
        {
            _rayInteractor = interactorObject as XRRayInteractor;
        }
        else if (interactorObjectType == typeof(NearFarInteractor))
        {
            _rayInteractor = interactorObject as NearFarInteractor;
        }
        else
        {
            return false;
        }
        return true;
    }

    private XRInputButtonReader GetInputReader()
    {
        switch (_rayInteractor)
        {
            case XRRayInteractor rayInteractor:
                return rayInteractor.activateInput;
            case NearFarInteractor nearFarInteractor:
                return nearFarInteractor.activateInput;
            default:
                return null;
        }
    }

    private void TriggerPressed() => onTriggerPressed?.Invoke();

    // private void TriggerReleased() => onTriggerReleased?.Invoke();
    private IEnumerator HoverExitCoroutine()
    {
        const int attempts = 3;
        for(var i = 0; i < attempts; i++)
        {
            yield return CheckHoverInteractableAndWait();
        }
        _hoverInteractable = null;
    }

    private IEnumerable CheckHoverInteractableAndWait()
    {
        if (_hoverInteractable == null) yield break;
        yield return _waitEndOfFrame;
    }
    
    private IEnumerator HoverEnterCoroutine()
    {
        XRInputButtonReader input = GetInputReader();
        
        if (input == null)
        {
            yield break;
        }
        // Debug.Log($"Starting HoverCoroutine for {_hoverInteractable}, with {_rayInteractor.GetType()} interactor, using {input}");
        while (true)
        {
            // Debug.Log($"Checking for trigger press on {_hoverInteractable}");
            if (input.inputActionPerformed.triggered)
            {
                Debug.Log($"Trigger pressed for {_hoverInteractable}");
                TriggerPressed();
                yield break;
            }
            yield return _waitOneSecond;
        }
    }
}
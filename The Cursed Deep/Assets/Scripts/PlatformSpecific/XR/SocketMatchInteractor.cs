using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class SocketMatchInteractor : XRSocketInteractor
{
    [System.Serializable]
    public struct PossibleMatch
    {
        public ID id;
    }

    [SerializeField] private List<PossibleMatch> triggerID;
    
    [SerializeField] private bool allowDebug;
    [SerializeField] private bool disableObjectOnSocket;
    [SerializeField] private bool deactivateGrabInteractionOnSocket;

    [SerializeField] private ID socketID;
    
    public UnityEvent onObjectSocketed;
    public UnityEvent onObjectUnsocketed;
    
    private IDBehavior _idBehavior;
    private readonly WaitForFixedUpdate _waitFixed = new();
    
    private int _originalInteractableLayerMask;
    private const int IsolatedLayerMask = 4;
    private XRGrabInteractable _socketedObject;
    private Collider _socketTrigger;
    
    private Coroutine _removeAndMoveCoroutine;
    
    private new void Awake()
    {
        interactionLayers = 6;
        
        if (socketID)
        {
            _idBehavior = gameObject.AddComponent<IDBehavior>();
            _idBehavior.id = socketID;
        }
        
        _socketTrigger = GetComponent<Collider>();
        if (!_socketTrigger)
        {
            if (allowDebug) Debug.LogWarning("Socket trigger appears to be null, Adding SphereCollider");
            _socketTrigger = gameObject.AddComponent<SphereCollider>();
        }

        if (_socketTrigger.isTrigger == false)
        {
            if (allowDebug) Debug.LogWarning("Socket trigger appears to be a collider, Setting to Trigger");
            _socketTrigger.isTrigger = true;
        }
        _removeAndMoveCoroutine = null;
        base.Awake();
    }
    
    public delegate void ObjectSocketedEvent(GameObject obj);
    public event ObjectSocketedEvent ObjectSocketed;
    public delegate void ObjectUnsocketedEvent([CanBeNull] GameObject obj);
    public event ObjectUnsocketedEvent ObjectUnsocketed;

    protected override void OnEnable()
    {
        GetComponent<XRSocketInteractor>().selectEntered.AddListener(OnObjectSocketed);
        GetComponent<XRSocketInteractor>().selectExited.AddListener(OnObjectUnsocketed);
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        GetComponent<XRSocketInteractor>().selectEntered.RemoveListener(OnObjectSocketed);
        GetComponent<XRSocketInteractor>().selectExited.RemoveListener(OnObjectUnsocketed);
        base.OnDisable();
    }
    
    private void OnObjectSocketed(SelectEnterEventArgs args)
    {
        ObjectSocketed?.Invoke(HandleSocketEventObject(args));
        onObjectSocketed.Invoke();
    }
    
    private void OnObjectUnsocketed(SelectExitEventArgs args)
    {
        ObjectUnsocketed?.Invoke(HandleSocketEventObject(args));
        onObjectUnsocketed.Invoke();
    }
    
    private GameObject HandleSocketEventObject(SelectEnterEventArgs args)
    {
        var obj = args.interactableObject.transform.gameObject;
        return HandleSocketEventObject(obj);
    }
    
    private GameObject HandleSocketEventObject(SelectExitEventArgs args)
    {
        var obj = args.interactableObject.transform.gameObject;
        return HandleSocketEventObject(obj);
    }
    
    private GameObject HandleSocketEventObject(GameObject obj)
    {
        var rb = obj.GetComponent<Rigidbody>();

        if (!rb || rb.isKinematic) return obj;
        
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;

        return obj;
    }
    
    private static ID FetchOtherID(GameObject interactable)
    {
        var idBehavior = interactable.transform.GetComponent<IDBehavior>();
        return idBehavior ? idBehavior.id : null;
    }
    
    private bool CheckId(Object nameId)
    {
        if (triggerID == null) return false;
        return nameId && triggerID.Any(obj => nameId == obj.id);
    }

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        return base.CanHover(interactable) && CheckId(FetchOtherID(interactable.transform.gameObject));
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return base.CanSelect(interactable) && CheckId(FetchOtherID(interactable.transform.gameObject));
    }
    
    public void AllowGrabInteraction(bool grabState)
    {
        if (grabState)
        {
            deactivateGrabInteractionOnSocket = false;
            
            if (!_socketedObject) return;
             _socketedObject.interactionLayers = _originalInteractableLayerMask;
        }
        else
        {
            deactivateGrabInteractionOnSocket = true;
            
            if (!_socketedObject) return;
            if (_socketedObject.interactionLayers.value == IsolatedLayerMask) return;
            _originalInteractableLayerMask = _socketedObject.interactionLayers.value;
            _socketedObject.interactionLayers = IsolatedLayerMask;
        }
    }
    
    public void EnableSocket() => SetSocketState(true);
    public void DisableSocket() => SetSocketState(false);
    public bool SocketState() => _socketTrigger.enabled;
    public bool GrabState() => !deactivateGrabInteractionOnSocket;
    
    private void SetSocketState(bool socketState)
    {
        if (_socketTrigger == null)
        {
            Debug.LogWarning("Socket Trigger appears to be null, returning");
            return;
        }
        _socketTrigger.enabled = socketState;
    }

    protected override bool StartSocketSnapping(XRGrabInteractable interactable)
    {
        _socketedObject = interactable;
        _socketedObject.StopAllCoroutines();
        
        if (disableObjectOnSocket)
        {
            StartCoroutine(DisableObject(interactable.gameObject));
            return false;
        }
        if (deactivateGrabInteractionOnSocket)
            AllowGrabInteraction(false);

        return base.StartSocketSnapping(interactable);
    }
    
    protected override bool EndSocketSnapping(XRGrabInteractable interactable)
    {
        if (deactivateGrabInteractionOnSocket)
            AllowGrabInteraction(true);
        return base.EndSocketSnapping(interactable);
    }
    
    private IEnumerator DisableObject(GameObject obj)
    {
        yield return _waitFixed;
        yield return _waitFixed;
        yield return _waitFixed;
        obj.SetActive(false);
    }
    
    public void RemoveAndMoveSocketObject(Transform copyTransform)
    {
        if (!_socketedObject)
        {
            Debug.LogWarning("[WARNING] Socket object appears to be null.", this); return;
        }
        if (interactablesSelected.Count == 0) return;
        DisableSocket();
        _removeAndMoveCoroutine ??= StartCoroutine(PerformRemoveAndMove(copyTransform.position, copyTransform.rotation));
    }

    public GameObject RemoveAndMoveSocketObject(Vector3 position, Quaternion rotation)
    {
        if (!_socketedObject)
        {
            Debug.LogWarning("[WARNING] Socket object appears to be null.", this); 
            return null;
        }
        if (interactablesSelected.Count == 0) return null;
        
        var obj = _socketedObject.gameObject;
        
        DisableSocket();
        
        _removeAndMoveCoroutine ??= StartCoroutine(PerformRemoveAndMove(position, rotation));
        
        return _removeAndMoveCoroutine == null ? null : obj;
    }
 
    private IEnumerator PerformRemoveAndMove(Vector3 position, Quaternion rotation)
    {
        var obj = _socketedObject.gameObject;
        var rb = obj.GetComponent<Rigidbody>();
        yield return _waitFixed;
        
        if (interactablesSelected.Count != 0) interactionManager.CancelInteractableSelection(interactablesSelected[0]);
        yield return _waitFixed;
        
        if (rb)
        {
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
        }
        
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        
        yield return _waitFixed;
        
        _socketedObject = null;
        yield return _waitFixed;
        
        _socketTrigger.enabled = true;
        _removeAndMoveCoroutine = null;
    }
}
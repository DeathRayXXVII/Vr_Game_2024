using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

using Object = UnityEngine.Object;

public class SocketMatchInteractor : XRSocketInteractor
{
    [System.Serializable]
    public struct PossibleMatch
    {
        public ID id;
    }

    [SerializeField]
    private List<PossibleMatch> triggerID;
    
    public bool allowDebug;
    public bool disableObjectOnSocket;

    public ID socketID;
    
    public UnityEvent onObjectSocketed;
    public UnityEvent onObjectUnsocketed;
    
    private IDBehavior _idBehavior;
    private WaitForFixedUpdate _wffu = new WaitForFixedUpdate();
    
    private XRGrabInteractable _socketedObject;
    private Collider _socketTrigger;
    
    private Coroutine _removeAndMoveCoroutine;
    
    private new void Awake()
    {
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
        ObjectSocketed?.Invoke(args.interactableObject.transform.gameObject);
        onObjectSocketed.Invoke();
    }
    
    private void OnObjectUnsocketed(SelectExitEventArgs args)
    {
        ObjectUnsocketed?.Invoke(args.interactableObject.transform.gameObject);
        onObjectUnsocketed.Invoke();
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

    protected override bool StartSocketSnapping(XRGrabInteractable interactable)
    {
        _socketedObject = interactable;
        _socketedObject.StopAllCoroutines();
        if (!disableObjectOnSocket) return base.StartSocketSnapping(interactable);
        StartCoroutine(DisableObject(interactable.gameObject));
        return false;
    }
    
    private IEnumerator DisableObject(GameObject obj)
    {
        yield return _wffu;
        yield return _wffu;
        yield return _wffu;
        obj.SetActive(false);
    }
    
    public void UnsocketObject()
    {
        if (!_socketedObject) return;
        RemoveAndMoveSocketObject(Vector3.zero, Quaternion.identity);
        _socketedObject = null;
    }
    
    protected override bool EndSocketSnapping(XRGrabInteractable interactable)
    {
        return base.EndSocketSnapping(interactable);
    }
    
    public void RemoveAndMoveSocketObject(Transform copyTransform)
    {
        if (!_socketedObject){Debug.LogWarning("SOCKET OBJECT APPEARS TO BE NULL"); return;}
        if (interactablesSelected.Count == 0) return;
        _socketTrigger.enabled = false;
        _removeAndMoveCoroutine ??= StartCoroutine(PerformRemoveAndMove(copyTransform.position, copyTransform.rotation));
    }

    public GameObject RemoveAndMoveSocketObject(Vector3 position, Quaternion rotation)
    {
        if (!_socketedObject){Debug.LogWarning("SOCKET OBJECT APPEARS TO BE NULL"); return null;}
        if (interactablesSelected.Count == 0) return null;
        var obj = _socketedObject.gameObject;
        _socketTrigger.enabled = false;
        _removeAndMoveCoroutine ??= StartCoroutine(PerformRemoveAndMove(position, rotation));
        return _removeAndMoveCoroutine == null ? null : obj;
    }
 
    private IEnumerator PerformRemoveAndMove(Vector3 position, Quaternion rotation)
    {
        var obj = _socketedObject.gameObject;
        yield return _wffu;
        interactionManager.CancelInteractableSelection(interactablesSelected[0]);
        yield return _wffu;
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        yield return _wffu;
        _socketedObject = null;
        yield return _wffu;
        _socketTrigger.enabled = true;
        _removeAndMoveCoroutine = null;
    }
}
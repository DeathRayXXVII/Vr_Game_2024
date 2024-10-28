using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using static ZPTools.Utility.UtilityFunctions;

public class CannonManager : MonoBehaviour
{
    private WaitForFixedUpdate _wffu;
    
    public UnityEvent onSuccessfulFire;
    
    [Header("Ammo:")]
    [SerializeField] private GameObject ammoEntity;
    private GameObject _socketedAmmo;
    private MeshFilter _ammoMeshFilter;
    private MeshRenderer _ammoMeshRenderer;
    
    [Header("Fire Physics System:")]
    [SerializeField, SteppedRange(rangeMin:0.0f, rangeMax:1000.0f, step:0.01f)] private float propellantForce = 10.0f;
    [SerializeField] private Transform muzzlePosition, breechPosition;
    [SerializeField] private SocketMatchInteractor reloadSocket;
    private Vector3 forceVector => !muzzlePosition || !breechPosition ? Vector3.zero : (ejectionPoint - ingitionPoint).normalized;
    private Vector3 momentumVector  => forceVector * propellantForce;
    private Vector3 ingitionPoint => !breechPosition ? Vector3.zero : breechPosition.position;
    private Vector3 ejectionPoint  => !muzzlePosition ? Vector3.zero : muzzlePosition.position;
    
    private List <GameObject> _currentAmmoList;
    private bool _isLoaded;
    private GameObject _ammoObj;
    private Coroutine _addForceCoroutine;

    [Header("Model Animation:")]
    [SerializeField] private Animator _modelAnimator;
    [SerializeField] private string _fireAnimationTrigger = "Fire";
    [SerializeField] private string _loadAnimationTrigger = "Load";
    
    private void Awake()
    {
        _wffu = new WaitForFixedUpdate();
        _addForceCoroutine = null;
        
        if (!_modelAnimator) _modelAnimator = GetComponent<Animator>();
    } 
    
    private static bool _errorsLogged = false;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (_errorsLogged) return;

        if (!muzzlePosition) Debug.LogError("Muzzle Position is not set.", this);
        if (!breechPosition) Debug.LogError("Breech Position is not set.", this);
        if (!reloadSocket) Debug.LogError("Reload Socket is not set.", this);

        _errorsLogged = true;
#endif
    }

    private void OnEnable()
    {
        if (!reloadSocket) return;
        reloadSocket.ObjectSocketed += LoadCannon;
        reloadSocket.ObjectUnsocketed += UnloadCannon;
    }

    private void OnDisable()
    {
        if (!reloadSocket) return;
        reloadSocket.ObjectSocketed -= LoadCannon;
        reloadSocket.ObjectUnsocketed -= UnloadCannon;
    }

    public void Fire()
    {
        if (!_ammoObj)
        {
            Debug.LogWarning($"No ammo found in {gameObject.name}", this);
            return;
        }

        if (!_isLoaded)
        {
            Debug.LogWarning($"{gameObject.name} has not been loaded.", this);
            return;
        }
        
        var ammoRb = _ammoObj.GetComponent<Rigidbody>();
        
        if (_addForceCoroutine != null){ _ammoObj.SetActive(false); return;}
        _ammoObj.SetActive(true);
        _modelAnimator.SetTrigger(_fireAnimationTrigger);
        onSuccessfulFire.Invoke();
        _addForceCoroutine ??= StartCoroutine(AddForceToAmmo(ammoRb));
        UnloadCannon(null);
    }

    private void LoadCannon(GameObject obj)
    {
        if (reloadSocket.socketScaleMode != SocketScaleMode.Fixed)
            reloadSocket.socketScaleMode = SocketScaleMode.Fixed;
        reloadSocket.fixedScale = Vector3.one * 0.01f;
        _isLoaded = true;
        _modelAnimator.SetTrigger(_loadAnimationTrigger);
        _ammoObj = GetAmmo();
        _ammoMeshFilter = GetObjectComponent<MeshFilter>(_ammoObj);
        _ammoMeshRenderer = GetObjectComponent<MeshRenderer>(_ammoObj);
        var objMeshFilter = GetObjectComponent<MeshFilter>(obj);
        var objMeshRenderer = GetObjectComponent<MeshRenderer>(obj);
        if (_ammoMeshFilter && objMeshFilter)
        {
            _ammoMeshFilter.mesh = objMeshFilter.mesh;
        }
        if (_ammoMeshRenderer && objMeshRenderer)
        {
            _ammoMeshRenderer.material = objMeshRenderer.material;
        }
    }

    private void UnloadCannon([CanBeNull] GameObject obj)
    {
        if (reloadSocket.socketScaleMode != SocketScaleMode.Fixed)
            reloadSocket.socketScaleMode = SocketScaleMode.Fixed;
        reloadSocket.fixedScale = Vector3.one / 0.01f;
        reloadSocket.UnsocketObject();
        _isLoaded = false;
        if (!obj) return;
        GetObjectComponent<PooledObjectBehavior>(obj)?.TriggerRespawn();
    }

    private GameObject GetAmmo()
    {
        _currentAmmoList ??= new List<GameObject>();
        foreach (var ammoObj in _currentAmmoList.Where(ammoObj => !ammoObj.activeSelf))
        {
            ammoObj.transform.position = muzzlePosition.position;
            ammoObj.transform.rotation = muzzlePosition.rotation;
            return ammoObj;
        }
        var newAmmo = Instantiate(ammoEntity, muzzlePosition.position, muzzlePosition.rotation);
        newAmmo.SetActive(false);
        _currentAmmoList.Add(newAmmo);
        return newAmmo;
    }
    
    private IEnumerator AddForceToAmmo(Rigidbody ammoRb)
    {
        ammoRb.isKinematic = false;
        ammoRb.useGravity = true;
        ammoRb.velocity = Vector3.zero;
        ammoRb.angularVelocity = Vector3.zero;
        
        yield return _wffu;
        yield return _wffu;
        yield return _wffu;
        yield return null;
        
        ammoRb.AddForce(momentumVector, ForceMode.Impulse);
        _addForceCoroutine = null; 
    }
    
#if UNITY_EDITOR
    [Range(0, 100)] public int simulationTime = 80;
    
    private void OnDrawGizmos()
    {
    if (!muzzlePosition || !breechPosition) return;
    
    Gizmos.color = Color.red;
    Gizmos.DrawSphere(ingitionPoint, 0.2f);
    Gizmos.DrawLine(ingitionPoint, ejectionPoint);

    // Simulate the trajectory
    Vector3 position = ejectionPoint;
    Vector3 newposition = position;
    Vector3 velocity = momentumVector;
    float timeStep = 0.025f;
    var count = Mathf.Clamp(propellantForce * (simulationTime * 0.01f), 0, 100);
    for (int i = 0; i < count; i++)
    {
        Gizmos.color = Color.Lerp(Color.red, Color.yellow, i / (count * 0.9f));
        float radius = Mathf.Lerp(0.2f, 0.01f, i / (count * 0.9f));
        if (Physics.Raycast(position, velocity, out var hit, 0.1f))
        {
            newposition = hit.point;
            // Gizmos.DrawLine(position, newposition);
            Gizmos.DrawSphere(position, radius);
            break;
            
        }
        newposition += velocity * timeStep;
        // Gizmos.DrawLine(position, newposition);
        Gizmos.DrawSphere(position, radius);
        position = newposition;
        velocity += Physics.gravity * timeStep;
    }
    }
#endif
}
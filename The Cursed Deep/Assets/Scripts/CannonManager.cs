using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class CannonManager : MonoBehaviour
{
    private WaitForFixedUpdate _wffu;
    
    public UnityEvent onSuccessfulFire;
    
    public GameObject ammoPrefab;
    private GameObject _socketedAmmo;
    private MeshFilter _ammoMeshFilter;
    private MeshRenderer _ammoMeshRenderer;
    
    [SerializeField, Range(0.0f, 1000.0f), Step(0.01f)] private float propellantForce = 10.0f;
    [SerializeField] private Transform muzzlePosition, breechPosition;
    [SerializeField] private SocketMatchInteractor reloadSocket;
    private Vector3 forceVector => CalculateFireDirection();
    private Vector3 momentumVector  => forceVector * propellantForce;
    private Vector3 ingitionPoint => breechPosition.position;
    private Vector3 ejectionPoint  => muzzlePosition.position;
    
    private List <GameObject> _currentAmmoList;
    private bool _isLoaded;
    private GameObject _ammoObj;
    private Coroutine _addForceCoroutine; 

    private void Awake()
    {
        _wffu = new WaitForFixedUpdate();
        _addForceCoroutine = null;
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
        onSuccessfulFire.Invoke();
        _addForceCoroutine ??= StartCoroutine(AddForceToAmmo(ammoRb));
        UnloadCannon(null);
    }

    public void LoadCannon(GameObject obj)
    {
        _isLoaded = true;
        _ammoObj = GetAmmo();
    }

    private void UnloadCannon([CanBeNull] GameObject obj)
    {
        _isLoaded = false;
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
        var newAmmo = Instantiate(ammoPrefab, muzzlePosition.position, muzzlePosition.rotation);
        _currentAmmoList.Add(newAmmo);
        return newAmmo;
    }
    
    private Vector3 CalculateFireDirection()
    {
        if (!muzzlePosition || !breechPosition) return Vector3.zero;
        var x = ejectionPoint.x - ingitionPoint.x;
        var y = ejectionPoint.y - ingitionPoint.y;
        var z = ejectionPoint.z - ingitionPoint.z;
        var direction = new Vector3(x, y, z).normalized;
        
        return direction;
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
    Gizmos.DrawSphere(ingitionPoint, 0.1f);
    Gizmos.DrawLine(ingitionPoint, ejectionPoint);

    // Simulate the trajectory
    
    Vector3 position = ejectionPoint;
    Vector3 newposition = position;
    Vector3 velocity = momentumVector;
    float timeStep = 0.05f;
    var count = Mathf.Clamp(propellantForce * (simulationTime * 0.01f), 0, 100);
    for (int i = 0; i < count; i++)
    {
        Gizmos.color = Color.Lerp(Color.red, Color.clear, i / (count * 0.9f));
        if (Physics.Raycast(position, velocity, out var hit, 0.1f))
        {
            newposition = hit.point;
            Gizmos.DrawLine(position, newposition);
            break;
        }
        // Gizmos.DrawSphere(position, 0.05f);
        newposition += velocity * timeStep;
        Gizmos.DrawLine(position, newposition);
        position = newposition;
        velocity += Physics.gravity * timeStep;
    }
    }
#endif
}
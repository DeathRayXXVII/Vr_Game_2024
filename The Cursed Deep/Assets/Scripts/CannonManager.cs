using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using static ZPTools.Utility.UtilityFunctions;

public class CannonManager : MonoBehaviour
{
    [Header("Ammo:")]
    [SerializeField] private GameObject ammoEntity;
    [SerializeField] private SocketMatchInteractor _ammoSpawnSocket;
    [SerializeField, Range(1, 50)] private int ammoDespawnTime = 30;
    private readonly List<GameObject> _despawningAmmoList = new();
    private GameObject _loadedAmmo;
    private Vector3 _ammoScale;
    private MeshFilter _ammoMeshFilter;
    private MeshRenderer _ammoMeshRenderer;
    
    [Header("Fire Physics System:")]
#if UNITY_EDITOR
    public bool solidLine;
    [Range(0, 100)] public int simulationTime = 80;
#endif
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
    private Rigidbody _ammoRb;
    private Coroutine _addForceCoroutine;

    [Header("Model Animation:")]
    [SerializeField] private Animator _modelAnimator;
    [SerializeField] private string _fireAnimationTrigger = "Fire";
    [SerializeField] private string _loadAnimationTrigger = "Load";
    
    [Header("State Events:")]
    public UnityEvent onSuccessfulFire;
    public UnityEvent onLoaded;
    
    private readonly WaitForFixedUpdate _waitFixedUpdate = new();
    private readonly WaitForSeconds _waitSingleSecond = new(1f);
    
    private void Awake()
    {
        if (!_modelAnimator) _modelAnimator = GetComponent<Animator>();
        if (reloadSocket.socketScaleMode != SocketScaleMode.Fixed)
            reloadSocket.socketScaleMode = SocketScaleMode.Fixed;
    } 
    
    private static bool _errorsLogged;

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
        if (reloadSocket)
            reloadSocket.ObjectSocketed += LoadCannon;
        
        if (_ammoSpawnSocket) 
            _ammoSpawnSocket.ObjectUnsocketed += HandleActivatedAmmo;
    }

    private void OnDisable()
    {
        if (reloadSocket)
            reloadSocket.ObjectSocketed -= LoadCannon;
        
        if (_ammoSpawnSocket) 
            _ammoSpawnSocket.ObjectUnsocketed -= HandleActivatedAmmo;
        
        _despawningAmmoList.Clear();
    }

    public SocketMatchInteractor ammoSpawnSocket
    {
        get => _ammoSpawnSocket;
        set
        {
            if (_ammoSpawnSocket)
            {
                _ammoSpawnSocket.ObjectUnsocketed -= HandleActivatedAmmo;
            }
            _ammoSpawnSocket = value;
            if (_ammoSpawnSocket)
            {
                _ammoSpawnSocket.ObjectUnsocketed += HandleActivatedAmmo;
            }
        }
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
        
        _ammoSpawnSocket.gameObject.SetActive(true);
        
        if (_addForceCoroutine != null)
            return;
        
        _ammoObj.SetActive(true);
        
        if (_modelAnimator.GetBool(_loadAnimationTrigger)) _modelAnimator.ResetTrigger(_loadAnimationTrigger);
        _modelAnimator.SetTrigger(_fireAnimationTrigger);
        
        onSuccessfulFire.Invoke();
        
        _addForceCoroutine ??= StartCoroutine(AddForceToAmmo());
        
        UnloadCannon();
    }

    private const float RESIZE_FACTOR = 0.1f;
    private static readonly Vector3 LOAD_SCALE = Vector3.one * RESIZE_FACTOR;
    private Vector3 UNLOAD_SCALE = LOAD_SCALE / RESIZE_FACTOR;
    private void LoadCannon(GameObject obj)
    {
        if (_isLoaded) return;
        
        _isLoaded = true;
        _loadedAmmo = obj;
        _ammoScale = obj.transform.localScale;
        
        reloadSocket.fixedScale = LOAD_SCALE;
        _modelAnimator.SetTrigger(_loadAnimationTrigger);
        
        _ammoObj = GetAmmo();
        _ammoRb = AdvancedGetComponent<Rigidbody>(_ammoObj);
        _ammoMeshFilter = AdvancedGetComponent<MeshFilter>(_ammoObj);
        _ammoMeshRenderer = AdvancedGetComponent<MeshRenderer>(_ammoObj);
        var objMeshFilter = AdvancedGetComponent<MeshFilter>(obj);
        var objMeshRenderer = AdvancedGetComponent<MeshRenderer>(obj);
        
        if (_ammoMeshFilter && objMeshFilter)
            _ammoMeshFilter.mesh = objMeshFilter.mesh;
            
        if (_ammoMeshRenderer && objMeshRenderer)
            _ammoMeshRenderer.material = objMeshRenderer.material;
        
        onLoaded.Invoke();
    }

    private void UnloadCannon()
    {
        reloadSocket.fixedScale = UNLOAD_SCALE;
        if (_loadedAmmo)
        {
            var currentScale = _loadedAmmo.transform.localScale;
            
            if (currentScale != _ammoScale) 
                _loadedAmmo.transform.localScale = _ammoScale;
            
            reloadSocket.RemoveAndMoveSocketObject(Vector3.zero, Quaternion.identity);
            
            StartCoroutine(DespawnAmmo(AdvancedGetComponent<PooledObjectBehavior>(_loadedAmmo)));
        }
        _loadedAmmo = null;
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
        var newAmmo = Instantiate(ammoEntity, muzzlePosition.position, muzzlePosition.rotation);
        newAmmo.SetActive(false);
        _currentAmmoList.Add(newAmmo);
        return newAmmo;
    }
    
    private IEnumerator AddForceToAmmo()
    {
        if (!_ammoRb) yield break;
        _ammoRb.isKinematic = false;
        _ammoRb.useGravity = true;
        _ammoRb.velocity = Vector3.zero;
        _ammoRb.angularVelocity = Vector3.zero;
        
        yield return _waitFixedUpdate;
        yield return _waitFixedUpdate;
        yield return _waitFixedUpdate;
        yield return null;
        
        _ammoRb.AddForce(momentumVector, ForceMode.Impulse);
        _addForceCoroutine = null; 
    }
    
    private void HandleActivatedAmmo(GameObject ammo)
    {
        if (_despawningAmmoList.Contains(ammo))
        {
            Debug.LogWarning($"Ammo is already despawning: {ammo.name}", this);
            return;
        }
        _ammoSpawnSocket.gameObject.SetActive(false);
        
        _despawningAmmoList.Add(ammo);
        // Debug.Log($"{ammo.name} has been unsocketed.", this);
        
        StartCoroutine(WaitToDespawnAmmo(ammo));
    }
    
    private IEnumerator WaitToDespawnAmmo(GameObject ammo)
    {
        // Debug.Log($"Waiting to despawn Ammo: {ammo.name}", this);
        var pooledObjectBehavior = AdvancedGetComponent<PooledObjectBehavior>(ammo);
        
        var time = Time.time;
        var timeToDespawn = time + ammoDespawnTime;
        
// #if UNITY_EDITOR
//         var debugSpacer = 0;
//         const int mod = 20;
// #endif
        
        while (time < timeToDespawn)
        {
            
// #if UNITY_EDITOR
//             if (debugSpacer++ % mod == 0)
//             {
//                 Debug.Log($"Time to Despawn Ammo: {ammo.name} => {timeToDespawn - time:0.00}", this);
//             }
// #endif

            if ((_isLoaded && _loadedAmmo == ammo) || !ammo.activeInHierarchy)
            {
                // Debug.Log($"{ammo.name} has been loaded or is not active.", this);
                _ammoSpawnSocket.gameObject.SetActive(true);
                yield break;
            }

            time += Time.deltaTime;
            yield return null;
        }
        
        // Debug.Log($"Attempting to Despawn Ammo: {ammo.name} => {pooledObjectBehavior != null}", this);
        yield return DespawnAmmo(pooledObjectBehavior);
    }
    
    private IEnumerator DespawnAmmo(PooledObjectBehavior ammoPoolBehavior)
    {
        // Debug.Log($"Despawning Ammo: {ammoPoolBehavior.gameObject.name}", this);
        _ammoSpawnSocket.gameObject.SetActive(true);
        
        if (_despawningAmmoList.Contains(ammoPoolBehavior.gameObject))
            _despawningAmmoList.Remove(ammoPoolBehavior.gameObject);
        
        yield return _waitFixedUpdate;
        ammoPoolBehavior.TriggerRespawn();
    }
    
#if UNITY_EDITOR
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
                if (solidLine) Gizmos.DrawLine(position, newposition);
                else Gizmos.DrawSphere(position, radius);
                break;
                
            }
            newposition += velocity * timeStep;
            if (solidLine) Gizmos.DrawLine(position, newposition);
            else Gizmos.DrawSphere(position, radius);
            position = newposition;
            velocity += Physics.gravity * timeStep;
        }
    }
#endif
}
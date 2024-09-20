using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CannonManager : MonoBehaviour
{
    private WaitForFixedUpdate _wffu;
    
    public UnityEvent onSuccessfulFire;
    
    public GameObject ammoPrefab;
    public float fireForce;
    [SerializeField] private Transform barrelExitVelocityPosition, barrelInitialVelocityPosition;
    private Vector3 _forceVector;
    
    private List <GameObject> _currentAmmoList;
    private bool _isLoaded;
    private GameObject _ammoObj;
    private Coroutine _addForceCoroutine; 

    private void Awake()
    {
        _wffu = new WaitForFixedUpdate();
        _addForceCoroutine = null;
        _forceVector = Vector3.zero;
    }

    public void Fire()
    {
        // var ammoObj = ammoSocket.RemoveAndMoveSocketObject(barrelExitVelocityPosition.position, barrelExitVelocityPosition.rotation);
        if(_ammoObj == null) {Debug.LogWarning($"No ammo found in {gameObject.name}"); return;}
        if (!_isLoaded) {Debug.LogWarning($"{gameObject.name} has not been loaded."); return;}
        
        var ammoRb = _ammoObj.GetComponent<Rigidbody>();
        
        if (_addForceCoroutine != null){ _ammoObj.SetActive(false); return;}
        _ammoObj.SetActive(true);
        onSuccessfulFire.Invoke();
        _addForceCoroutine ??= StartCoroutine(AddForceToAmmo(ammoRb));
        UnloadCannon();
    }

    public void LoadCannon()
    {
        _isLoaded = true;
        _ammoObj = GetAmmo();
    }

    private void UnloadCannon()
    {
        _isLoaded = false;
    }

    private GameObject GetAmmo()
    {
        _currentAmmoList ??= new List<GameObject>();
        foreach (var ammoObj in _currentAmmoList.Where(ammoObj => !ammoObj.activeSelf))
        {
            ammoObj.transform.position = barrelExitVelocityPosition.position;
            ammoObj.transform.rotation = barrelExitVelocityPosition.rotation;
            return ammoObj;
        }
        var newAmmo = Instantiate(ammoPrefab, barrelExitVelocityPosition.position, barrelExitVelocityPosition.rotation);
        _currentAmmoList.Add(newAmmo);
        return newAmmo;
    }
    
    private Vector3 CalculateFireDirection()
    {
        var x = barrelExitVelocityPosition.position.x - barrelInitialVelocityPosition.position.x;
        var y = barrelExitVelocityPosition.position.y - barrelInitialVelocityPosition.position.y;
        var z = barrelExitVelocityPosition.position.z - barrelInitialVelocityPosition.position.z;
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
        
        _forceVector = CalculateFireDirection();
        
        ammoRb.AddForce(_forceVector * fireForce, ForceMode.Impulse);
        _addForceCoroutine = null; 
    }
}
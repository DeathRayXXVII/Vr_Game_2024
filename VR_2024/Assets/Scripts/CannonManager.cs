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
    private Vector3 _fireDirection;
    public SocketMatchInteractor ammoSocket;
    
    private List <GameObject> _currentAmmoList;
    private bool _isLoaded;
    private GameObject _ammoObj;
    private Coroutine _addForceCoroutine; 

    private void Awake()
    {
        _wffu = new WaitForFixedUpdate();
        _addForceCoroutine = null;
        _fireDirection = Vector3.zero;
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
        
        if (_fireDirection == Vector3.zero){
            var fireDirectionX = barrelExitVelocityPosition.position.x - barrelInitialVelocityPosition.position.x;
            var fireDirectionZ = barrelExitVelocityPosition.position.z - barrelInitialVelocityPosition.position.z;

            if (Math.Abs(fireDirectionX) > Math.Abs(fireDirectionZ))
            {
                _fireDirection.x = (fireDirectionX > 0) ? 1 : -1;
                _fireDirection.z = 0;
            }
            else
            {
                _fireDirection.z = (fireDirectionZ > 0) ? 1 : -1;
                _fireDirection.x = 0;
            }

            _fireDirection.y = (barrelExitVelocityPosition.position.z - barrelInitialVelocityPosition.position.z > 0)
                ? barrelExitVelocityPosition.position.z - barrelInitialVelocityPosition.position.z
                : 0.1f;
        }
        
        ammoRb.AddForce(_fireDirection * fireForce, ForceMode.Impulse);
        _addForceCoroutine = null; 
    }
}
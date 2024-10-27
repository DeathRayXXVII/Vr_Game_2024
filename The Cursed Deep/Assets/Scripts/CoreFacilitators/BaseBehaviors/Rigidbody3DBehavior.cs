using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Rigidbody3DBehavior : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private bool allowDebug;
#endif
    public Rigidbody rigidBody;
    public bool useGravity;
    
    private void Start()
    {
        if (!rigidBody) rigidBody = GetComponent<Rigidbody>();
        if (!rigidBody) Debug.LogError("No Rigidbody found on " + gameObject.name);
        SetGravity(useGravity);
    }
    
    public void AddForce(Vector3 dir, float power) => AddForce(dir, power, ForceMode.Impulse);
    
    public void AddForce(Vector3 dir, float power, ForceMode mode) => rigidBody.AddForce(dir * power, mode);
    
    public void SetGravity(bool value) => rigidBody.useGravity = value;
    
    public void ZeroOutVelocity()
    {
        if (rigidBody.isKinematic) return;
        rigidBody.velocity = Vector3.zero;
    }
    
    public void ZeroOutAngularVelocity()
    {
        if (rigidBody.isKinematic) return;
        rigidBody.angularVelocity = Vector3.zero;
    }
    
    public void FreezeRigidbody() => rigidBody.constraints = RigidbodyConstraints.FreezeAll;
    
    public void UnFreezeRigidbody() => rigidBody.constraints = RigidbodyConstraints.None;
    
#if UNITY_EDITOR
    private void OnCollisionEnter(Collision other)
    {
        if (allowDebug) Debug.Log($"Collision detected with: {other.gameObject}");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (allowDebug) Debug.Log($"Trigger detected with: {other.gameObject}");
    }
#endif
}

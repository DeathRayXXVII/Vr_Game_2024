using UnityEngine;

public class UIFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 lookAtPosition;

    private void Update()
    {
        lookAtPosition = new Vector3( target.position.x, transform.position.y, target.position.z);
        transform.LookAt(lookAtPosition);
        transform.forward = -transform.forward;
    }
}
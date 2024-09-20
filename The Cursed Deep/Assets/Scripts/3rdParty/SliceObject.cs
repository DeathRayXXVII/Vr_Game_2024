// https://www.youtube.com/watch?v=GQzW6ZJFQ94&t=22s
using UnityEngine;
using EzySlice;

public class SliceObject : MonoBehaviour
{
    public VelocityEstimator velocictyEstimator;
    public LayerMask sliceableLayer;
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public Material crossSectionMaterial;
    public float cutForce = 100f;

    private void FixedUpdate()
    {
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer);
        if (hasHit)
        {	
            GameObject target = hit.transform.gameObject;
            Slice(target);
        }
    }	

    public void Slice(GameObject target)
    {
        Vector3 velocity = velocictyEstimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();
	
        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);
	
        if(hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
            SetupSlicedComponent(upperHull);
		
            GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);
            SetupSlicedComponent(lowerHull);
		
            target.SetActive(false);
        }
    }
    
    public void SetupSlicedComponent(GameObject obj)
    {
        Rigidbody rb = obj.AddComponent<Rigidbody>();
        MeshCollider mCollider = obj.AddComponent<MeshCollider>();
        mCollider.convex = true;
        
        rb.AddExplosionForce(cutForce, endSlicePoint.position, 1f);
    }   
}

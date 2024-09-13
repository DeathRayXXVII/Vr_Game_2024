using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class ObjectInstancer : MonoBehaviour, INeedButton
{
    [SerializeField] private InstancerData instancerData;
    [SerializeField] private bool instantiateOnStart;
    
    public void SetInstancerData(InstancerData data) { instancerData = data; }
    
    private void Start()
    {
        if (instancerData == null) 
        { 
            Debug.LogError("InstancerData is missing.");
            return;
        }
        if (instantiateOnStart) InstantiateObjects();
    }
    
    public void InstantiateObjects()
    {
        foreach (var instance in instancerData.instances)
        {
            var instanceOffset = Vector3.zero;
            if (instance.offset != null) {instanceOffset = instance.offset;}

            var finalOffset = !instance.excludePrefabOffset && instancerData.prefabOffset != null ? instanceOffset + instancerData.prefabOffset.value : instanceOffset;
            InstantiateObject(instance.targetPosition, finalOffset);
        }
    }
    
    private void InstantiateObject(TransformData location, Vector3 offset)
    {
        var newInstance = Instantiate(instancerData.prefab, location.position, location.rotation);
        newInstance.transform.localPosition += location.rotation * offset;
        newInstance.transform.SetParent(transform);
    }

    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)> {(InstantiateObjects, "Instantiate Objects")};
    }
}

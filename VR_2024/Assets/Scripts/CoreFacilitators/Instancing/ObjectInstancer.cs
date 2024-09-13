using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class ObjectInstancer : MonoBehaviour, INeedButton
{
    public GameObject prefab;
    public Vector3Data prefabOffset;
    
    [System.Serializable]
    private class InstanceData
    {
        public TransformData targetPosition;
        public Vector3Data offset;
        public bool excludePrefabOffset;
    }
    
    [SerializeField] private InstanceData[] instances;
    
    [SerializeField] private bool instantiateOnStart;
    
    private void Start()
    {
        if (instantiateOnStart) InstantiateObjects();
    }
    
    public void InstantiateObjects()
    {
        foreach (var instance in instances)
        {
            var instanceOffset = Vector3.zero;
            if (instance.offset != null) {instanceOffset = instance.offset;}

            var finalOffset = !instance.excludePrefabOffset && prefabOffset != null ? instanceOffset + prefabOffset.value : instanceOffset;
            InstantiateObject(instance.targetPosition, finalOffset);
        }
    }
    
    private void InstantiateObject(TransformData location, Vector3 offset)
    {
        var newInstance = Instantiate(prefab, location.position, location.rotation);
        newInstance.transform.localPosition += location.rotation * offset;
        newInstance.transform.SetParent(transform);
    }

    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)> {(InstantiateObjects, "Instantiate Objects")};
    }
}

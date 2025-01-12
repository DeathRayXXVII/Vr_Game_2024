using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ZPTools.Interface;

public class ObjectInstancer : MonoBehaviour, INeedButton
{
    [SerializeField] private InstancerData instancerData;
    [SerializeField] private string groupName;
    
    public UnityEvent onCompleted;
    
    private GameObject _groupObject;
    
    public void SetInstancerData(InstancerData data) => instancerData = data;

    private Transform parentObject => 
        instancerData.noParent ? null : instancerData.hierarchyParent ? instancerData.hierarchyParent.transform : transform;

    private void Start()
    {
        if (!instancerData) 
        { 
            Debug.LogError("InstancerData is missing.", this);
        }
    }
    
    private void SetupGroupObject()
    {
        var objectName = !string.IsNullOrEmpty(groupName) ? groupName :
            parentObject ? $"{parentObject.name} - instances" : "World - instances";
        _groupObject = GameObject.Find(objectName);
        if (!_groupObject) _groupObject = new GameObject(objectName);
        if (parentObject) _groupObject.transform.SetParent(parentObject.transform);
        _groupObject.transform.localPosition = Vector3.zero;
    }
    
    public void InstantiateObjects()
    {
        SetupGroupObject();
        
        foreach (var instanceData in instancerData.instancesData)
        {
            var instanceOffset = Vector3.zero;
            if (instanceData.instanceOffset) {instanceOffset = instanceData.instanceOffset;}

            var finalOffset = !instanceData.excludePrefabOffset && instancerData.prefabOffset ? instanceOffset + instancerData.prefabOffset.value : instanceOffset;
            InstantiateObject(instanceData.targetPosition, finalOffset);
        }
        onCompleted?.Invoke();
    }
    
    private void InstantiateObject(TransformData location, Vector3 offset)
    {
        var newInstance = Instantiate(instancerData.prefab, location.position, location.rotation);
        newInstance.transform.localPosition += location.rotation * offset;
        newInstance.transform.SetParent(_groupObject.transform);
        instancerData.instances.Add(newInstance);
    }
    
    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)> {(InstantiateObjects, "Instantiate Objects")};
    }
}

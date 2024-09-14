using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "InstancerData", menuName = "Data/ManagerData/InstancerData")]
public class InstancerData : ScriptableObject
{
    public GameObject prefab;
    public Vector3Data prefabOffset;
    
    [System.Serializable]
    public class InstanceData
    {
        public TransformData targetPosition;
        public Vector3Data offset;
        public bool excludePrefabOffset;
    }
    
    public List<InstanceData> instances = new();
}

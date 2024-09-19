using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "InstancerData", menuName = "Data/ManagerData/InstancerData")]
public class InstancerData : ScriptableObject
{
    [SerializeField] private PrefabData prefabData;
    public Vector3Data prefabOffset;
    
    public GameObject prefab => prefabData.prefab;
    
    public void SetPrefabData(PrefabData data)
    {
        prefabData = data;
    }
    
    public void SetPrefabOffset(Vector3Data data)
    {
        prefabOffset = data;
    }
    
    [System.Serializable]
    public class InstanceData
    {
        public TransformData targetPosition;
        public Vector3Data instanceOffset;
        public bool excludePrefabOffset;
    }
    
    public List<InstanceData> instances = new();

    public void OnEnable()
    {
        if (!prefabData) Debug.LogError("Prefab Data is null. Please assign a value.", this);
    }
}

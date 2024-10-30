using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "InstancerData", menuName = "Data/ManagerData/InstancerData")]
public class InstancerData : ScriptableObject
{
    private GameObject _hierarchyParent;
    [SerializeField] private bool _noParent;
    [SerializeField] private PrefabData prefabData;
    public Vector3Data prefabOffset;
    
    public GameObject prefab => prefabData.prefab;
    
    public void SetPrefabData(PrefabData data) => prefabData = data;
    
    public void SetPrefabOffset(Vector3Data data) => prefabOffset = data;

    public bool noParent => _noParent;
    public Transform hierarchyParent => _hierarchyParent ? _hierarchyParent.transform : null;

    public void SetHierarchyParent(GameObject parent)
    {
        _noParent = false;
        _hierarchyParent = parent;
    }

    [System.Serializable]
    public class InstanceData
    {
        public TransformData targetPosition;
        public Vector3Data instanceOffset;
        public bool excludePrefabOffset;
    }
    
    [HideInInspector] public List<GameObject> instances = new();
    public List<InstanceData> instancesData = new();

    public void OnEnable()
    {
#if UNITY_EDITOR
        if (!prefabData) Debug.LogError("Prefab Data is null. Please assign a value.", this);
#endif
    }

    public void OnDisable() => instances.Clear();
}

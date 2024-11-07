//
//  Outline.cs
//  QuickOutline
//
//  Created by Chris Nolet on 3/30/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//
// -- Modified by: Riley Anderson -- 
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Outline : MonoBehaviour
{
    private static readonly List<Mesh> RegisteredMeshes = new List<Mesh>();

    public enum Mode
    {
        OutlineAll,
        OutlineVisible,
        OutlineHidden,
        OutlineAndSilhouette,
        SilhouetteOnly
    }

    public Mode OutlineMode
    {
        get => outlineMode;
        set
        {
            outlineMode = value;
            _needsUpdate = true;
        }
    }

    public Color OutlineColor
    {
        get => outlineColor;
        set
        {
            outlineColor = value;
            _needsUpdate = true;
        }
    }

    public float OutlineWidth
    {
        get => outlineWidth;
        set
        {
            outlineWidth = value;
            _needsUpdate = true;
        }
    }

    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }

    [SerializeField]
    private Mode outlineMode;

    [SerializeField]
    private Color outlineColor = Color.white;

    [SerializeField, Range(0f, 10f)]
    private float outlineWidth = 2f;

    [SerializeField] private bool outlineContinuously;
    private bool outlineTimer;
    private float _seconds =  1f;
    private WaitForSecondsRealtime _wfsrtObj;

    [Header("Optional")]
    [SerializeField, Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
    + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
    private bool precomputeOutline;

    [SerializeField, HideInInspector]
    private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector]
    private List<ListVector3> bakeValues = new List<ListVector3>();

    private Renderer[] _renderers;
    private Material _outlineMaskMaterial;
    private Material _outlineFillMaterial;

    private bool _needsUpdate;

    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int ZTestID = Shader.PropertyToID("_ZTest");
    private static readonly int OutlineWidthID = Shader.PropertyToID("_OutlineWidth");

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();

        _outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
        _outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));

        _outlineMaskMaterial.name = "OutlineMask (Instance)";
        _outlineFillMaterial.name = "OutlineFill (Instance)";

        LoadSmoothNormals();

        enabled = false;

    }
    
    private void Start()
    {
        _wfsrtObj = new WaitForSecondsRealtime(_seconds);
    }

    private void OnEnable()
    {
        if (!enabled) return;
        
        foreach (var rend in _renderers)
        {
            var materials = rend.sharedMaterials.ToList();
            materials.Add(_outlineMaskMaterial);
            materials.Add(_outlineFillMaterial);
            rend.materials = materials.ToArray();
        }
    }

    private void OnValidate()
    {
        _needsUpdate = true;

        if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
        {
            bakeKeys.Clear();
            bakeValues.Clear();
        }

        if (precomputeOutline && bakeKeys.Count == 0)
        {
            Bake();
        }
    }

    private void Update()
    {
        if (!_needsUpdate) return;
        _needsUpdate = false;
        UpdateMaterialProperties();
    }

    private void OnDisable()
    {
        foreach (var rend in _renderers)
        {
            var materials = rend.sharedMaterials.ToList();
            materials.Remove(_outlineMaskMaterial);
            materials.Remove(_outlineFillMaterial);
            rend.materials = materials.ToArray();
        }
    }

    public void EnableOutline()
    {
        enabled = true;
        OutlineColor = outlineColor;
        OutlineWidth = outlineWidth;
        //_needsUpdate = true;
        UpdateMaterialProperties();
    }

    public void DisableOutline()
    {
        enabled = false;
        //_needsUpdate = true;
        UpdateMaterialProperties();
    }
    
    public void StartOutlineFlash()
    {
        outlineTimer = true;
        StartCoroutine(OutlineTimer());
    }
    
    private IEnumerator OutlineTimer()
    {
        while (outlineTimer)
        {
            EnableOutline();
            yield return _wfsrtObj;
            DisableOutline();
            yield return _wfsrtObj;
        }
    }
    
    public void StopOutlineFlash()
    {
        outlineTimer = false;
        StopCoroutine(OutlineTimer());
    }

    private void OnDestroy()
    {
        Destroy(_outlineMaskMaterial);
        Destroy(_outlineFillMaterial);
    }

    private void Bake()
    {
        var bakedMeshes = new HashSet<Mesh>();

        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (!bakedMeshes.Add(meshFilter.sharedMesh)) continue;

            var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

            bakeKeys.Add(meshFilter.sharedMesh);
            bakeValues.Add(new ListVector3 { data = smoothNormals });
        }
    }

    private void LoadSmoothNormals()
    {
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            if (RegisteredMeshes.Contains(meshFilter.sharedMesh)) continue;

            RegisteredMeshes.Add(meshFilter.sharedMesh);

            var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
            var smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);

            meshFilter.sharedMesh.SetUVs(3, smoothNormals);

            if (meshFilter.TryGetComponent(out Renderer rend))
            {
                CombineSubmeshes(meshFilter.sharedMesh, rend.sharedMaterials);
            }
        }

        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (RegisteredMeshes.Contains(skinnedMeshRenderer.sharedMesh)) continue;

            RegisteredMeshes.Add(skinnedMeshRenderer.sharedMesh);

            skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];

            CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
        }
    }

    private List<Vector3> SmoothNormals(Mesh mesh)
    {
        var groups = new Dictionary<Vector3, List<int>>();

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            if (!groups.ContainsKey(mesh.vertices[i]))
            {
                groups[mesh.vertices[i]] = new List<int>();
            }
            groups[mesh.vertices[i]].Add(i);
        }

        var smoothNormals = new List<Vector3>(mesh.normals);

        foreach (var group in groups.Values)
        {
            if (group.Count == 1) continue;

            var smoothNormal = Vector3.zero;

            foreach (var index in group)
            {
                smoothNormal += smoothNormals[index];
            }

            smoothNormal.Normalize();

            foreach (var index in group)
            {
                smoothNormals[index] = smoothNormal;
            }
        }

        return smoothNormals;
    }

    private void CombineSubmeshes(Mesh mesh, Material[] materials)
    {
        if (mesh.subMeshCount == 1 || mesh.subMeshCount > materials.Length) return;

        mesh.subMeshCount++;
        mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
    }

    private void UpdateMaterialProperties()
    {
        _outlineFillMaterial.SetColor(OutlineColorID, outlineColor);

        switch (outlineMode)
        {
            case Mode.OutlineAll:
                _outlineMaskMaterial.SetFloat(ZTestID, (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat(ZTestID, (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat(OutlineWidthID, outlineWidth);
                break;

            case Mode.OutlineVisible:
                _outlineMaskMaterial.SetFloat(ZTestID, (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat(ZTestID, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                _outlineFillMaterial.SetFloat(OutlineWidthID, outlineWidth);
                break;

            case Mode.OutlineHidden:
                _outlineMaskMaterial.SetFloat(ZTestID, (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat(ZTestID, (float)UnityEngine.Rendering.CompareFunction.Greater);
                _outlineFillMaterial.SetFloat(OutlineWidthID, outlineWidth);
                break;

          case Mode.OutlineAndSilhouette:
            _outlineMaskMaterial.SetFloat(ZTestID, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
            _outlineFillMaterial.SetFloat(ZTestID, (float)UnityEngine.Rendering.CompareFunction.Always);
            _outlineFillMaterial.SetFloat(OutlineWidthID, outlineWidth);
            break;

          case Mode.SilhouetteOnly:
            _outlineMaskMaterial.SetFloat(ZTestID, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
            _outlineFillMaterial.SetFloat(ZTestID, (float)UnityEngine.Rendering.CompareFunction.Greater);
            _outlineFillMaterial.SetFloat(OutlineWidthID, 0f);
            break;
        }
      }
}

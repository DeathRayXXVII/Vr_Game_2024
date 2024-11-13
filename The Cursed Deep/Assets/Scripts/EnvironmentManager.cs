using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnvironmentSettings
{
    public string environmentName;
    [Header("Fog Settings")]
    public bool fogEnabled;
    public Color fogColor = Color.gray;
    public FogMode fogMode = FogMode.Linear;
    public float fogStart = 0.01f;
    public float fogEnd = 300f;

    [Header("Skybox Settings")]
    public Material skyboxMaterial;
    [Range(0, 8)] public float skyboxExposure = 1f;
}

public class EnvironmentManager : MonoBehaviour
{
    public bool randomEnvironment;
    public List<EnvironmentSettings> environmentSettings;
    private static readonly int Exposure = Shader.PropertyToID("_Exposure");

    private void Start()
    {
        if (randomEnvironment && environmentSettings.Count > 0)
        {
            int randomIndex = Random.Range(0, environmentSettings.Count);
            ApplyEnvironmentSettings(environmentSettings[randomIndex]);
        }
        else if (environmentSettings.Count > 0)
        {
            ApplyEnvironmentSettings(environmentSettings[0]);
        }
    }

    private void OnValidate()
    {
        if (environmentSettings is { Count: > 0 })
        {
            ApplyEnvironmentSettings(environmentSettings[0]);
        }
    }

    public void SetEnvironment(string id)
    {
        foreach (var settings in environmentSettings.Where(settings => settings.environmentName == id))
        {
            ApplyEnvironmentSettings(settings);
            return;
        }
    }
    private void ApplyEnvironmentSettings(EnvironmentSettings settings)
    {
        ApplyFogSettings(settings);
        ApplySkyboxSettings(settings);
    }

    private void ApplyFogSettings(EnvironmentSettings settings)
    {
        RenderSettings.fog = settings.fogEnabled;
        RenderSettings.fogMode = settings.fogMode;
        RenderSettings.fogColor = settings.fogColor;
        RenderSettings.fogStartDistance = settings.fogStart;
        RenderSettings.fogEndDistance = settings.fogEnd;
    }

    private void ApplySkyboxSettings(EnvironmentSettings settings)
    {
        if (settings.skyboxMaterial != null)
        {
            RenderSettings.skybox = settings.skyboxMaterial;
            settings.skyboxMaterial.SetFloat(Exposure, settings.skyboxExposure);
        }
    }
}
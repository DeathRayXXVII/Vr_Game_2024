using UnityEngine;
using System.Collections;

public class LightingManager : MonoBehaviour
{
    [SerializeField]
    private Light directionalLight;
    [SerializeField]
    private LightingData lightData;
    [SerializeField, Range(0, 24)]
    private float timeOfDay;
    [SerializeField]
    private float timeScale = 0.5f;
    [SerializeField]
    private Material skyboxMaterial;
    [SerializeField, Range(0, 1)]
    private float nightExposure = 0.2f;
    [SerializeField, Range(1, 2)]
    private float dayExposure = 1f;
    
    public void StartDayCycle()
    {
        if (lightData != null)
        {
            StartCoroutine(UpdateLightingRoutine());
        }
    }

    private IEnumerator UpdateLightingRoutine()
    {
        while (true)
        {
            timeOfDay += Time.deltaTime * timeScale;
            timeOfDay %= 24;
            float timePercent = timeOfDay / 24f;
            UpdateLighting(timePercent);
            UpdateExposure(timePercent);
            yield return null;
        }
    }

    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = lightData.ambientColor.Evaluate(timePercent);
        RenderSettings.fogColor = lightData.fogColor.Evaluate(timePercent);
        if (directionalLight != null)
        {
            directionalLight.color = lightData.directionalColor.Evaluate(timePercent);
            directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }
    }

    private void UpdateExposure(float timePercent)
    {
        float currentExposure = Mathf.Lerp(nightExposure, dayExposure, Mathf.Sin(timePercent * Mathf.PI));
        skyboxMaterial.SetFloat("_Exposure", currentExposure);
    }

    private void OnValidate()
    {
        if (directionalLight != null)
        {
            return;
        }
        if (RenderSettings.sun != null)
        {
            directionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    return;
                }
            }
        }
    }
}

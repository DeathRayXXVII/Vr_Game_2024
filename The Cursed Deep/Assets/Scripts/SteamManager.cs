using System;
using UnityEngine;
using Steamworks;


public class SteamManager : MonoBehaviour
{
    [Header("Steamworks Settings")]
    [SerializeField] private uint steamAppID;
    [SerializeField] private bool isSteamworksEnabled;
    [SerializeField] private BoolData isSteamworksEnabledData;
    private static SteamManager s_instance;
    private void Awake()
    {
        if (!isSteamworksEnabled)
        {
            isSteamworksEnabledData.value = false;
            return;
        }
        isSteamworksEnabledData.value = true;
        if (s_instance != null) {
            Destroy(gameObject);
            return;
        }
        s_instance = this;

        DontDestroyOnLoad(gameObject);
        
        try
        {
            SteamClient.Init(steamAppID);
            Debug.Log("Steamworks initialized");
        }
        catch (Exception e)
        {
            Debug.LogError($"SteamApi_Init failed: {e.Message}");
            Console.WriteLine(e);
            throw;
        }
    }

    private void Update()
    {
        if (!isSteamworksEnabled) return;
        SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        if (!isSteamworksEnabled) return;
        SteamClient.Shutdown();
    }
    
    private void OnEnable() {
        if (s_instance == null) {
            s_instance = this;
        }
    }

    private void OnDestroy() {
        if (s_instance == this)
            s_instance = null;
    }
}
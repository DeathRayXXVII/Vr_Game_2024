using System;
using UnityEngine;
using Steamworks;


public class SteamManager : MonoBehaviour
{
    [Header("Steamworks Settings")]
    [SerializeField] private uint steamAppID;
    [SerializeField] private bool isSteamworksEnabled;
    private void Awake()
    {
        if (!isSteamworksEnabled) return;
        try
        {
            SteamClient.Init(steamAppID);
            Debug.Log("Steamworks initialized");
        }
        catch (Exception e)
        {
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
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ZPTools.Utility.UtilityFunctions;

public class CannonInstancerHelper : MonoBehaviour
{
    [SerializeField] private InstancerData _cannonInstancerData;
    [SerializeField] private List<SocketMatchInteractor> _ammoSockets = new();
    
    public void TieAmmoSocketsToCannons()
    {
        if (!_cannonInstancerData) return;
        
        var socketIndex = 0;
        foreach (var cannon in _cannonInstancerData.instances.Where(cannon => cannon))
        {
            var manager = AdvancedGetComponent<CannonManager>(cannon);
            if (!manager) 
                continue;
            
            var socket = _ammoSockets.Count > socketIndex ? _ammoSockets[socketIndex] : null;
            if (!socket) 
                continue;
            
            socketIndex++;
            manager.ammoSpawnSocket = socket;
        }
    }
}

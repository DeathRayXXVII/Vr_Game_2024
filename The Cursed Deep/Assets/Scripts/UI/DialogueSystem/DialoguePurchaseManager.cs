using System.Collections.Generic;
using UnityEngine;

public class PurchaseHandlerManager : MonoBehaviour
{
    public Dictionary<string, DialoguePurcheseHandler> purchaseHandlers = new Dictionary<string, DialoguePurcheseHandler>();

    private void Awake()
    {
        // Initialize and register all purchase handlers in the scene
        foreach (var handler in FindObjectsOfType<DialoguePurcheseHandler>())
        {
            purchaseHandlers[handler.Id] = handler;
        }
    }

    public DialoguePurcheseHandler GetHandler(string id)
    {
        if (purchaseHandlers.TryGetValue(id, out var handler))
        {
            return handler;
        }
        Debug.LogError($"PurchaseHandler with ID {id} not found.");
        return null;
    }
}

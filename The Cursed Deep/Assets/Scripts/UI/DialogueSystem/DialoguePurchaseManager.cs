using System.Collections.Generic;
using UnityEngine;

public class PurchaseHandlerManager : MonoBehaviour
{
    public Dictionary<string, DialoguePurchaseHandler> purchaseHandlers = new Dictionary<string, DialoguePurchaseHandler>();

    private void Awake()
    {
        // Initialize and register all purchase handlers in the scene
        foreach (var handler in FindObjectsOfType<DialoguePurchaseHandler>())
        {
            purchaseHandlers[handler.id] = handler;
        }
    }

    public DialoguePurchaseHandler GetHandler(string id)
    {
        if (purchaseHandlers.TryGetValue(id, out var handler))
        {
            return handler;
        }
        Debug.LogError($"PurchaseHandler with ID {id} not found.");
        return null;
    }
}

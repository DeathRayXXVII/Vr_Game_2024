using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class InteractionLayerMaskHandler : MonoBehaviour
{
    [SerializeField] private XRBaseInteractor layerMaskProvider;
    
    private int _originalLayerMask = -2;
    private const int NothingLayerMask = 0;
    
    private void Awake()
    {
        SetOriginalLayerMask();
    }
    
    private void SetOriginalLayerMask()
    {
        if (_originalLayerMask != -2)
        {
            return;
        }
        
        if (layerMaskProvider == null)
        {
            Debug.LogWarning("LayerMaskProvider is null, Disabling InteractionLayerMaskHandler");
            enabled = false;
            return;
        }
        _originalLayerMask = layerMaskProvider.interactionLayers.value;
    }
    
    public void DisableInteractions()
    {
        SetOriginalLayerMask();
        layerMaskProvider.interactionLayers = NothingLayerMask;
    }
    
    public void EnableInteractions()
    {
        layerMaskProvider.interactionLayers = _originalLayerMask;
    }
}

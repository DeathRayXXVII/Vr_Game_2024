using UnityEngine;

public class OutlineSelection : MonoBehaviour
{
    public void EnableOutline(MeshRenderer obj)
    {
        var outline = obj.GetComponent<Outline>();
        if (outline == null) return;
        
        outline.enabled = true;
        outline.OutlineColor = Color.yellow;
        outline.OutlineWidth = 5;
    }

    public void DisableOutline(MeshRenderer obj)
    {
        var outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}
using UnityEngine;

public class OutlineSelection : MonoBehaviour
{
    public void EnableOutline(Transform obj)
    {
        var outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.gameObject.AddComponent<Outline>();
            outline.OutlineColor = Color.yellow;
            outline.OutlineWidth = 5;
        }
        outline.enabled = true;
    }

    public void DisableOutline(Transform obj)
    {
        var outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}
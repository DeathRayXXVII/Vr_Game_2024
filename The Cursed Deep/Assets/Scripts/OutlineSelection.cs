using UnityEngine;

public class OutlineSelection : MonoBehaviour
{
    private Transform selectedObj;
    private Transform highlightedObj;
    private RaycastHit hit;
    [SerializeField] private LayerMask layerMask;

    /*public void Update()
    {
        if (highlightedObj != null)
        {
            DisableOutline(highlightedObj);
            highlightedObj = null;
        }

        if (Camera.main == null) return;
        if (layerMask == 20)
        {
            highlightedObj = hit.transform;
            if (highlightedObj != selectedObj)
            {
                EnableOutline(highlightedObj);
            }
            else
            {
                highlightedObj = null;
            }
        }
    }*/

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
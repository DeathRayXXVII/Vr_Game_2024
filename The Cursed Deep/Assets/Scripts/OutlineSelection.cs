using UnityEngine;

public class OutlineSelection : MonoBehaviour
{
    private Transform selectedObj;
    private Transform highlightedObj;
    private RaycastHit hit;
    private LayerMask layerMask;

    private void Start()
    {
        layerMask = LayerMask.GetMask("PlayerInteractables");
    }

    public void Update()
    {
        if (highlightedObj != null)
        {
            var outline = highlightedObj.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false;
            }
            highlightedObj = null;
        }

        if (Camera.main == null) return;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            highlightedObj = hit.transform;
            if (highlightedObj != selectedObj)
            {
                var outline = highlightedObj.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = highlightedObj.gameObject.AddComponent<Outline>();
                    outline.OutlineColor = Color.yellow;
                    outline.OutlineWidth = 5;
                }
                outline.enabled = true;
            }
            else
            {
                highlightedObj = null;
            }
        }
    }
}
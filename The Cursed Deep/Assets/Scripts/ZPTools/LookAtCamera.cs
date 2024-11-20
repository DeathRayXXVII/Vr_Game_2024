using System.Collections;
using UnityEngine;
namespace ZPTools
{
    public class LookAtCamera : LookAtObject
    {
        [Tooltip("Assign a specific camera if needed; otherwise, defaults to main camera.")]
        [SerializeField] private Camera targetCamera;

        protected override Transform GetTargetTransform()
        {
            // If no specific camera is assigned, default to the main camera.
            return targetCamera != null ? targetCamera.transform : Camera.main?.transform;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZPTools
{
    public class LookAtCamera : LookAtObject
    {
        [Tooltip("Assign a specific camera if needed; Otherwise, defaults to main camera.")]
        [SerializeField] private Camera targetCamera;

        protected override void OnEnable()
        {
            if (targetCamera != null)
            {
                targetObject = targetCamera.gameObject;
                base.OnEnable();
                return;
            }

            targetCamera = Camera.main;

#if UNITY_XR_AVAILABLE
            if (targetCamera == null)
            {
                StartCoroutine(WaitForXRSubsystem(() => base.OnEnable()));
            }
            else
#endif
            {
                targetObject = targetCamera!.gameObject;
                base.OnEnable();
            }
        }

#if UNITY_XR_AVAILABLE
        private IEnumerator WaitForXRSubsystem(System.Action onComplete = null)
        {
            var xrSubsystems = new List<UnityEngine.XR.XRInputSubsystem>();
            SubsystemManager.GetInstances(xrSubsystems);

            while (!xrSubsystems.Any(sub => sub.running))
            {
                yield return null;
            }

            targetCamera = Camera.main;
            targetObject = targetCamera!.gameObject;
            onComplete?.Invoke();
        }
#endif
    }
}
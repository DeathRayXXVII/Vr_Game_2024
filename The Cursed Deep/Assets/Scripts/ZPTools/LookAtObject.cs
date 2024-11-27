using System.Collections;
using UnityEngine;

namespace ZPTools
{
    public class LookAtObject : MonoBehaviour
    {
        [System.Flags]
        public enum FreezeAxis
        {
            None = 0,
            X = 1,
            Y = 2,
            Z = 4
        }

        [SerializeField] protected bool _performOnEnable = true;
        [SerializeField] protected GameObject targetObject;
        [SerializeField, BitMask] private FreezeAxis _freezeAxes = FreezeAxis.None;
        [SerializeField] protected Quaternion _offsetRotation = Quaternion.identity;

        [SerializeField, SteppedRange(0, 10, 0.1f)]
        protected float _updateInterval = 0.1f;

        private WaitForSeconds _waitSeconds;
        private readonly WaitForFixedUpdate _waitFixed = new();
        private YieldInstruction wait => _updateInterval > Time.fixedDeltaTime ? _waitSeconds : _waitFixed;

        private Coroutine _lookCoroutine;

        protected virtual void OnEnable()
        {
            _waitSeconds = new WaitForSeconds(_updateInterval);

            if (_performOnEnable && GetTargetTransform() != null)
            {
                StartLookAtObject();
            }
        }

        protected virtual void OnDisable() => StopLookAtObject();

        public void StartLookAtObject() => _lookCoroutine ??= StartCoroutine(UpdateRotation());

        public void StopLookAtObject()
        {
            if (_lookCoroutine == null) return;
            StopCoroutine(_lookCoroutine);
            _lookCoroutine = null;
        }

        private IEnumerator UpdateRotation()
        {
            if (!GetTargetTransform()) yield break;

            var targetIsValid = CalculateLookRotation();
            var previousTargetPosition = GetTargetTransform().position;
            while (true)
            {
                if (targetIsValid && previousTargetPosition != GetTargetTransform().position)
                {
                    targetIsValid = CalculateLookRotation();
                    previousTargetPosition = GetTargetTransform().position;
                }

                yield return wait;
            }
        }

        protected virtual Transform GetTargetTransform()
        {
            return targetObject != null ? targetObject.transform : null;
        }

        private bool CalculateLookRotation()
        {
            var targetTransform = GetTargetTransform();
            if (targetTransform == null) return false;

            Quaternion currentRotation = transform.localRotation;

            transform.LookAt(targetTransform);
            Quaternion targetRotation = transform.localRotation * _offsetRotation;

            Vector3 newEulerAngles = targetRotation.eulerAngles;

            Vector3 currentEulerAngles = currentRotation.eulerAngles;

            if (_freezeAxes != FreezeAxis.None)
            {
                if ((_freezeAxes & FreezeAxis.X) == FreezeAxis.X)
                {
                    newEulerAngles.x = currentEulerAngles.x;
                }

                if ((_freezeAxes & FreezeAxis.Y) == FreezeAxis.Y)
                {
                    newEulerAngles.y = currentEulerAngles.y;
                }

                if ((_freezeAxes & FreezeAxis.Z) == FreezeAxis.Z)
                {
                    newEulerAngles.z = currentEulerAngles.z;
                }
            }
            transform.localRotation = Quaternion.Euler(newEulerAngles);

            return true;
        }
    }
}
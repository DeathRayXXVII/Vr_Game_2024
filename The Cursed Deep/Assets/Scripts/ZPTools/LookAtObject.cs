using System.Collections;
using UnityEngine;

namespace ZPTools
{
    public class LookAtObject : MonoBehaviour
    {
        [SerializeField] protected bool _performOnStart;
        [SerializeField] protected GameObject targetObject;
        [SerializeField] protected Quaternion _offsetRotation = Quaternion.identity;
        [SerializeField, SteppedRange(0, 10, 0.1f)] protected float _updateInterval = 0.1f;

        private WaitForSeconds _waitSeconds;
        private readonly WaitForFixedUpdate _waitFixed = new();
        private YieldInstruction wait => _updateInterval > Time.fixedDeltaTime ? _waitSeconds : _waitFixed;
        
        private Coroutine _lookCoroutine;

        protected virtual void OnEnable()
        {
            _waitSeconds = new WaitForSeconds(_updateInterval);
            
            if (_performOnStart && GetTargetTransform() != null)
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
            transform.LookAt(targetTransform);
            transform.rotation *= _offsetRotation;
            return true;
        }
    }
}
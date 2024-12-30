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
        
        [SerializeField, SteppedRange(0, 10, 0.1f)] protected float _updateInterval = 0.1f;

#if UNITY_EDITOR
        public bool visualizeRange;
#endif
        [SerializeField] public bool _interpolateScaleBasedOnDistance;
        [SerializeField, SteppedRange(0.1f, 100, 0.01f)] protected float _activeRange = 5f;
        [SerializeField, SteppedRange(0.1f, 100, 0.01f)] protected float _fullScaleRange = 5f;
        private const float MIN_RANGE = 0.1f;
        
        private float _distanceToTarget;
        private Vector3 _initialScale;
        
        private WaitForSeconds _waitSeconds;
        private readonly WaitForFixedUpdate _waitFixed = new();
        private YieldInstruction wait => _updateInterval > Time.fixedDeltaTime ? _waitSeconds : _waitFixed;

        private Coroutine _lookCoroutine;

        protected virtual void OnValidate()
        {
            _activeRange = Mathf.Max(MIN_RANGE, _activeRange);
            _fullScaleRange = Mathf.Clamp(_fullScaleRange, MIN_RANGE, _activeRange);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        protected virtual void Awake()
        {
            _initialScale = transform.localScale;
        }

        protected virtual void OnEnable()
        {
            _waitSeconds = new WaitForSeconds(_updateInterval);
            
            _activeRange = Mathf.Max(MIN_RANGE, _activeRange);
            _fullScaleRange = Mathf.Clamp(_fullScaleRange, MIN_RANGE, _activeRange);

            if (_performOnEnable && GetTargetTransform() != null)
            {
                StartLookAtObject();
            }
        }

        protected virtual void OnDisable() => StopLookAtObject();

        public void StartLookAtObject() => _lookCoroutine ??= StartCoroutine(UpdateTransform());

        public void StopLookAtObject()
        {
            if (_lookCoroutine == null) return;
            StopCoroutine(_lookCoroutine);
            _lookCoroutine = null;
        }

        private IEnumerator UpdateTransform()
        {
            var targetTransform = GetTargetTransform();
            if (!targetTransform) yield break;
            
            var initialScale = transform.localScale;
            var targetPosition = targetTransform.position;
            transform.rotation = CalculateLookRotation(targetTransform);
            while (IsValidTarget())
            {
                if (!TargetWithinRange(targetTransform))
                {
                    _lookCoroutine = StartCoroutine(WaitForTargetToComeInRange());
                    yield break;
                }
                
                var currentPosition = targetTransform.position;
                
                if (targetPosition != currentPosition)
                {
                    transform.rotation = CalculateLookRotation(targetTransform);
                    targetPosition = currentPosition;
                }
                
                if (_interpolateScaleBasedOnDistance)
                {
                    transform.localScale = CalculateScale();
                }

                yield return wait;
            }
        }

        private IEnumerator WaitForTargetToComeInRange()
        {
            var targetTransform = GetTargetTransform();
                
            if (_interpolateScaleBasedOnDistance)
            {
                transform.localScale = Vector3.zero;
            }
            
            while (IsValidTarget() && !TargetWithinRange(targetTransform))
            {
                yield return wait;
            }

            _lookCoroutine = StartCoroutine(UpdateTransform());
        }
        private bool IsValidTarget()
        {
            if (targetObject != null) return true;
            
            Debug.LogError("Target object is null!", this);
            return false;
        }


        protected virtual Transform GetTargetTransform()
        {
            return IsValidTarget() ? targetObject.transform : null;
        }

        private static float CalculateDistance(Vector3 start, Vector3 end) => Vector3.Distance(start, end);

        private bool TargetWithinRange(Transform target)
        {
            _distanceToTarget = CalculateDistance(transform.position, target.position);
            return _distanceToTarget <= _activeRange;
        }

        private Vector3 CalculateScale()
        {
            // Normalize the distance to the target within the active range and full scale range to get a value between 0 and 1
            var normalizedDistToFullScale = Mathf.InverseLerp(_activeRange, _fullScaleRange, _distanceToTarget);

            return Vector3.Lerp(Vector3.zero, _initialScale, normalizedDistToFullScale);
        }

        private Quaternion CalculateLookRotation(Transform target)
        {
            Vector3 forwardDirection = (target.position - transform.position).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(forwardDirection, Vector3.up);
            Quaternion offsetRotation = Quaternion.Euler(_offsetRotation.eulerAngles);
            
            targetRotation *= offsetRotation;

            Vector3 newEulerAngles = targetRotation.eulerAngles;
            
            if (_freezeAxes == FreezeAxis.None) return Quaternion.Euler(newEulerAngles);
            
            Vector3 currentEulerAngles = transform.localRotation.eulerAngles;
            
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

            return Quaternion.Euler(newEulerAngles);
        }
        
#if UNITY_EDITOR
        protected void OnDrawGizmos()
        // protected void OnDrawGizmosSelected()
        {
            if (!visualizeRange) return;

            var targetTransform = GetTargetTransform();
            if (targetTransform != null)
            {
                Vector3 direction = (targetTransform.position - transform.position).normalized;
                Vector3 cardinalDirection = GetClosestDirection(direction);

                if (cardinalDirection == Vector3.forward)
                    Gizmos.color = Color.blue;
                else if (cardinalDirection == Vector3.back)
                    Gizmos.color = Color.cyan;
                else if (cardinalDirection == Vector3.right)
                    Gizmos.color = Color.red;
                else if (cardinalDirection == Vector3.left)
                    Gizmos.color = Color.magenta;
                else if (cardinalDirection == Vector3.up)
                    Gizmos.color = Color.green;
                else if (cardinalDirection == Vector3.down)
                    Gizmos.color = Color.yellow;
                else
                    Gizmos.color = Color.white;

                var rayDirection = (targetTransform.position - transform.position).normalized;
                var rayEnd = transform.position + rayDirection * _activeRange / 5;

                Gizmos.DrawLine(transform.position, rayEnd);

                float arrowHeadLength = 0.1f;
                float arrowHeadAngle = 15.0f;

                Vector3 right = Quaternion.LookRotation(rayDirection) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
                Vector3 left = Quaternion.LookRotation(rayDirection) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;

                Vector3 arrowHeadPoint = rayEnd + rayDirection * arrowHeadLength;
                Vector3 arrowRightPoint = arrowHeadPoint + right * arrowHeadLength;
                Vector3 arrowLeftPoint = arrowHeadPoint + left * arrowHeadLength;

                Gizmos.DrawLine(arrowHeadPoint, arrowRightPoint);
                Gizmos.DrawLine(arrowHeadPoint, arrowLeftPoint);
                Gizmos.DrawLine(arrowRightPoint, arrowLeftPoint);
                
                Gizmos.color = Color.white;
                Gizmos.DrawLine(arrowHeadPoint, targetTransform.position);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _activeRange);

            if (!_interpolateScaleBasedOnDistance) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _fullScaleRange);
        }

        private static Vector3 GetClosestDirection(Vector3 direction)
        {
            // List of cardinal directions
            Vector3[] directions = {
                Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, Vector3.down
            };

            // Find the closest direction
            Vector3 closestDirection = directions[0];
            float maxDot = Vector3.Dot(direction, closestDirection);

            foreach (var dir in directions)
            {
                float dot = Vector3.Dot(direction, dir);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    closestDirection = dir;
                }
            }

            return closestDirection;
        }
#endif
    }
}
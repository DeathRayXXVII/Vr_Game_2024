using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace ShipGame.Manager
{
    public class LevelSelectUIManager : MonoBehaviour
    {
        [SerializeField] private IntData _currentLevel;
        
        [SerializeField] private Transform _uiParent;
        
        [SerializeField] private TextMeshProBehavior _headerTextMesh;
        [SerializeField] private TextMeshProBehavior _levelNumberTextMesh;
        
        private string _headerText;
        private string levelNumberText => $"Level {_currentLevel ?? 0}";
        
        [SerializeField] private XRSimpleInteractable confirmButton;
        [SerializeField] private XRSimpleInteractable cancelButton;
        
        public UnityEvent confirmedSelection;
        public UnityEvent cancelledSelection;
        
        private readonly WaitForFixedUpdate _waitFixed = new();
        private Coroutine _uiAnimationCoroutine;
        
        private Vector3 _initialScale;
        private float _animationDuration;

        private void Awake()
        {
            confirmButton.colliders[0].enabled = false;
            cancelButton.colliders[0].enabled = false;
            
            _initialScale = _uiParent.localScale;
            _uiParent.localScale = Vector3.zero;
            _uiParent.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            confirmButton.selectEntered.AddListener(SelectionConfirmed);
            cancelButton.selectExited.AddListener(SelectionCancelled);
        }

        private void OnDisable()
        {
            confirmButton.selectEntered.RemoveListener(SelectionConfirmed);
            cancelButton.selectExited.RemoveListener(SelectionCancelled);
        }
        
        private void SelectionConfirmed(SelectEnterEventArgs args)
        {
            confirmedSelection?.Invoke();
        }
        
        private void SelectionCancelled(SelectExitEventArgs args)
        {
            cancelledSelection?.Invoke();
        }
        
        public void ActivateUI(string headerText, Vector3 startPosition, Vector3 targetPosition, float duration = 1f)
        {
            _headerText = headerText;

            if (_uiAnimationCoroutine != null)
            {
                return;
            }
            
            _uiAnimationCoroutine ??= StartCoroutine(
                PerformUIAnimation(true, startPosition, targetPosition, duration));
        }
        
        public IEnumerator DeactivateUIAndWait(Vector3 startPosition, Vector3 targetPosition, float duration = 1f)
        {
            Debug.Log("Deactivating UI and waiting for animation to complete.", this);

            if (_uiAnimationCoroutine != null)
            {
                StopCoroutine(_uiAnimationCoroutine);
                _uiAnimationCoroutine = null;
            }
            
            _animationDuration = duration;
            DeactivateUI(startPosition, targetPosition, duration);
            
            var time = 0f;
            while (_uiAnimationCoroutine == null || time < _animationDuration)
            {
                Debug.Log($"Waiting for UI Animation to start: {time} / {_animationDuration}", this);
                time += Time.deltaTime;
                yield return null;
            }
            
            time = 0f;
            while (_uiAnimationCoroutine != null || time < _animationDuration)
            {
                Debug.Log($"Waiting for UI Deactivation: {time} / {_animationDuration}", this);
                time += Time.deltaTime;
                yield return null;
            }
            
            yield return _waitFixed;

            if (_uiAnimationCoroutine == null || !(time > _animationDuration)) yield break;
            
            StopCoroutine(_uiAnimationCoroutine);
            _uiAnimationCoroutine = null;
        }
        
        public void DeactivateUI(Vector3 startPosition, Vector3 targetPosition, float duration = 1f)
        {
            if (_uiAnimationCoroutine != null)
            {
                return;
            }
            
            confirmButton.colliders[0].enabled = false;
            cancelButton.colliders[0].enabled = false;
            
            _uiAnimationCoroutine ??= StartCoroutine(
                PerformUIAnimation(false, startPosition, targetPosition, duration));
        }
        
        private IEnumerator PerformUIAnimation(bool activeState, Vector3 startPosition, Vector3 targetPosition,
            float duration = 1f)
        {
            _animationDuration = duration;
            
            if (activeState)
            {
                _uiParent.gameObject.SetActive(true);
            
                _headerTextMesh.UpdateLabel(_headerText);
                _levelNumberTextMesh.UpdateLabel(levelNumberText);
                
                confirmButton.colliders[0].enabled = true;
                cancelButton.colliders[0].enabled = true;
            }
            _uiParent.localScale = activeState ? Vector3.zero : _initialScale;
            yield return _waitFixed;
            
            var time = 0f;
            
            while (time < duration)
            {
                // Debug.Log($"Time: {time}, Duration: {duration}, Time / Duration: {time / duration}, " +
                //           $"Start Position: {startPosition}, Target Position: {targetPosition}, " +
                //           $"UI Parent Position: {_uiParent.position}");
                time += Time.deltaTime;
                _uiParent.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
                _uiParent.localScale = activeState ?
                    Vector3.Lerp(Vector3.zero, _initialScale, time / duration) :
                    Vector3.Lerp(_initialScale, Vector3.zero, time / duration);
                yield return null;
            }
            yield return _waitFixed;
            
            if (!activeState)
            {
                _uiParent.gameObject.SetActive(false);
                yield return _waitFixed;
            }
            
            _uiAnimationCoroutine = null;
        }
    }
}

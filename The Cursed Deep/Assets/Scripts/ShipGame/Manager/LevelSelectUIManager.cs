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
        [SerializeField] private TextMeshProBehavior _detailsTextMesh;
        
        private string _headerText;
        private string _detailsText;
        private string _confirmText;
        private string _cancelText;
        
        [SerializeField] private XRSimpleInteractable confirmButton;
        [SerializeField] private XRSimpleInteractable cancelButton;
        [SerializeField] private TextMeshProBehavior _confirmTextMesh;
        [SerializeField] private TextMeshProBehavior _cancelTextMesh;
        
        public UnityEvent confirmedSelection;
        public UnityEvent cancelledSelection;
        
        private readonly WaitForFixedUpdate _waitFixed = new();
        private Coroutine _uiAnimationCoroutine;
        
        private Vector3 _initialScale;

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
        
        public IEnumerator ActivateUI(string headerText, string detailText, string confirmText, string cancelText,
            Vector3 startPosition, Vector3 targetPosition, float duration = 1f)
        {
            if (_headerTextMesh == null || _detailsTextMesh == null || _confirmTextMesh == null || _cancelTextMesh == null)
            {
                Debug.LogError("Header, Details, Confirm or Cancel Text Mesh is null, cannot update text. Stopping UI Activation", this);
                yield break;
            }
            
            if (_uiAnimationCoroutine != null)
            {
                StopCoroutine(_uiAnimationCoroutine);
                yield return _waitFixed;
                _uiAnimationCoroutine = null;
            }
            
            _headerText = headerText;
            _detailsText = detailText;
            _confirmText = confirmText;
            _cancelText = cancelText;
            
            _uiAnimationCoroutine ??= StartCoroutine(
                PerformUIAnimation(true, startPosition, targetPosition, duration));
            
            yield return new WaitUntil(() => _uiAnimationCoroutine == null);
        }
        
        public IEnumerator DeactivateUI(Vector3 startPosition, Vector3 targetPosition, float duration = 1f)
        {
            if (_uiAnimationCoroutine != null)
            {
                StopCoroutine(_uiAnimationCoroutine);
                yield return _waitFixed;
                _uiAnimationCoroutine = null;
            }
            
            _uiAnimationCoroutine ??= StartCoroutine(
                PerformUIAnimation(false, startPosition, targetPosition, duration));
            
            yield return new WaitUntil(() => _uiAnimationCoroutine == null);
        }
        
        private IEnumerator PerformUIAnimation(bool activeState, Vector3 startPosition, Vector3 targetPosition,
            float duration = 1f)
        {
            confirmButton.colliders[0].enabled = false;
            cancelButton.colliders[0].enabled = false;
            yield return _waitFixed;
            
            if (activeState)
            {
                _uiParent.gameObject.SetActive(true);
                
                _headerTextMesh.UpdateLabel(_headerText);
                _detailsTextMesh.UpdateLabel(_detailsText);
                _confirmTextMesh.UpdateLabel(_confirmText);
                _cancelTextMesh.UpdateLabel(_cancelText);
            }
            _uiParent.localScale = activeState ? Vector3.zero : _initialScale;
            yield return _waitFixed;
            
            var time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                _uiParent.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
                _uiParent.localScale = activeState ?
                    Vector3.Lerp(Vector3.zero, _initialScale, time / duration) :
                    Vector3.Lerp(_initialScale, Vector3.zero, time / duration);
                yield return null;
            }
            yield return _waitFixed;

            if (activeState)
            {
                confirmButton.colliders[0].enabled = true;
                cancelButton.colliders[0].enabled = true;    
            }
            else
            {
                _uiParent.gameObject.SetActive(false);
                yield return _waitFixed;
            }
            
            _uiAnimationCoroutine = null;
        }
    }
}

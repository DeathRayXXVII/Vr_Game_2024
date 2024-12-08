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
        
        [SerializeField] private XRSimpleInteractable confirmButton;
        [SerializeField] private XRSimpleInteractable cancelButton;
        
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
        
        public void ActivateUI(string headerText, string detailText, Vector3 startPosition, Vector3 targetPosition, float duration = 1f)
        {
            _headerText = headerText;
            _detailsText = detailText;
            

            if (_uiAnimationCoroutine != null)
            {
                return;
            }
            
            if (_headerTextMesh == null || _detailsTextMesh == null)
            {
                Debug.LogError("Header or Details Text Mesh is null, cannot update text. Stopping UI Activation", this);
                return;
            }
            
            _uiAnimationCoroutine ??= StartCoroutine(
                PerformUIAnimation(true, startPosition, targetPosition, duration));
        }
        
        private bool _waitingForUI;
        public IEnumerator DeactivateUIAndWait(Vector3 startPosition, Vector3 targetPosition, float duration = 1f)
        {
            if (_waitingForUI)
            {
                Debug.LogError("Already waiting for UI to deactivate. Cannot wait again.", this);
                yield break;
            }
            _waitingForUI = true;
            if (_uiAnimationCoroutine != null)
            {
                StopCoroutine(_uiAnimationCoroutine);
                _uiAnimationCoroutine = null;
                yield return _waitFixed;
            }
            
            _uiAnimationCoroutine ??= StartCoroutine(
                PerformUIAnimation(false, startPosition, targetPosition, duration));
            
            yield return new WaitUntil(() => _uiAnimationCoroutine == null);
            
            yield return _waitFixed;
            _waitingForUI = false;
        }
        
        public void DeactivateUI(Vector3 startPosition, Vector3 targetPosition, float duration = 1f)
        {
            if (_uiAnimationCoroutine != null)
            {
                Debug.LogError($"[ERROR] UI Animation Coroutine is already running. Cannot deactivate UI.", this);
                return;
            }
            
            _uiAnimationCoroutine ??= StartCoroutine(
                PerformUIAnimation(false, startPosition, targetPosition, duration));
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

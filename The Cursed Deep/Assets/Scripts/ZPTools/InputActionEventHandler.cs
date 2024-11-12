using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ZPTools
{
    [System.Serializable]
    public struct InputActionEvent : System.IEquatable<InputActionEvent>
    {
        [SerializeField] private bool _listenOnStart;
        [SerializeField] private string _actionName;
        [SerializeField] private InputActionReference _inputAction;
        
        [SerializeField] private UnityEvent inputPerformedEvent;
        
        private bool _eventCaptured;
        
        public string actionName => _actionName;
        
        private void OnEnable()
        {
            if (!_listenOnStart) return;
            StartListening();
        }
        
        private void OnDisable() => StopListening();
        
        public void StartListening()
        {
            _eventCaptured = false;
            _inputAction.action.performed += OnInputPerformed;
        }
        
        public void StopListening()
        {
            _inputAction.action.performed -= OnInputPerformed;
        }
        
        private void OnInputPerformed(InputAction.CallbackContext context)
        {
            if (_eventCaptured) return;
            _eventCaptured = true;
            StopListening();
            inputPerformedEvent.Invoke();
        }

        public bool Equals(InputActionEvent other)
        {
            return _actionName == other._actionName && Equals(inputPerformedEvent, other.inputPerformedEvent);
        }

        public override bool Equals(object obj)
        {
            return obj is InputActionEvent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_actionName, inputPerformedEvent);
        }
    }
    public class InputActionEventHandler : MonoBehaviour
    {
        [SerializeField] private InputActionEvent[] _inputActionEvents;
        
        private InputActionEvent TryFindInputActionEvent(string actionName)
        {
            foreach (var inputActionEvent in _inputActionEvents)
            {
                if (inputActionEvent.actionName == actionName)
                {
                    return inputActionEvent;
                }
            }
            return default(InputActionEvent);
        }
        
        public void StartListening(string actionName)
        {
            var inputActionEvent = TryFindInputActionEvent(actionName);
            if (inputActionEvent.Equals(default(InputActionEvent))) return;
            inputActionEvent.StartListening();
        }
        
        private void OnDisable()
        {
            foreach (var inputActionEvent in _inputActionEvents)
            {
                inputActionEvent.StopListening();
            }
        }
    }
}

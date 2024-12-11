using System.Collections;
using UnityEngine;

namespace ZPTools
{
    public abstract class ScreenManager : MonoBehaviour
    {
        [SerializeField] protected bool allowDebug;
        
        [SerializeField, SteppedRange(0, 10, 0.1f)] protected float _transitionDuration = 1f;
        public float transitionDuration { get => _transitionDuration; set => _transitionDuration = value; }
        public bool isTransitioning => TransitionCoroutine != null;
        
        protected readonly WaitForFixedUpdate WaitFixed = new();
        protected Coroutine TransitionCoroutine;

        protected enum TransitionType
        {
            In,
            Out
        }
        
        private int _transitionType;
        protected int transitionType
        {
            get => _transitionType;
            set => _transitionType = System.Math.Clamp(value, 0, 1);
        }
        
        protected virtual void Start()
        {
            Initialize();
        }

        public virtual IEnumerator TransitionIn()
        {
            if (isTransitioning)
            {
                if (allowDebug) Debug.LogWarning("Transition already in progress.", this);
                yield break;
            }
            transitionType = (int)TransitionType.In;
            TransitionCoroutine ??= StartCoroutine(ExecuteTransition());
            yield return new WaitUntil(() => TransitionCoroutine == null);
        
            yield return WaitFixed; 
        }

        public virtual IEnumerator TransitionOut()
        {
            if (isTransitioning)
            {
                if (allowDebug) Debug.LogWarning("Transition already in progress.", this);
                yield break;
            }
            transitionType = (int)TransitionType.Out;
            TransitionCoroutine ??= StartCoroutine(ExecuteTransition());
            yield return new WaitUntil(() => TransitionCoroutine == null);
        
            yield return WaitFixed; 
        }
        
        protected virtual void OnDisable()
        {
            if (TransitionCoroutine == null) return;
            
            StopCoroutine(TransitionCoroutine);
            TransitionCoroutine = null;
        }
        
        protected abstract void Initialize();
        protected abstract IEnumerator ExecuteTransition();
    }
}
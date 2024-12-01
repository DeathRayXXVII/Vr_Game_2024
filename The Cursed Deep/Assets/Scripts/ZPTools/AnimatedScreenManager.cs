using System.Collections;
using UnityEngine;

namespace ZPTools
{
    public class AnimatedScreenManager : ScreenManager
    {
        [SerializeField] private Animator transitionAnimator;
        [SerializeField] private string transitionInTrigger = "FadeIn";
        [SerializeField] private string transitionOutTrigger = "FadeOut";
    
        private bool? animatorRunning => transitionAnimator == null ? null :
            transitionAnimator.IsInTransition(0) || transitionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
        
        private bool HasTrigger(string triggerName)
        {
            if (transitionAnimator == null)
            {
                return false;
            }

            foreach (var parameter in transitionAnimator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
                {
                    return true;
                }
            }
            return false;
        }
        
        public void SetAnimator(AnimatorOverrideController animator)
        {
            if (transitionAnimator == null)
            {
                return;
            }
            transitionAnimator.runtimeAnimatorController = animator;
        }
        
        protected override void Initialize()
        {
        }
        
        protected override IEnumerator ExecuteTransition()
        {
            if (transitionAnimator == null)
            {
                if (allowDebug) Debug.LogWarning("Transition Animator is null, cannot transition.", this);
                yield break;
            }
            
            var transitionTrigger = transitionType == (int)TransitionType.In ? transitionInTrigger : transitionOutTrigger;
            if (!HasTrigger(transitionTrigger))
            {
                if (allowDebug) Debug.LogWarning($"Transition Trigger: {transitionTrigger} not found, cannot transition.", this);
                TransitionCoroutine = null;
                yield break;
            }
            
            transitionAnimator.SetTrigger(transitionTrigger);
            
            var time = Time.time;
            float timeElapsed = 0;
// #if UNITY_EDITOR
            var debugSpacer = 0;
            const int mod = 20;
// #endif

            var transitioning = animatorRunning ?? false;
            switch (transitionDuration)
            {
                // If animator is not transitioning and duration is less than or equal to 0
                case <= 0 when !transitioning:
                    TransitionCoroutine = null;
                    yield break;
                // If animator is transitioning and duration is less than or equal to 0
                case <= 0:
                {
                    while (transitioning)
                    {
// #if UNITY_EDITOR
                        if (allowDebug && debugSpacer++ % mod == 0)
                        {
                            Debug.Log($"Time: {Time.time}, Time - time: {timeElapsed}, Wait Time: {transitionDuration}, " +
                                      $"Transitioning: {animatorRunning}", this);
                        }
// #endif
                        yield return WaitFixed;
                        transitioning = animatorRunning ?? false;
                        timeElapsed = Time.time - time;
                    }
                    break;
                }
                // If animator is transitioning and duration is greater than 0
                default:
                {
                    while (transitioning || timeElapsed < transitionDuration)
                    {
// #if UNITY_EDITOR
                        if (allowDebug && debugSpacer++ % mod == 0)
                        {
                            Debug.Log($"Time: {Time.time}, Time - time: {Time.time - time}, Wait Time: {transitionDuration}, " +
                                      $"Transitioning: {animatorRunning}", this);
                        }
// #endif
                        yield return WaitFixed;
                        transitioning = animatorRunning ?? false;
                        timeElapsed = Time.time - time;
                    }
                    break;
                }
            }

// #if UNITY_EDITOR
            if (allowDebug) Debug.Log($"Transition Complete, Time: {Time.time}", this);
// #endif
            yield return WaitFixed;
            
            TransitionCoroutine = null;
        }
    }
}

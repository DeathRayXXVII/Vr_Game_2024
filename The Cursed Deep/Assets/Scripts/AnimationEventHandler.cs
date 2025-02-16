using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHandler : MonoBehaviour
{
    [System.Serializable]
    private struct AnimationEvents
    {
        public string eventName;
        public UnityEvent onAnimationEvent;
    }
    
    [SerializeField] private AnimationEvents[] animationEvents;

    public void AnimationEvent(AnimationEvent triggeredEvent)
    {
        if (animationEvents == null || animationEvents.Length == 0) return;
        
        foreach (var animationEvent in animationEvents)
        {
            if (animationEvent.eventName != triggeredEvent.stringParameter || animationEvent.onAnimationEvent == null)
            {
                continue;
            }
            
            animationEvent.onAnimationEvent.Invoke();
            return;
        }
    }
}
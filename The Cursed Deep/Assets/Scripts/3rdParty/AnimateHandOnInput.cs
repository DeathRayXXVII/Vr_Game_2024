using UnityEngine;
using UnityEngine.InputSystem;

namespace XR_Assets.Hand_package.Lets_Make_a_VR_Game.Oculus_Hands.Scripts
{
    public class AnimateHandOnInput : MonoBehaviour
    {
        public InputActionProperty pinchAnimationAction;
        public InputActionProperty gripAnimationAction;
        public Animator handAnimator;

        // Update is called once per frame
        void Update()
        {
            float triggerValue = pinchAnimationAction.action.ReadValue<float>();
            handAnimator.SetFloat("Trigger", triggerValue);

            float gripValue = gripAnimationAction.action.ReadValue<float>();
            handAnimator.SetFloat("Grip", gripValue);
        }
    }
}


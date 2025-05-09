using UnityEngine;

namespace Player.StateMachine
{
    public class AttackStateBehaviour : StateMachineBehaviour
    {
        // Chamado quando a animação termina
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Acessa o AnimationEventBridge no mesmo objeto do Animator
            AnimationEventBridge bridge = animator.GetComponent<AnimationEventBridge>();
            if (bridge != null)
            {
                bridge.NotifyAnimationEnd(); // Comunica com o objeto principal
            }
        }
    }
}
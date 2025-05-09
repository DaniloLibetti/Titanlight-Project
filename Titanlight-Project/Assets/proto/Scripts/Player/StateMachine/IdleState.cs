using UnityEngine;

namespace Player.StateMachine
{
    public class IdleState : PlayerBaseState
    {
        public IdleState(PlayerStateMachine player) : base(player) { }

        public override void EnterState(PlayerStateMachine player)
        {
            player.animator.SetBool("IsWalking", false);
            player.animator.SetBool("IsDashing", false);

            player.animator.ResetTrigger("Attack");
            player.animator.ResetTrigger("RangedAttack");
            player.animator.ResetTrigger("HeavyAttack");

            player.rb.linearVelocity = Vector2.zero;
        }

        public override void UpdateState(PlayerStateMachine player)
        {
            // Use player.moveInput (não player.moveInput)
            if (player.moveInput != Vector2.zero) // Corrigido
            {
                player.SwitchState(player.MovingState);
            }
        }

        public override void FixedUpdateState(PlayerStateMachine player)
        {
            // nenhum movimento físico enquanto idle
        }

        public override void ExitState(PlayerStateMachine player)
        {
            // nada a limpar
        }
    }
}

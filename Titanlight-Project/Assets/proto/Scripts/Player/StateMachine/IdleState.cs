// File: IdleState.cs
using UnityEngine;

namespace Player.StateMachine
{
    public class IdleState : PlayerBaseState
    {
        public override void EnterState(PlayerStateMachine player)
        {
            // Desliga o flag de walk e dash
            player.animator.SetBool("IsWalking", false);
            player.animator.SetBool("IsDashing", false);

            // Zera trigger de ataque caso algo tenha ficado pendente
            player.animator.ResetTrigger("Attack");
            player.animator.ResetTrigger("RangedAttack");
            player.animator.ResetTrigger("HeavyAttack");

            // Para o corpo físico
            player.rb.linearVelocity = Vector2.zero;
        }

        public override void UpdateState(PlayerStateMachine player)
        {
            // Se começar a receber input de movimento, vai pro MovingState
            if (player.moveInput != Vector2.zero)
            {
                player.SwitchState(player.MovingState);
                return;
            }

            // Aqui você poderia detectar um comando de dash ou ataque,
            // mas a lógica de transição já está no PlayerStateMachine.
        }

        public override void FixedUpdateState(PlayerStateMachine player)
        {
            // nada
        }

        public override void ExitState(PlayerStateMachine player)
        {
            // nada
        }
    }
}

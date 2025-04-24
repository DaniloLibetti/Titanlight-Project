// File: MovingState.cs
using UnityEngine;

namespace Player.StateMachine
{
    public class MovingState : PlayerBaseState
    {
        public override void EnterState(PlayerStateMachine player)
        {
            // Liga o flag de walking para o Blend Tree
            player.animator.SetBool("IsWalking", true);
        }

        public override void UpdateState(PlayerStateMachine player)
        {
            // Se parar de mover, volta ao Idle
            if (player.moveInput == Vector2.zero)
            {
                player.SwitchState(player.IdleState);
                return;
            }
        }

        public override void FixedUpdateState(PlayerStateMachine player)
        {
            // Aplica o movimento suavemente
            Vector2 targetVel = player.moveInput * player.config.moveSpeed;
            float smoothTime = player.moveInput.magnitude > 0
                ? 1f / player.config.acceleration
                : 1f / player.config.deceleration;

            player.rb.linearVelocity = Vector2.SmoothDamp(
                player.rb.linearVelocity,
                targetVel,
                ref player.currentSmoothVelocity,
                smoothTime
            );
        }

        public override void ExitState(PlayerStateMachine player)
        {
            // Desliga o walking e garante velocidade zero
            player.animator.SetBool("IsWalking", false);
            player.rb.linearVelocity = Vector2.zero;
        }
    }
}

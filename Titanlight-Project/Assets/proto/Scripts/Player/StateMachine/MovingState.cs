using UnityEngine;

namespace Player.StateMachine
{
    public class MovingState : PlayerBaseState
    {
        public MovingState(PlayerStateMachine player) : base(player) { }

        public override void EnterState(PlayerStateMachine player)
        {
            player.animator.SetBool("IsWalking", true);
        }

        public override void UpdateState(PlayerStateMachine player)
        {
            if (player.moveInput == Vector2.zero)
            {
                player.SwitchState(player.IdleState);
            }
        }

        public override void FixedUpdateState(PlayerStateMachine player)
        {
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
            player.animator.SetBool("IsWalking", false);
            player.rb.linearVelocity = Vector2.zero;
        }
    }
}

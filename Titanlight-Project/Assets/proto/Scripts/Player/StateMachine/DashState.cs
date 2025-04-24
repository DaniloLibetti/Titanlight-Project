// File: DashState.cs
using UnityEngine;
using System.Collections;

namespace Player.StateMachine
{
    public class DashState : PlayerBaseState
    {
        private float dashTimer;
        private Vector2 dashDirection;
        private int originalLayer;

        public override void EnterState(PlayerStateMachine player)
        {
            // Define a dire��o do dash e salva a layer original
            dashDirection = player.lastDirection.normalized;
            originalLayer = player.gameObject.layer;
            player.gameObject.layer = LayerMask.NameToLayer("Dashing");

            // Desliga qualquer walking e liga o dash no Animator
            player.animator.SetBool("IsWalking", false);
            player.animator.SetBool("IsDashing", true);

            // Ajusta os floats do Blend Tree para a dire��o do dash
            player.animator.SetFloat("MoveX", dashDirection.x);
            player.animator.SetFloat("MoveY", dashDirection.y);

            dashTimer = player.config.dashDuration;
            player.canDash = false;
        }

        public override void UpdateState(PlayerStateMachine player)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                player.SwitchState(player.IdleState);
        }

        public override void FixedUpdateState(PlayerStateMachine player)
        {
            // Move o Rigidbody na dire��o do dash
            player.rb.linearVelocity = dashDirection * player.config.dashSpeed;
        }

        public override void ExitState(PlayerStateMachine player)
        {
            // Restaura layer, desliga a flag de dash e zera a velocidade
            player.gameObject.layer = originalLayer;
            player.animator.SetBool("IsDashing", false);
            player.rb.linearVelocity = Vector2.zero;

            // Reinicia o cooldown do dash
            player.StartCoroutine(ResetDashCooldown(player));
        }

        private IEnumerator ResetDashCooldown(PlayerStateMachine player)
        {
            yield return new WaitForSeconds(player.config.dashCooldown);
            player.canDash = true;
        }
    }
}

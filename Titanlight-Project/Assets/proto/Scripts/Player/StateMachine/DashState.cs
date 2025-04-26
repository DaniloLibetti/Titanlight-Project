using UnityEngine;
using System.Collections;

namespace Player.StateMachine
{
    public class DashState : PlayerBaseState
    {
        private float dashTimer;
        private Vector2 dashDirection;
        private int originalLayer;

        public DashState(PlayerStateMachine player) : base(player) { }

        public override void EnterState(PlayerStateMachine player)
        {
            player.LastDashPosition = player.transform.position;
            player.IsDashing = true;

            // Ativa apenas hitbox de dano, não altera collider principal
            // player.SetCollidersTrigger(true);  // removido para manter colisão com paredes

            originalLayer = player.gameObject.layer;
            player.gameObject.layer = LayerMask.NameToLayer("Dashing");

            dashDirection = player.lastDirection.normalized;
            player.animator.SetBool("IsWalking", false);
            player.animator.SetBool("IsDashing", true);
            player.animator.SetFloat("MoveX", dashDirection.x);
            player.animator.SetFloat("MoveY", dashDirection.y);

            dashTimer = player.config.dashDuration;
            player.SetCanDash(false);
        }

        public override void UpdateState(PlayerStateMachine player)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                player.SwitchState(player.IdleState);
        }

        public override void FixedUpdateState(PlayerStateMachine player)
        {
            float moveDist = player.config.dashSpeed * Time.fixedDeltaTime;
            Vector2 currentPos = player.rb.position;
            Vector2 disp = dashDirection * moveDist;

            // Checa colisão com parede antes de mover
            int wallMask = LayerMask.GetMask("Wall");
            RaycastHit2D hit = Physics2D.Raycast(currentPos, dashDirection, moveDist, wallMask);
            if (hit.collider != null)
            {
                // Para no ponto de contato
                Vector2 stopPos = hit.point - dashDirection * 0.01f;
                player.rb.MovePosition(stopPos);
                dashTimer = 0f; // encerra dash
            }
            else
            {
                player.rb.MovePosition(currentPos + disp);
            }
        }

        public override void ExitState(PlayerStateMachine player)
        {
            player.IsDashing = false;

            // player.SetCollidersTrigger(false); // não era alterado

            player.gameObject.layer = originalLayer;
            player.animator.SetBool("IsDashing", false);
            player.rb.linearVelocity = Vector2.zero;

            player.StartCoroutine(ResetDashCooldown(player));
        }

        private IEnumerator ResetDashCooldown(PlayerStateMachine player)
        {
            yield return new WaitForSeconds(player.config.dashCooldown);
            player.SetCanDash(true);
        }
    }
}

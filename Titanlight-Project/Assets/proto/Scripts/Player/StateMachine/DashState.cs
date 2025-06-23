using UnityEngine;
using System.Collections;

namespace Player.StateMachine
{
    public class DashState : PlayerBaseState
    {
        private float dashTimer;
        private Vector2 dashDirection;
        private int originalLayer;
        private int holeLayer;

        public DashState(PlayerStateMachine player) : base(player)
        {
            holeLayer = LayerMask.NameToLayer("HoleFloor");
        }

        public override void EnterState(PlayerStateMachine player)
        {
            player.IsDashing = true;
            originalLayer = player.gameObject.layer;

            // Muda para layer Dashing
            int dashingLayer = LayerMask.NameToLayer("Dashing");
            if (dashingLayer == -1)
                Debug.LogError("Dashing layer not found!");
            else
                player.gameObject.layer = dashingLayer;

            // Salva posição ANTES de mover
            player.LastDashPosition = player.transform.position;

            dashDirection = player.lastDirection.normalized;
            player.animator.SetBool("IsDashing", true);
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

            // Apenas colide com paredes
            RaycastHit2D hit = Physics2D.Raycast(
                currentPos,
                dashDirection,
                moveDist,
                LayerMask.GetMask("Wall")
            );

            if (hit.collider != null)
            {
                player.rb.MovePosition(hit.point - (Vector2)dashDirection * 0.1f);
                dashTimer = 0f;
            }
            else
            {
                player.rb.MovePosition(currentPos + disp);
            }
        }

        public override void ExitState(PlayerStateMachine player)
        {
            player.IsDashing = false;
            player.gameObject.layer = originalLayer;
            player.animator.SetBool("IsDashing", false);
            player.rb.linearVelocity = Vector2.zero;

            // Verificação de buraco pós-dash
            CheckForHoleAfterDash(player);

            player.StartCoroutine(ResetDashCooldown(player));
        }

        private void CheckForHoleAfterDash(PlayerStateMachine player)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                player.transform.position,
                0.3f, // Raio pequeno
                1 << holeLayer // Apenas layer HoleFloor
            );

            foreach (Collider2D hit in hits)
            {
                HoleTrigger hole = hit.GetComponent<HoleTrigger>();
                if (hole != null && !hole.IsFalling)
                {
                    hole.StartCoroutine(hole.HandleFall(player));
                    break;
                }
            }
        }

        private IEnumerator ResetDashCooldown(PlayerStateMachine player)
        {
            yield return new WaitForSeconds(player.config.dashCooldown);
            player.SetCanDash(true);
        }
    }
}
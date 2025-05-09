using UnityEngine;

namespace Player.StateMachine
{
    public class AttackState : PlayerBaseState
    {
        private int currentCombo;
        private Vector2 attackDirection;
        private bool inputQueued;
        private const float ComboWindow = 0.5f;
        private const float ComboCooldown = 1f;

        public AttackState(PlayerStateMachine player) : base(player) { }

        public override void EnterState(PlayerStateMachine player)
        {
            currentCombo = 1;
            inputQueued = false;
            attackDirection = player.lastDirection;
            PlayAttackAnimation(player, currentCombo);
        }

        public override void UpdateState(PlayerStateMachine player)
        {
            if (currentCombo < player.config.maxCombo && Input.GetKeyDown(KeyCode.O))
                inputQueued = true;

            if (player.moveInput != Vector2.zero)
                player.lastDirection = player.moveInput;
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
            player.animator.ResetTrigger("AttackRight1");
            player.animator.ResetTrigger("AttackRight2");
            player.animator.ResetTrigger("AttackRight3");
            player.animator.ResetTrigger("AttackLeft1");
            player.animator.ResetTrigger("AttackLeft2");
            player.animator.ResetTrigger("AttackLeft3");
            player.animator.ResetTrigger("AttackUp1");
            player.animator.ResetTrigger("AttackUp2");
            player.animator.ResetTrigger("AttackUp3");
            player.animator.ResetTrigger("AttackDown1");
            player.animator.ResetTrigger("AttackDown2");
            player.animator.ResetTrigger("AttackDown3");
        }

        private void PlayAttackAnimation(PlayerStateMachine player, int comboStep)
        {
            string trigger = GetDirectionalAnimation(comboStep);
            player.animator.SetTrigger(trigger);
        }

        public void ApplyDamage(PlayerStateMachine player)
        {
            Vector2 attackPos = (Vector2)player.transform.position + attackDirection * player.config.meleeAttackDistance;
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos, player.config.meleeAttackRadius);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<Health>(out var health))
                    health.TakeDamage(player.config.comboDamage[currentCombo - 1]);
            }
        }

        public void OnAnimationEnd(PlayerStateMachine player)
        {
            if (inputQueued && currentCombo < player.config.maxCombo)
            {
                currentCombo++;
                inputQueued = false;
                PlayAttackAnimation(player, currentCombo);
            }
            else
            {
                if (currentCombo >= player.config.maxCombo)
                    player.cooldownTimer = ComboCooldown;

                player.SwitchState(player.IdleState);
            }
        }

        private string GetDirectionalAnimation(int comboStep)
        {
            Vector2 dir = attackDirection.normalized;
            return Mathf.Abs(dir.x) > Mathf.Abs(dir.y) ?
                dir.x > 0 ? $"AttackRight{comboStep}" : $"AttackLeft{comboStep}" :
                dir.y > 0 ? $"AttackUp{comboStep}" : $"AttackDown{comboStep}";
        }
    }
}
using UnityEngine;
using System.Collections;

namespace Player.StateMachine
{
    /// <summary>
    /// Estado de invencibilidade temporária: jogador não toma dano e pisca.
    /// </summary>
    public class InvincibleState : PlayerBaseState
    {
        private float duration;
        private float blinkInterval;
        private SpriteRenderer spriteRenderer;

        public InvincibleState(PlayerStateMachine player, float duration, float blinkInterval) : base(player)
        {
            this.duration = duration;
            this.blinkInterval = blinkInterval;
            // assume sprite no children
            spriteRenderer = player.GetComponent<SpriteRenderer>() ?? player.GetComponentInChildren<SpriteRenderer>();
        }

        public override void EnterState(PlayerStateMachine player)
        {
            // inicia coroutine de invencibilidade
            player.StartCoroutine(InvincibilityCoroutine(player));
        }

        private IEnumerator InvincibilityCoroutine(PlayerStateMachine player)
        {
            float timer = 0f;
            while (timer < duration)
            {
                if (spriteRenderer != null)
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(blinkInterval);
                timer += blinkInterval;
            }
            if (spriteRenderer != null)
                spriteRenderer.enabled = true;
            // volta para Idle
            player.SwitchState(player.IdleState);
        }

        public override void UpdateState(PlayerStateMachine player) { }
        public override void FixedUpdateState(PlayerStateMachine player) { }
        public override void ExitState(PlayerStateMachine player) { }
    }
}

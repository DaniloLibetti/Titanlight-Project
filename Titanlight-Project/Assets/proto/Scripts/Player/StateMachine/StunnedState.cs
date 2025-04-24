// File: StunnedState.cs
using UnityEngine;
using System.Collections;

namespace Player.StateMachine
{
    public class StunnedState : PlayerBaseState
    {
        private float stunTimer;

        public override void EnterState(PlayerStateMachine player)
        {
            stunTimer = player.config.stunDuration;
            player.rb.linearVelocity = Vector2.zero;
        }

        public override void UpdateState(PlayerStateMachine player)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
                player.SwitchState(player.IdleState);
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

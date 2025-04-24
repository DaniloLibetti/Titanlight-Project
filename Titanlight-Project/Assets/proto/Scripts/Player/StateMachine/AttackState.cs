// File: AttackState.cs
using UnityEngine;
using System.Collections;

namespace Player.StateMachine
{
    public class AttackState : PlayerBaseState
    {
        public override void EnterState(PlayerStateMachine player)
        {
            player.animator.SetTrigger("Attack");
            if (player.config.isRangedMode)
                player.StartCoroutine(player.RangedAttack());
            else
                player.StartCoroutine(player.MeleeAttack());
        }

        public override void UpdateState(PlayerStateMachine player)
        {
            // troca de estado feita nas coroutines
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

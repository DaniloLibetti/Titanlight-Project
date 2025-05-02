using UnityEngine;

namespace Player.StateMachine
{
    public class AttackState : PlayerBaseState
    {
        public AttackState(PlayerStateMachine player) : base(player) { }

        public override void EnterState(PlayerStateMachine player)
        {
            // Opcional: dispara animação de ataque genérica
            player.animator.SetTrigger("Attack");
            // Se quiser, volte direto pro Idle:
            player.SwitchState(player.IdleState);
        }

        public override void UpdateState(PlayerStateMachine player)
        {
            // Nada aqui
        }

        public override void FixedUpdateState(PlayerStateMachine player)
        {
            // Nada aqui
        }

        public override void ExitState(PlayerStateMachine player)
        {
            // Nada aqui
        }
    }
}

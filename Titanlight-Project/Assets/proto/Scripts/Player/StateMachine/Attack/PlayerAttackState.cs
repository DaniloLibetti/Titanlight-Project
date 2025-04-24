// File: PlayerAttackState.cs
using UnityEngine;
using Player.StateMachine;

namespace Game
{
    // Estado que executa um único ataque (leve ou pesado)
    public class PlayerAttackState : PlayerState
    {
        [field: SerializeField]
        // Dados do ataque configurados no SO
        public PlayerAttackSO playerAttack { get; private set; }

        private float progress; // tempo normalizado [0,1]

        public override void Setup(PlayerStateMachine sm)
        {
            base.Setup(sm);
        }

        // — esses métodos devem ser public para que PlayerAttackingState
        //   consiga invocá-los em currentAttackState.*
        public override void Enter()
        {
            startTime = Time.time;
            IsComplete = false;

            if (playerAttack.IsSpecial)
                stateMachine.animator.SetTrigger("HeavyAttack");
            else
                stateMachine.animator.SetTrigger("Attack");
        }

        public override void Do()
        {
            progress = (Time.time - startTime) / playerAttack.Duration;
            stateMachine.animator.Play(playerAttack.Animation.name, 0, progress);

            if (Time.time - startTime >= playerAttack.Duration)
                IsComplete = true;
        }

        public override void FixedDo()
        {
            float curveVal = playerAttack.VelocityCurve.Evaluate(progress);
            // preserve y, só altera x
            var v = stateMachine.rb.linearVelocity;
            v.x = curveVal * playerAttack.MaxVelocity;
            stateMachine.rb.linearVelocity = v;
        }

        public override void Exit()
        {
            // nada extra
        }
    }
}

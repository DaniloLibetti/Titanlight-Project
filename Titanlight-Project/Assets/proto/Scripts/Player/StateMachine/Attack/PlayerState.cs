// File: PlayerState.cs
using UnityEngine;
using Player.StateMachine;

namespace Game
{
    // Base para todos os estados de ataque, agora integrado � m�quina de estados comum
    public abstract class PlayerState : PlayerBaseState
    {
        protected PlayerStateMachine stateMachine;    // refer�ncia � m�quina de estados do player
        protected float startTime;                    // momento que o estado foi iniciado
        public bool IsComplete { get; protected set; } // marca se o estado terminou

        // Chamado pela m�quina de estados ao entrar neste estado
        public override void EnterState(PlayerStateMachine sm)
        {
            Setup(sm);
            Enter();
        }

        // Chamado todo frame enquanto ativo
        public override void UpdateState(PlayerStateMachine sm)
        {
            Do();
            if (IsComplete)
            {
                // retorna ao estado Idle ao completar
                sm.nextState = sm.IdleState;
                sm.overrideStateCompletion = true;
            }
        }

        // Chamado em FixedUpdate
        public override void FixedUpdateState(PlayerStateMachine sm)
        {
            FixedDo();
        }

        // Chamado ao sair deste estado
        public override void ExitState(PlayerStateMachine sm)
        {
            Exit();
        }

        // Configura refer�ncia � m�quina de estados
        public virtual void Setup(PlayerStateMachine sm)
        {
            stateMachine = sm;
            startTime = Time.time;
            IsComplete = false;
        }

        // Implementados por cada estado espec�fico:
        public abstract void Enter();      // l�gica ao entrar no estado
        public abstract void Do();         // execu��o por frame
        public abstract void FixedDo();    // execu��o em FixedUpdate
        public abstract void Exit();       // limpeza ao sair do estado
    }
}

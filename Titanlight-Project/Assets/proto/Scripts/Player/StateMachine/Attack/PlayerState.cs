// File: PlayerState.cs
using UnityEngine;
using Player.StateMachine;

namespace Game
{
    // Base para todos os estados de ataque, agora integrado à máquina de estados comum
    public abstract class PlayerState : PlayerBaseState
    {
        protected PlayerStateMachine stateMachine;    // referência à máquina de estados do player
        protected float startTime;                    // momento que o estado foi iniciado
        public bool IsComplete { get; protected set; } // marca se o estado terminou

        // Chamado pela máquina de estados ao entrar neste estado
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

        // Configura referência à máquina de estados
        public virtual void Setup(PlayerStateMachine sm)
        {
            stateMachine = sm;
            startTime = Time.time;
            IsComplete = false;
        }

        // Implementados por cada estado específico:
        public abstract void Enter();      // lógica ao entrar no estado
        public abstract void Do();         // execução por frame
        public abstract void FixedDo();    // execução em FixedUpdate
        public abstract void Exit();       // limpeza ao sair do estado
    }
}

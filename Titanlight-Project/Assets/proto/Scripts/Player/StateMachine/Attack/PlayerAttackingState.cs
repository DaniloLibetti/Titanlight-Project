// File: PlayerAttackingState.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Player.StateMachine;

namespace Game
{
    // Gerencia fila de ataques (combos)
    public class PlayerAttackingState : PlayerState
    {
        [SerializeField]
        private PlayerComboSO[] playerCombos;

        private Dictionary<PlayerAttackSO, PlayerAttackState> attackStateMap;
        private PlayerAttackState currentAttackState;
        private PlayerAttackState queuedAttackState;
        private List<PlayerAttackSO> attackHistory;

        public override void Setup(PlayerStateMachine sm)
        {
            base.Setup(sm);
            attackHistory = new List<PlayerAttackSO>();
            attackStateMap = new Dictionary<PlayerAttackSO, PlayerAttackState>();

            foreach (var combo in playerCombos)
                foreach (var ca in combo.ComboAttacks)
                {
                    var st = new PlayerAttackState();
                    st.Setup(sm);
                    attackStateMap[ca.Attack] = st;
                }

            var actions = sm.playerInput.actions;
            actions["Punch"].performed += OnInput;
            actions["Kick"].performed += OnInput;
            actions["RunningAttack"].performed += OnInput;
            actions["Special"].performed += OnInput;
        }

        private void OnInput(InputAction.CallbackContext ctx)
        {
            if (queuedAttackState != null) return;
            if (!System.Enum.TryParse(ctx.action.name, out EPlayerInput input)) return;

            foreach (var combo in playerCombos)
            {
                int idx = attackHistory.Count;
                if (combo.ComboAttacks.Length <= idx) continue;
                var ca = combo.ComboAttacks[idx];
                if (ca.Input != input) continue;

                queuedAttackState = attackStateMap[ca.Attack];
                if (currentAttackState == null)
                {
                    stateMachine.nextState = this;
                    stateMachine.overrideStateCompletion = true;
                }
                break;
            }
        }

        public override void Enter()
        {
            IsComplete = false;
            startTime = Time.time;
        }

        public override void Do()
        {
            if (currentAttackState == null && queuedAttackState == null)
            {
                stateMachine.nextState = stateMachine.IdleState;
                attackHistory.Clear();
                IsComplete = true;
                return;
            }

            if (currentAttackState == null)
            {
                currentAttackState = queuedAttackState;
                queuedAttackState = null;
                attackHistory.Add(currentAttackState.playerAttack);
                currentAttackState.Enter();
            }
            else if (currentAttackState.IsComplete)
            {
                currentAttackState.Exit();
                currentAttackState = queuedAttackState;
                queuedAttackState = null;
                if (currentAttackState != null)
                {
                    attackHistory.Add(currentAttackState.playerAttack);
                    currentAttackState.Enter();
                }
            }

            currentAttackState?.Do();
        }

        public override void FixedDo()
        {
            currentAttackState?.FixedDo();
        }

        public override void Exit()
        {
            currentAttackState?.Exit();
            currentAttackState = null;
            queuedAttackState = null;
            attackHistory.Clear();
            IsComplete = false;
        }
    }
}

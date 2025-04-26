namespace Player.StateMachine
{
    public abstract class PlayerBaseState
    {
        protected PlayerStateMachine player;

        protected PlayerBaseState(PlayerStateMachine player)
        {
            this.player = player;
        }

        public abstract void EnterState(PlayerStateMachine player);
        public abstract void UpdateState(PlayerStateMachine player);
        public abstract void FixedUpdateState(PlayerStateMachine player);
        public abstract void ExitState(PlayerStateMachine player);
    }
}

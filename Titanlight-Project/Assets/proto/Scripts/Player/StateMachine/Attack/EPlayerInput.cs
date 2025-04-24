using Player.StateMachine;


namespace Game
{
    // Enum que deve casar com os nomes das Actions no InputSystem
    public enum EPlayerInput
    {
        Attack = 0,       // antes Punch
        HeavyAttack = 1,  // antes Kick
        RunningAttack = 2, // terceiro golpe do combo
        Special = 3,       // ataque pesado
    }
}

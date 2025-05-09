using UnityEngine;
using Player.StateMachine;

public class AnimationEventBridge : MonoBehaviour
{
    // Referência para o PlayerStateMachine (pai)
    private PlayerStateMachine playerStateMachine;

    private void Start()
    {
        // Acessa o PlayerStateMachine no GameObject pai
        playerStateMachine = transform.parent.GetComponent<PlayerStateMachine>();
    }

    // ▼▼▼ Método chamado pelo AttackStateBehaviour ▼▼▼
    public void NotifyAnimationEnd()
    {
        if (playerStateMachine != null)
        {
            // Avisa o PlayerStateMachine que a animação terminou
            playerStateMachine.OnAttackAnimationEnd();
        }
    }
}
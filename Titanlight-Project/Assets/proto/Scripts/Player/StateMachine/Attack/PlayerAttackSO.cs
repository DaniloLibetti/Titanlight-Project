using Player.StateMachine;

using UnityEngine;
using NaughtyAttributes;

namespace Game
{
    [CreateAssetMenu(fileName = "PlayerAttack", menuName = "Player/Attack")]
    // ScriptableObject que define parâmetros de um ataque
    public class PlayerAttackSO : ScriptableObject
    {
        [field: Header("PARAMETERS"), HorizontalLine(2f, EColor.Red)]
        [field: SerializeField]
        // Clip de animação do ataque
        public AnimationClip Animation { get; private set; }

        [field: SerializeField]
        // Dano causado pelo ataque
        public float Damage { get; private set; }

        [field: SerializeField, Min(0)]
        // Duração em segundos do ataque
        public float Duration { get; private set; }

        [field: SerializeField]
        // Curva de velocidade durante o ataque
        public AnimationCurve VelocityCurve { get; private set; }

        [field: SerializeField]
        // Velocidade máxima aplicada na curva
        public float MaxVelocity { get; private set; }

        [field: SerializeField, Min(0)]
        // Tempo de atordoamento no alvo
        public float StunDuration { get; private set; }

        [field: SerializeField]
        // Força de empurrão (knockback)
        public float KnockbackStrenght { get; private set; }

        [field: Header("SPECIAL"), HorizontalLine(2f, EColor.Orange)]
        [field: SerializeField]
        // Indica se é um ataque especial/pesado
        public bool IsSpecial { get; private set; }

        [field: SerializeField, ShowIf("IsSpecial")]
        // Custo para executar ataque especial
        public float SpecialCost { get; private set; }
    }
}

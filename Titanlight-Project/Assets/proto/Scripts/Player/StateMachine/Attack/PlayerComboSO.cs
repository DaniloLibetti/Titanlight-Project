using Player.StateMachine;

using UnityEngine;
using System;

namespace Game
{
    [CreateAssetMenu(fileName = "PlayerComboAttack", menuName = "Players/ComboAttack")]
    // Define sequências de inputs → ataques (combos)
    public class PlayerComboSO : ScriptableObject
    {
        [field: SerializeField]
        // Lista de ataques na ordem em que devem ser executados
        public PlayerComboAttack[] ComboAttacks { get; private set; }
    }

    [Serializable]
    // Mapeia um input a um SO de ataque
    public class PlayerComboAttack
    {
        [field: SerializeField]
        // Input esperado (enum)
        public EPlayerInput Input { get; private set; }

        [field: SerializeField]
        // Dados do ataque a ser executado
        public PlayerAttackSO Attack { get; private set; }
    }
}

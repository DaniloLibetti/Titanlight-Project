using UnityEngine;

namespace Player.Config
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Player/Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Combo Settings")]
        public int maxCombo = 3;
        public float comboWindow = 0.8f;
        public float[] comboDamage = { 10f, 15f, 20f };

        [Header("Movimento")]
        [Tooltip("Velocidade máxima de movimento")]
        public float moveSpeed = 5f;

        [Tooltip("Tempo para atingir velocidade máxima (0 = instantâneo)")]
        [Range(0, 1)] public float accelerationTime = 0.1f;

        [Tooltip("Tempo para parar completamente (0 = instantâneo)")]
        [Range(0, 1)] public float decelerationTime = 0.05f;

        [Header("Física")]
        [Tooltip("Massa do jogador (afeta impacto de forças)")]
        public float mass = 10f;

        [Tooltip("Arrasto linear (resistência ao movimento)")]
        public float linearDrag = 5f;

        [Tooltip("Arrasto angular (resistência à rotação)")]
        public float angularDrag = 0.05f;

        [Header("Dash")]
        public float dashSpeed = 15f;
        public float dashDuration = 0.2f;
        public float dashCooldown = 1f;

        [Header("Dash Damage Settings")]
        public float dashHitRadius = 1.0f;
        public int dashDamage = 10;

        [Header("Ataque Corpo a Corpo")]
        public float meleeAttackDistance = 1f;
        public float meleeAttackRadius = 0.5f;

        [Header("Ataque Geral")]
        public float attackCooldown = 0.5f;
        public bool isRangedMode = false;

        [Header("Ataque à Distância - Comum")]
        public float normalBulletSpeed = 25f;
        public float normalBulletLifetime = 0.5f;
        public float normalAttackCooldown = 0.5f;

        [Header("Ataque à Distância - Shotgun")]
        public float shotgunSpreadAngle = 45f;
        public int shotgunPelletCount = 6;
        public float shotgunCooldown = 0.7f;

        [Header("Ataque à Distância - Metralhadora")]
        public float machineGunCooldown = 0.2f;

        [Header("Sistema de Aquecimento")]
        public float heatMax = 100f;
        public float heatIncreaseRate = 20f;
        public float heatDecreaseRate = 15f;
        public float overheatThreshold = 20f;

        [Header("Mecânica de Carregamento")]
        public float maxChargeTime = 2f;
        public float chargeMultiplierMin = 1f;
        public float chargeMultiplierMax = 3f;

        [Header("Atordoamento")]
        public float stunDuration = 1.0f;

        [Header("Multiplicadores de Velocidade")]
        public float chargingSpeedMultiplier = 0.5f;
        public float machineGunSpeedMultiplier = 0.7f;
    }
}
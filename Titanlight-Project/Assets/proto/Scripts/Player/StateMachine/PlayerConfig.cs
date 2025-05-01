// File: PlayerConfig.cs  
using UnityEngine;

namespace Player.Config
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Player/Config")] // Cria asset no menu do Unity  
    public class PlayerConfig : ScriptableObject // Configs centralizadas num ScriptableObject  
    {
        [Header("Movimento")]
        public float moveSpeed = 5f; // velocidade base  
        public float acceleration = 10f; // aceleração do movimento  
        public float deceleration = 15f; // desaceleração  

        [Header("Dash")]
        public float dashSpeed = 15f; // velocidade durante dash  
        public float dashDuration = 0.2f; // tempo de duração  
        public float dashCooldown = 1f; // tempo de espera pra usar dnv  

        [Header("Dash Damage Settings")]
        public float dashHitRadius = 1.0f; // alcance do dano do dash  
        public int dashDamage = 10; // dano causado  

        [Header("Ataque Corpo a Corpo")]
        public float meleeAttackDistance = 1f; // distância do ataque  
        public float meleeAttackRadius = 0.5f; // área de colisão  
        public float meleeDamage = 10f; // dano  
        public float meleeCooldown = 0.5f; // intervalo entre ataques  

        [Header("Ataque Geral")]
        public float attackCooldown = 0.5f; // cooldown genérico  
        public bool isRangedMode = false; // modo ranged (true) ou melee (false)  

        [Header("Ataque à Distância - Comum")]
        public float normalBulletSpeed = 25f; // velocidade da bala  
        public float normalBulletLifetime = 0.5f; // tempo até bala desaparecer  
        public float normalAttackCooldown = 0.5f; // intervalo entre tiros  

        [Header("Ataque à Distância - Shotgun")]
        public float shotgunSpreadAngle = 45f; // ângulo do espalhamento  
        public int shotgunPelletCount = 6; // qtd de projéteis por tiro  
        public float shotgunCooldown = 0.7f; // cooldown do shotgun  

        [Header("Ataque à Distância - Metralhadora")]
        public float machineGunCooldown = 0.2f; // cooldown rápido p/ metralhadora  

        [Header("Sistema de Aquecimento")]
        public float heatMax = 100f; // limite de aquecimento  
        public float heatIncreaseRate = 20f; // qntd q aumenta por tiro  
        public float heatDecreaseRate = 15f; // qntd q resfria por segundo  
        public float overheatThreshold = 20f; // limite p/ entrar em overheat  

        [Header("Mecânica de Carregamento")]
        public float maxChargeTime = 2f; // tempo máximo de carga  
        public float chargeMultiplierMin = 1f; // multiplicador mínimo (sem carga)  
        public float chargeMultiplierMax = 3f; // multiplicador máximo (carga total)  

        [Header("Atordoamento")]
        public float stunDuration = 1.0f; // tempo q o player fica stunnado  

        [Header("Multiplicadores de Velocidade")]
        public float chargingSpeedMultiplier = 0.5f; // redução de velocidade ao carregar  
        public float machineGunSpeedMultiplier = 0.7f; // redução ao usar metralhadora  
    }
}
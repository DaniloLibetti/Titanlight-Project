using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Player.Config;
using Player.StateMachine; // ajuste conforme seu namespace de estados
// Note: garanta que Singleton<T> esteja no namespace correto.

namespace Player.StateMachine
{
    public enum RangedAttackType { Shotgun = 1, MachineGun = 0 }

    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput), typeof(Health))]
    public class PlayerStateMachine : Singleton<PlayerStateMachine>
    {
        public static PlayerStateMachine Instance { get; private set; }
        public static PlayerStateMachine Instance2 { get; private set; }

        public static event Action<PlayerStateMachine> HackPressed;
        public static event Action<PlayerStateMachine> HackReleased;
        public static event Action<PlayerStateMachine> InteractPressed;

        [Header("Identification")]
        [Range(1, 2)] public int playerIndex = 1;

        [Header("Recoil Settings")]
        public float baseRecoilForce = 2f;
        [Header("Combo Settings")]
        public float cooldownTimer;
        [Header("Config Scriptable")]
        public PlayerConfig config;
        [Header("Dash Settings")]
        public bool IsDashing { get; set; }
        public Vector3 LastDashPosition { get; set; }
        [Header("Invincibility")]
        public float invincibleDuration = 2f;
        public float blinkInterval = 0.1f;
        [Header("Movement Control")]
        public bool CanMove { get; private set; } = true;
        public bool CanDash { get; private set; } = true;
        public void SetCanMove(bool v) => CanMove = v;
        public void SetCanDash(bool v) => CanDash = v;

        [Header("References")]
        public Transform firePoint;
        public GameObject shotgunBulletPrefab;
        public GameObject machineGunBulletPrefab;
        public LayerMask projectileCollisionLayers;
        public Animator animator { get; private set; }
        public Rigidbody2D rb { get; private set; }

        [Header("Shotgun Config")]
        public int extraPelletsMultiplier = 2;
        public float minShotgunSpreadAngle = 10f;
        public float maxShotgunLifetimeMultiplier = 2f;

        [Header("Cooldowns")]
        public float shotgunCooldown = 0.7f;
        public float machineGunCooldown = 0.2f;

        [Header("Projectile Settings")]
        public float projectileSpeed = 25f;
        public float projectileLifetime = 0.5f;
        public float shootPointDistance = 0.5f;

        [Header("Interact & Hack")]
        [SerializeField] private float interactRadius = 1.5f;
        [SerializeField] private LayerMask interactLayer;

        // State machine
        public PlayerBaseState IdleState { get; private set; }
        public PlayerBaseState MovingState { get; private set; }
        public PlayerBaseState DashState { get; private set; }
        public PlayerBaseState StunnedState { get; private set; }
        public PlayerBaseState InvincibleState { get; private set; }
        public PlayerBaseState AttackState { get; private set; }
        public PlayerBaseState CurrentState { get; private set; }

        // Mechanics
        private const float ANALOG_DEADZONE = 0.2f;
        public Vector2 moveInput;
        public Vector2 currentSmoothVelocity;
        public Vector2 lastDirection = Vector2.right;
        public float currentHeat;
        public bool overheated;

        private Vector3 firePointInitialLocalPos;
        private Collider2D[] _colliders;
        private float nextFireTime = 0f;
        private RangedAttackType currentRangedType = RangedAttackType.MachineGun;

        // Health & death tracking
        private Health _health;
        private bool _handledDeath = false;

        protected override void Awake()
        {
            // Singleton setup para multiplayer local
            if (playerIndex == 1)
            {
                if (Instance == null)
                    base.Awake();
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                if (Instance2 == null)
                    Instance2 = this;
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }

            rb = GetComponent<Rigidbody2D>();
            animator = GetComponentInChildren<Animator>();
            firePointInitialLocalPos = firePoint != null ? firePoint.localPosition : Vector3.zero;
            _colliders = GetComponents<Collider2D>();

            // Inicializar LastDashPosition na posição inicial do player
            LastDashPosition = transform.position;

            // state machine init
            IdleState = new IdleState(this);
            MovingState = new MovingState(this);
            DashState = new DashState(this);
            StunnedState = new StunnedState(this);
            AttackState = new AttackState(this);
            InvincibleState = new InvincibleState(this, invincibleDuration, blinkInterval);

            CurrentState = IdleState;
            CurrentState.EnterState(this);

            // health setup
            _health = GetComponent<Health>();
            if (_health != null)
            {
                _health.OnDeath += OnLocalPlayerDeath;
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDeath -= OnLocalPlayerDeath;
            }
        }

        void Update()
        {
            if (!CanMove) return;

            UpdateFirePointTransform();
            if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
            UpdateAnimations();
            CurrentState.UpdateState(this);
            UpdateHeat();
        }

        void FixedUpdate() => CurrentState.FixedUpdateState(this);

        public void OnMove(InputValue value)
        {
            Vector2 raw = value.Get<Vector2>();
            if (raw.magnitude < ANALOG_DEADZONE)
                moveInput = Vector2.zero;
            else
            {
                float angle = Mathf.Atan2(raw.y, raw.x);
                float step = Mathf.PI / 4f;
                float snapped = Mathf.Round(angle / step) * step;
                moveInput = new Vector2(Mathf.Cos(snapped), Mathf.Sin(snapped));
                lastDirection = moveInput;
            }
        }

        public void OnFire(InputValue value)
        {
            if (currentRangedType == RangedAttackType.Shotgun)
            {
                if (value.isPressed && Time.time >= nextFireTime)
                {
                    nextFireTime = Time.time + shotgunCooldown;
                    StartCoroutine(ShotgunAttack());
                }
            }
            else // MachineGun
            {
                if (value.isPressed && Time.time >= nextFireTime && !overheated)
                {
                    nextFireTime = Time.time + machineGunCooldown;
                    StartCoroutine(MachineGunAttack());
                }
            }
        }

        public void OnSwitchWeapon(InputValue value)
        {
            if (!value.isPressed) return;
            currentRangedType = currentRangedType == RangedAttackType.MachineGun
                ? RangedAttackType.Shotgun
                : RangedAttackType.MachineGun;
            if (animator != null)
                animator.SetInteger("WeaponType", (int)currentRangedType);
        }

        public void OnMelee(InputValue value)
        {
            if (value.isPressed && cooldownTimer <= 0f && CurrentState != AttackState)
                SwitchState(AttackState);
        }

        public void OnDash(InputValue value)
        {
            if (value.isPressed && CanDash && moveInput != Vector2.zero)
            {
                // Salva posição antes do dash para retorno em caso de buraco
                LastDashPosition = transform.position;
                SwitchState(DashState);
            }
        }

        public void OnHack(InputValue value)
        {
            if (!CanMove) return;
            if (value.isPressed) HackPressed?.Invoke(this);
            else HackReleased?.Invoke(this);
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed) InteractPressed?.Invoke(this);
        }

        public void SwitchState(PlayerBaseState newState)
        {
            CurrentState.ExitState(this);
            CurrentState = newState;
            CurrentState.EnterState(this);
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;
            Vector2 dir = moveInput != Vector2.zero ? moveInput : lastDirection;
            animator.SetBool("IsWalking", moveInput != Vector2.zero);
            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
        }

        private void UpdateFirePointTransform()
        {
            if (firePoint == null) return;
            float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
            firePoint.localRotation = Quaternion.Euler(0f, 0f, angle);
            firePoint.localPosition = new Vector3(
                lastDirection.x * shootPointDistance,
                firePointInitialLocalPos.y,
                firePointInitialLocalPos.z
            );
        }

        private IEnumerator ShotgunAttack()
        {
            // animator.SetTrigger("RangedAttack");
            ShootShotgun();
            yield return new WaitForSeconds(shotgunCooldown);
        }

        private IEnumerator MachineGunAttack()
        {
            // animator.SetTrigger("RangedAttack");
            ShootProjectile(machineGunBulletPrefab);
            yield return new WaitForSeconds(machineGunCooldown);
        }

        private void ShootProjectile(GameObject prefab)
        {
            if (prefab == null || firePoint == null) return;
            var proj = Instantiate(prefab, firePoint.position, firePoint.rotation);
            if (proj.TryGetComponent<Projectile>(out var comp))
            {
                comp.Initialize(firePoint.right, projectileSpeed, projectileCollisionLayers);
                comp.damageMultiplier = 1f;
            }
            Destroy(proj, projectileLifetime);
        }

        private void ShootShotgun()
        {
            if (config == null) return;
            // Exemplo de uso de config; ajuste conforme seu PlayerConfig
            float t = 1f;
            int total = config.shotgunPelletCount + Mathf.RoundToInt((t - 1f) * extraPelletsMultiplier);
            float spread = Mathf.Lerp(minShotgunSpreadAngle, config.shotgunSpreadAngle, (t - 1f) / (config.chargeMultiplierMax - 1f));
            float life = projectileLifetime * Mathf.Lerp(1f, maxShotgunLifetimeMultiplier, (t - 1f) / (config.chargeMultiplierMax - 1f));

            for (int i = 0; i < total; i++)
            {
                float offset = -spread * .5f + spread * i / (total - 1);
                var rot = firePoint.rotation * Quaternion.Euler(0, 0, offset);
                var pellet = Instantiate(shotgunBulletPrefab, firePoint.position, rot);
                if (pellet.TryGetComponent<Projectile>(out var comp))
                {
                    comp.Initialize(rot * Vector2.right, projectileSpeed, projectileCollisionLayers);
                    comp.damageMultiplier = t;
                }
                Destroy(pellet, life);
            }

            rb.AddForce(-lastDirection * baseRecoilForce * (t - 1f), ForceMode2D.Impulse);
        }

        private void UpdateHeat()
        {
            if (currentRangedType == RangedAttackType.MachineGun && Time.time < nextFireTime)
            {
                if (config != null)
                {
                    currentHeat = Mathf.Min(currentHeat + config.heatIncreaseRate * Time.deltaTime, config.heatMax);
                    if (currentHeat >= config.heatMax) overheated = true;
                }
            }
            else
            {
                if (config != null)
                {
                    currentHeat = Mathf.Max(currentHeat - config.heatDecreaseRate * Time.deltaTime, 0f);
                    if (overheated && currentHeat <= config.overheatThreshold) overheated = false;
                }
            }
        }

        public void OnAttackAnimationEnd()
        {
            if (CurrentState is AttackState a)
                a.OnAnimationEnd(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsDashing) return;
            if (other.CompareTag("Enemy") && other.TryGetComponent<Health>(out var health))
                health.TakeDamage(config != null ? config.dashDamage : 0);
        }

        public void SetCollidersTrigger(bool trigger)
        {
            foreach (var c in _colliders) c.isTrigger = trigger;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }

        /// <summary>
        /// Chamado quando Health.OnDeath dispara. Trata estado de morte local e notifica GameManager.
        /// </summary>
        private void OnLocalPlayerDeath()
        {
            if (_handledDeath) return;
            _handledDeath = true;

            // Notifica GameManager de morte para contagem de runs/multiplayer
            GameManager.Instance.NotifyPlayerDied();

            // Transitar para estado de morte, se tiver um DeadState:
            // Exemplo: SwitchState(DeadState);
            // Se não tiver um estado de morte definido, mantenha em Idle ou outro estado adequado.
            // animator.SetTrigger("Death"); // comente ou ajuste conforme sua animação
        }
    }
}

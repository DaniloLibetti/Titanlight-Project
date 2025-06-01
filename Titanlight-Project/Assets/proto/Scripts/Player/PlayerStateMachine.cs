// PlayerStateMachine.cs
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Player.Config;

namespace Player.StateMachine
{
    public enum RangedAttackType { Shotgun = 1, MachineGun = 0 }

    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
    public class PlayerStateMachine : Singleton<PlayerStateMachine>
    {
        public static PlayerStateMachine Instance2 { get; private set; }

        [Header("Identification")]
        [Tooltip("1 = Player1 (Instance), 2 = Player2 (Instance2)")]
        [Range(1, 2)] public int playerIndex = 1;

        [Header("Recoil Settings")] public float baseRecoilForce = 2f;
        [Header("Combo Settings")] public float cooldownTimer;
        [Header("Config Scriptable")] public PlayerConfig config;
        [Header("Dash Settings")] public bool IsDashing { get; set; }
        public Vector3 LastDashPosition { get; set; }
        [Header("Invincibility")] public float invincibleDuration = 2f;
        public float blinkInterval = 0.1f;
        [Header("Movement Control")] public bool CanMove { get; private set; } = true;
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

        [Header("Shotgun Configuration")] public int extraPelletsMultiplier = 2;
        public float minShotgunSpreadAngle = 10f;
        public float maxShotgunLifetimeMultiplier = 2f;

        [Header("Cooldowns")] public float shotgunCooldown = 0.7f;
        public float machineGunCooldown = 0.2f;

        [Header("Projectile Settings")] public float projectileSpeed = 25f;
        public float projectileLifetime = 0.5f;
        public float shootPointDistance = 0.5f;

        [Header("Interact & Hack Settings")]
        [Tooltip("Raio de alcance para interagir/hackear")]
        [SerializeField] private float interactRadius = 1.5f;
        [SerializeField] private LayerMask interactLayer;

        // State Machine fields
        public PlayerBaseState IdleState { get; private set; }
        public PlayerBaseState MovingState { get; private set; }
        public PlayerBaseState DashState { get; private set; }
        public PlayerBaseState StunnedState { get; private set; }
        public PlayerBaseState InvincibleState { get; private set; }
        public PlayerBaseState AttackState { get; private set; }
        public PlayerBaseState CurrentState { get; private set; }

        // Mechanics fields
        public Vector2 moveInput;
        public Vector2 currentSmoothVelocity;
        public Vector2 lastDirection = Vector2.right;
        public float currentHeat;
        public bool overheated;
        private Vector3 firePointInitialLocalPos;
        private Collider2D[] _colliders;
        private float nextFireTime = 0f;
        private bool isRangedMode = true;
        private RangedAttackType currentRangedType = RangedAttackType.MachineGun;

        protected override void Awake()
        {
            if (Instance == null)
                base.Awake();
            else if (Instance2 == null)
                Instance2 = this;
            else
            {
                Debug.LogError("Já existem duas instâncias de PlayerStateMachine!");
                Destroy(gameObject);
                return;
            }

            rb = GetComponent<Rigidbody2D>();
            animator = GetComponentInChildren<Animator>();
            if (animator == null) Debug.LogError("Animator não encontrado.");
            if (firePoint != null) firePointInitialLocalPos = firePoint.localPosition;
            _colliders = GetComponents<Collider2D>();

            animator.SetInteger("WeaponType", (int)currentRangedType);

            // Initialize states
            IdleState = new IdleState(this);
            MovingState = new MovingState(this);
            DashState = new DashState(this);
            StunnedState = new StunnedState(this);
            AttackState = new AttackState(this);
            InvincibleState = new InvincibleState(this, invincibleDuration, blinkInterval);

            CurrentState = IdleState;
            CurrentState.EnterState(this);
        }

        void Update()
        {
            if (!CanMove) return;

            if (moveInput != Vector2.zero)
                lastDirection = moveInput;

            UpdateFirePointTransform();

            if (cooldownTimer > 0)
                cooldownTimer -= Time.deltaTime;

            UpdateAnimations();
            CurrentState.UpdateState(this);
            UpdateHeat();
        }

        void FixedUpdate() => CurrentState.FixedUpdateState(this);

        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        public void OnFire(InputValue value)
        {
            if (!isRangedMode) return;

            if (currentRangedType == RangedAttackType.Shotgun)
            {
                if (value.isPressed && Time.time >= nextFireTime)
                {
                    nextFireTime = Time.time + shotgunCooldown;
                    StartCoroutine(ShotgunAttack());
                }
            }
            else if (currentRangedType == RangedAttackType.MachineGun)
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
            if (value.isPressed)
            {
                currentRangedType = (currentRangedType == RangedAttackType.MachineGun)
                    ? RangedAttackType.Shotgun
                    : RangedAttackType.MachineGun;
                animator.SetInteger("WeaponType", (int)currentRangedType);
            }
        }

        public void OnMelee(InputValue value)
        {
            if (value.isPressed && cooldownTimer <= 0 && CurrentState != AttackState)
                SwitchState(AttackState);
        }

        public void OnDash(InputValue value)
        {
            if (value.isPressed && CanDash && moveInput != Vector2.zero)
                SwitchState(DashState);
        }

        // ---------------------
        // HACKACTION: desbloqueia porta, mas não atravessa
        // ---------------------
        public void OnHack(InputValue value)
        {
            Debug.Log($"OnHack chamado: isPressed={value.isPressed}, CanMove={CanMove}");
            if (!value.isPressed || !CanMove) return;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);
            foreach (Collider2D col in hits)
            {
                DoorTrigger door = col.GetComponent<DoorTrigger>();
                if (door != null)
                {
                    Debug.Log("Tentando hackear porta: " + door.gameObject.name);
                    door.Hack(); // apenas desbloqueia
                    return;
                }
            }
        }

        // ---------------------
        // INTERACTACTION: atravessa porta (se aberta) OU coleta item
        // ---------------------
        public void OnInteract(InputValue value)
        {
            Debug.Log($"OnInteract chamado: isPressed={value.isPressed}, CanMove={CanMove}");
            if (!value.isPressed || !CanMove) return;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);
            foreach (Collider2D col in hits)
            {
                DoorTrigger door = col.GetComponent<DoorTrigger>();
                if (door != null)
                {
                    Debug.Log("Tentando atravessar porta: " + door.gameObject.name);
                    door.Interact(); // atravessa somente se isOpen = true
                    return;
                }

                PickupableItem item = col.GetComponent<PickupableItem>();
                if (item != null)
                {
                    Debug.Log("Tentando coletar item: " + item.gameObject.name);
                    item.Interact();
                    return;
                }
            }
        }

        public void SwitchState(PlayerBaseState newState)
        {
            CurrentState.ExitState(this);
            CurrentState = newState;
            CurrentState.EnterState(this);
        }

        private void UpdateAnimations()
        {
            Vector2 dir = (moveInput != Vector2.zero) ? moveInput : lastDirection;
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
            animator.SetTrigger("RangedAttack");
            ShootShotgun();
            yield return new WaitForSeconds(shotgunCooldown);
        }

        private IEnumerator MachineGunAttack()
        {
            animator.SetTrigger("RangedAttack");
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
            float t = 1f;
            int total = config.shotgunPelletCount + Mathf.RoundToInt((t - 1f) * extraPelletsMultiplier);
            float spread = Mathf.Lerp(minShotgunSpreadAngle, config.shotgunSpreadAngle, (t - 1f) / (config.chargeMultiplierMax - 1f));
            float life = projectileLifetime * Mathf.Lerp(1f, maxShotgunLifetimeMultiplier, (t - 1f) / (config.chargeMultiplierMax - 1f));

            for (int i = 0; i < total; i++)
            {
                float offset = -spread * 0.5f + spread * i / (total - 1);
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
            if (isRangedMode && currentRangedType == RangedAttackType.MachineGun && moveInput != Vector2.zero)
            {
                currentHeat = Mathf.Min(currentHeat + config.heatIncreaseRate * Time.deltaTime, config.heatMax);
                overheated = currentHeat >= config.heatMax;
                if (overheated) SoundManager.PlaySound(SoundType.OVERHEAT);
            }
            else
            {
                currentHeat = Mathf.Max(currentHeat - config.heatDecreaseRate * Time.deltaTime, 0f);
                if (overheated && currentHeat <= config.overheatThreshold)
                    overheated = false;
            }
        }

        public void OnAttackAnimationEnd()
        {
            if (CurrentState is AttackState attackState)
                attackState.OnAnimationEnd(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsDashing) return;
            if (other.CompareTag("Enemy") && other.TryGetComponent<Health>(out var health))
                health.TakeDamage(config.dashDamage);
        }

        public void SetCollidersTrigger(bool trigger)
        {
            foreach (var col in _colliders)
                col.isTrigger = trigger;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}

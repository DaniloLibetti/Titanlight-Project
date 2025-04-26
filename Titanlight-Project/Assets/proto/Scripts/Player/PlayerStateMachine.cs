using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Player.Config;

namespace Player.StateMachine
{
    public enum RangedAttackType { Shotgun, MachineGun }

    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
    public class PlayerStateMachine : Singleton<PlayerStateMachine>
    {
        [Header("Config Scriptable")]
        public PlayerConfig config;

        [Header("Config Dash")]
        public bool IsDashing { get; set; }           // usado pelo HoleTrigger
        public Vector3 LastDashPosition { get; set; } // onde retorna após cair

        [Header("Invincible Settings")]
        public float invincibleDuration = 2f;
        public float blinkInterval = 0.1f;

        [Header("Movement & Dash Control")]
        public bool CanMove { get; private set; } = true;
        public void SetCanMove(bool v) => CanMove = v;

        public bool CanDash { get; private set; } = true;
        public void SetCanDash(bool v) => CanDash = v;

        [Header("Input Keys")]
        public KeyCode shotgunKey = KeyCode.C;
        public KeyCode machineGunKey = KeyCode.V;
        public KeyCode dashKey = KeyCode.Space;
        public KeyCode switchModeKey = KeyCode.Q;

        [Header("References & Prefabs")]
        public Transform firePoint;
        public GameObject shotgunBulletPrefab;
        public GameObject machineGunBulletPrefab;
        public LayerMask projectileCollisionLayers;
        public Animator animator { get; private set; }
        public Rigidbody2D rb { get; private set; }

        [Header("Shotgun Settings")]
        public int extraPelletsMultiplier = 2;
        public float minShotgunSpreadAngle = 10f;
        public float maxShotgunLifetimeMultiplier = 2f;
        public float baseRecoilForce = 2f;

        [Header("Ranged Cooldowns")]
        public float shotgunCooldown = 0.7f;
        public float machineGunCooldown = 0.2f;

        [Header("Shooting")]
        public float projectileSpeed = 25f;
        public float projectileLifetime = 0.5f;
        public float shootPointDistance = 0.5f;

        // Player states
        public PlayerBaseState IdleState { get; private set; }
        public PlayerBaseState MovingState { get; private set; }
        public PlayerBaseState DashState { get; private set; }
        public PlayerBaseState StunnedState { get; private set; }
        public PlayerBaseState InvincibleState { get; private set; }
        public PlayerBaseState CurrentState { get; private set; }

        // Cooldown trackers
        private float nextShotgunTime = 0f;
        private float nextMachineGunTime = 0f;

        // Runtime variables
        public Vector2 moveInput;
        public Vector2 currentSmoothVelocity;
        public Vector2 lastDirection = Vector2.right;
        public float currentHeat;
        public bool overheated;
        private bool isRangedMode = false;

        // Remember initial firePoint local position
        private Vector3 firePointInitialLocalPos;

        // Collider support
        private Collider2D[] _colliders;

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
                Debug.LogError("Animator not found in Player children.");

            if (firePoint != null)
                firePointInitialLocalPos = firePoint.localPosition;

            _colliders = GetComponents<Collider2D>();

            // Initialize states
            IdleState = new IdleState(this);
            MovingState = new MovingState(this);
            DashState = new DashState(this);
            StunnedState = new StunnedState(this);

            CurrentState = IdleState;
            CurrentState.EnterState(this);

            // Invincibility state with configurable duration and blink interval
            InvincibleState = new InvincibleState(this, invincibleDuration, blinkInterval);
        }

        void Update()
        {
            // If player is locked (e.g., falling in a hole), skip input and updates
            if (!CanMove)
                return;

            // Toggle melee/ranged mode
            if (Input.GetKeyDown(switchModeKey))
                isRangedMode = !isRangedMode;

            // Movement input
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(h, v).normalized;
            if (moveInput != Vector2.zero)
                lastDirection = moveInput;

            // Update firePoint orientation/position
            UpdateFirePointTransform();

            if (isRangedMode)
            {
                if (Input.GetKeyDown(shotgunKey) && Time.time >= nextShotgunTime)
                {
                    nextShotgunTime = Time.time + shotgunCooldown;
                    StartCoroutine(ShotgunAttack());
                }
                if (Input.GetKey(machineGunKey) && Time.time >= nextMachineGunTime && !overheated)
                {
                    nextMachineGunTime = Time.time + machineGunCooldown;
                    StartCoroutine(MachineGunAttack());
                }
            }

            // Dash input
            if (Input.GetKeyDown(dashKey) && CanDash && lastDirection != Vector2.zero)
                SwitchState(DashState);

            // Animations based on state
            UpdateAnimations();

            // State-specific logic & heat management
            CurrentState.UpdateState(this);
            UpdateHeat();
        }

        void FixedUpdate()
        {
            CurrentState.FixedUpdateState(this);
        }

        public void SwitchState(PlayerBaseState newState)
        {
            CurrentState.ExitState(this);
            CurrentState = newState;
            CurrentState.EnterState(this);
        }

        // Enable/disable trigger on all colliders
        public void SetCollidersTrigger(bool trigger)
        {
            foreach (var col in _colliders)
                col.isTrigger = trigger;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsDashing) return;
            if (other.TryGetComponent<Health>(out var h))
                h.TakeDamage(config.dashDamage);
        }

        private void UpdateAnimations()
        {
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
            GameObject proj = Instantiate(prefab, firePoint.position, firePoint.rotation);
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
            int extra = Mathf.RoundToInt((t - 1f) * extraPelletsMultiplier);
            int total = config.shotgunPelletCount + extra;
            float spread = Mathf.Lerp(minShotgunSpreadAngle, config.shotgunSpreadAngle,
                                       (t - 1f) / (config.chargeMultiplierMax - 1f));
            float life = projectileLifetime * Mathf.Lerp(1f, maxShotgunLifetimeMultiplier,
                                                         (t - 1f) / (config.chargeMultiplierMax - 1f));
            for (int i = 0; i < total; i++)
            {
                float offset = -spread * 0.5f + spread * i / (total - 1);
                Quaternion rot = firePoint.localRotation * Quaternion.Euler(0, 0, offset);
                GameObject pellet = Instantiate(shotgunBulletPrefab, firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, offset));
                if (pellet.TryGetComponent<Projectile>(out var comp))
                {
                    Vector2 dir = rot * Vector3.right;
                    comp.Initialize(dir, projectileSpeed, projectileCollisionLayers);
                    comp.damageMultiplier = t;
                }
                Destroy(pellet, life);
            }
            rb.AddForce(-lastDirection * baseRecoilForce * (t - 1f), ForceMode2D.Impulse);
        }

        private void UpdateHeat()
        {
            if (isRangedMode && Input.GetKey(machineGunKey) && !overheated)
            {
                currentHeat = Mathf.Min(currentHeat + config.heatIncreaseRate * Time.deltaTime, config.heatMax);
                if (currentHeat >= config.heatMax) overheated = true;
            }
            else
            {
                currentHeat = Mathf.Max(currentHeat - config.heatDecreaseRate * Time.deltaTime, 0f);
                if (overheated && currentHeat <= config.overheatThreshold)
                    overheated = false;
            }
        }
    }
}

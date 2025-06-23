using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Player.Config;
using Player.StateMachine;

namespace Player.StateMachine
{
    public enum RangedAttackType { Shotgun = 1, MachineGun = 0 }

    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
    public class PlayerStateMachine : MonoBehaviour
    {
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

        
        private Chest _nearbyChest;
        private bool _canInteract = true;

       
        public PlayerBaseState IdleState { get; private set; }
        public PlayerBaseState MovingState { get; private set; }
        public PlayerBaseState DashState { get; private set; }
        public PlayerBaseState StunnedState { get; private set; }
        public PlayerBaseState InvincibleState { get; private set; }
        public PlayerBaseState AttackState { get; private set; }
        public PlayerBaseState CurrentState { get; private set; }

        
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

        private Health _health;
        private bool _handledDeath = false;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponentInChildren<Animator>();

            var interactionCollider = gameObject.AddComponent<CircleCollider2D>();
            interactionCollider.radius = 0.7f;
            interactionCollider.isTrigger = true;

            if (rb != null && config != null)
            {
                rb.mass = config.mass;
                rb.linearDamping = config.linearDrag;
                rb.angularDamping = config.angularDrag;
            }

            if (firePoint != null)
            {
                firePointInitialLocalPos = firePoint.localPosition;
            }
            else
            {
                Debug.LogWarning("FirePoint não atribuído no PlayerStateMachine.");
            }

            _colliders = GetComponents<Collider2D>();
            LastDashPosition = transform.position;

            IdleState = new IdleState(this);
            MovingState = new MovingState(this);
            DashState = new DashState(this);
            StunnedState = new StunnedState(this);
            AttackState = new AttackState(this);
            InvincibleState = new InvincibleState(this, invincibleDuration, blinkInterval);

            CurrentState = IdleState;
            CurrentState.EnterState(this);

            _health = GetComponent<Health>();
        }

        private void Start()
        {
            if (_health != null)
            {
               
            }
        }

        void Update()
        {
            if (!CanMove) return;

            if (_health != null && _health.CurrentHealth <= 0 && !_handledDeath)
            {
                OnLocalPlayerDeath();
            }

            if (firePoint != null)
            {
                UpdateFirePointTransform();
            }

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            UpdateAnimations();
            CurrentState.UpdateState(this);
            UpdateHeat();
        }

        void FixedUpdate()
        {
            CurrentState.FixedUpdateState(this);
        }

        public void OnMove(InputValue value)
        {
            Vector2 raw = value.Get<Vector2>();
            if (raw.magnitude < ANALOG_DEADZONE)
            {
                moveInput = Vector2.zero;
            }
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
            else 
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
            {
                animator.SetInteger("WeaponType", (int)currentRangedType);
            }
        }

        public void OnMelee(InputValue value)
        {
            if (value.isPressed && cooldownTimer <= 0f && CurrentState != AttackState)
            {
                SwitchState(AttackState);
            }
        }

        public void OnDash(InputValue value)
        {
            if (value.isPressed && CanDash && moveInput != Vector2.zero)
            {
                LastDashPosition = transform.position;
                SwitchState(DashState);
            }
        }

        public void OnHack(InputValue value)
        {
            if (!CanMove) return;

            if (value.isPressed)
            {
                Debug.Log($"Jogador {playerIndex} pressionou Hack");
                HackPressed?.Invoke(this);
            }
            else
            {
                Debug.Log($"Jogador {playerIndex} soltou Hack");
                HackReleased?.Invoke(this);
            }
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed && _canInteract)
            {
                Debug.Log($"Jogador {playerIndex} pressionou Interact");
                InteractPressed?.Invoke(this);
                TryInteractWithChest();
            }
        }

        private void TryInteractWithChest()
        {
            if (_nearbyChest != null)
            {
                _nearbyChest.OpenChest();
                _canInteract = false;
                StartCoroutine(ResetInteractCooldown());
            }
        }

        private IEnumerator ResetInteractCooldown()
        {
            yield return new WaitForSeconds(0.5f);
            _canInteract = true;
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
            if (firePoint == null || lastDirection == Vector2.zero) return;

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
            try
            {
                ShootShotgun();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro no ataque de shotgun: {ex.Message}");
            }

            yield return new WaitForSeconds(shotgunCooldown);
        }

        private IEnumerator MachineGunAttack()
        {
            try
            {
                ShootProjectile(machineGunBulletPrefab);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro no ataque de metralhadora: {ex.Message}");
            }

            yield return new WaitForSeconds(machineGunCooldown);
        }

        private void ShootProjectile(GameObject prefab)
        {
            if (prefab == null || firePoint == null) return;

            GameObject proj = Instantiate(prefab, firePoint.position, firePoint.rotation);
            Projectile projectileComp = proj.GetComponent<Projectile>();

            if (projectileComp != null)
            {
                projectileComp.Initialize(firePoint.right, projectileSpeed, projectileCollisionLayers);
                projectileComp.damageMultiplier = 1f;
                SoundManager.PlaySound(SoundType.HEATLASER);
            }

            Destroy(proj, projectileLifetime);
        }

        private void ShootShotgun()
        {
            if (config == null || firePoint == null) return;

            float t = 1f;
            int total = config.shotgunPelletCount + Mathf.RoundToInt((t - 1f) * extraPelletsMultiplier);
            float spread = Mathf.Lerp(minShotgunSpreadAngle, config.shotgunSpreadAngle,
                                    (t - 1f) / (config.chargeMultiplierMax - 1f));
            float life = projectileLifetime * Mathf.Lerp(1f, maxShotgunLifetimeMultiplier,
                                    (t - 1f) / (config.chargeMultiplierMax - 1f));

            for (int i = 0; i < total; i++)
            {
                float offset = -spread * .5f + spread * i / (total - 1);
                Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, offset);
                GameObject pellet = Instantiate(shotgunBulletPrefab, firePoint.position, rot);
                Projectile projectileComp = pellet.GetComponent<Projectile>();

                if (projectileComp != null)
                {
                    projectileComp.Initialize(rot * Vector2.right, projectileSpeed, projectileCollisionLayers);
                    projectileComp.damageMultiplier = t;
                }

                Destroy(pellet, life);
            }

            rb.AddForce(-lastDirection * baseRecoilForce * (t - 1f), ForceMode2D.Impulse);
            SoundManager.PlaySound(SoundType.LASERSHOTGUN);
        }

        private void UpdateHeat()
        {
            if (config == null) return;

            if (currentRangedType == RangedAttackType.MachineGun && Time.time < nextFireTime)
            {
                currentHeat = Mathf.Min(currentHeat + config.heatIncreaseRate * Time.deltaTime, config.heatMax);
                if (currentHeat >= config.heatMax)
                {
                    overheated = true;
                    SoundManager.PlaySound(SoundType.OVERHEAT);
                }
            }
            else
            {
                currentHeat = Mathf.Max(currentHeat - config.heatDecreaseRate * Time.deltaTime, 0f);
                if (overheated && currentHeat <= config.overheatThreshold)
                {
                    overheated = false;
                }
            }
        }

        public void OnAttackAnimationEnd()
        {
            if (CurrentState is AttackState attackState)
            {
                attackState.OnAnimationEnd(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsDashing)
            {
                if (other.CompareTag("Enemy"))
                {
                    Health enemyHealth = other.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(config != null ? config.dashDamage : 0);
                    }
                }
            }

            if (other.CompareTag("Chest"))
            {
                _nearbyChest = other.GetComponent<Chest>();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Chest") && _nearbyChest != null && other.GetComponent<Chest>() == _nearbyChest)
            {
                _nearbyChest = null;
            }
        }

        public void SetCollidersTrigger(bool trigger)
        {
            foreach (Collider2D c in _colliders)
            {
                if (c != null)
                {
                    c.isTrigger = trigger;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }

        private void OnLocalPlayerDeath()
        {
            if (_handledDeath) return;
            _handledDeath = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.NotifyPlayerDied();
            }
        }
    }
}
// File: PlayerStateMachine.cs
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Player.Config;
using Player.StateMachine;

namespace Player.StateMachine
{
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
    public class PlayerStateMachine : Singleton<PlayerStateMachine>
    {
        [SerializeField] private KeyCode meleeKey = KeyCode.O;
        [SerializeField] private KeyCode shotgunKey = KeyCode.K;
        [SerializeField] private KeyCode machineGunKey = KeyCode.M;
        [SerializeField] private KeyCode dashKey = KeyCode.Space;
        [SerializeField] private KeyCode switchModeKey = KeyCode.Q;

        [HideInInspector] public Rigidbody2D rb;
        [HideInInspector] public Animator animator;
        [HideInInspector] public SpriteRenderer spriteRenderer;

        // Novo: referência ao Input System
        public PlayerInput playerInput { get; private set; }

        // Novo: campos para uso por estados baseados em PlayerState
        public PlayerBaseState nextState { get; set; }
        public bool overrideStateCompletion { get; set; }

        public PlayerConfig config;

        public PlayerBaseState CurrentState { get; private set; }
        public IdleState IdleState { get; private set; }
        public MovingState MovingState { get; private set; }
        public DashState DashState { get; private set; }
        public AttackState AttackState { get; private set; }
        public StunnedState StunnedState { get; private set; }

        [HideInInspector] public Vector2 moveInput;
        [HideInInspector] public Vector2 lastDirection = Vector2.down;
        [HideInInspector] public bool canDash = true;
        [HideInInspector] public Vector2 currentSmoothVelocity;

        protected override void Awake()
        {
            base.Awake();

            rb = GetComponent<Rigidbody2D>();
            animator = GetComponentInChildren<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            playerInput = GetComponent<PlayerInput>();

            if (animator == null)
                Debug.LogError("Animator não encontrado.");
            else if (animator.runtimeAnimatorController == null)
                Debug.LogError("AnimatorController não atribuído.");

            IdleState = new IdleState();
            MovingState = new MovingState();
            DashState = new DashState();
            AttackState = new AttackState();
            StunnedState = new StunnedState();
            SwitchState(IdleState);
        }

        void Update()
        {
            // Se um estado baseado em PlayerState definiu nextState e override, aplica imediatamente
            if (overrideStateCompletion && nextState != null)
            {
                SwitchState((PlayerBaseState)nextState);
                overrideStateCompletion = false;
                nextState = null;
            }

            // Lê input de movimento tradicional (eixos)
            moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ).normalized;
            if (moveInput != Vector2.zero)
                lastDirection = moveInput;

            animator.SetFloat("MoveX", lastDirection.x);
            animator.SetFloat("MoveY", lastDirection.y);

            if (Input.GetKeyDown(switchModeKey))
                ToggleAttackMode();

            if (Input.GetKeyDown(dashKey) && canDash && lastDirection != Vector2.zero)
                SwitchState(DashState);

            bool wantMelee = Input.GetKeyDown(meleeKey) && !config.isRangedMode;
            bool wantRanged = config.isRangedMode &&
                               (Input.GetKeyDown(meleeKey)
                                || Input.GetKeyDown(shotgunKey)
                                || Input.GetKey(machineGunKey));
            if ((CurrentState == IdleState || CurrentState == MovingState)
                && (wantMelee || wantRanged))
            {
                SwitchState(AttackState);
            }

            CurrentState.UpdateState(this);
        }

        void FixedUpdate()
        {
            CurrentState.FixedUpdateState(this);
        }

        public void SwitchState(PlayerBaseState newState)
        {
            CurrentState?.ExitState(this);
            CurrentState = newState;
            CurrentState.EnterState(this);
        }

        private void ToggleAttackMode()
        {
            config.isRangedMode = !config.isRangedMode;
            spriteRenderer.color = config.isRangedMode ? Color.green : Color.white;
        }

        public IEnumerator MeleeAttack()
        {
            // lógica melee
            yield return new WaitForSeconds(config.meleeCooldown);
            SwitchState(IdleState);
        }

        public IEnumerator RangedAttack()
        {
            // lógica ranged
            yield return new WaitForSeconds(config.attackCooldown);
            SwitchState(IdleState);
        }
    }
}

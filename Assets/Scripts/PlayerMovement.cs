using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerStats playerStats;
    private Animator animator;

    // Dash States
    private bool isDashing = false;
    private float dashTimeLeft;
    private float lastDashTime = -10f;

    // Input Actions
    private InputSystem_Actions inputActions;
    private InputAction moveAction;
    private InputAction dashAction;

    void OnEnable()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        
        moveAction = inputActions.Player.Move;
        dashAction = inputActions.Player.Dash;
        
        dashAction.performed += OnDashPerformed;
    }

    void OnDisable()
    {
        dashAction.performed -= OnDashPerformed;
        inputActions.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Game paused check
        if (Time.timeScale == 0) return;

        // Dash Input (using Input Action)
        if (Time.time >= lastDashTime + dashCooldown && dashAction.triggered)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                EndDash();
            }
            return; // Dash sırasında normal hareketi engelle
        }

        // Get movement input from Input Action
        moveInput = moveAction.ReadValue<Vector2>();

        // Hız sabitleme (Diagonal hareketi dengeler)
        if (moveInput.sqrMagnitude > 1)
        {
            moveInput.Normalize();
        }

        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            // Dash yönü: Hareket ediyorsa o yöne, etmiyorsa mevcut yöne
            Vector2 dashDir = moveInput == Vector2.zero ? (Vector2)transform.up : moveInput;
            rb.linearVelocity = dashDir * dashSpeed;
        }
        else
        {
            // Normal hareket
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        // Karakterin hareket hızını gönder (0 ise Idle, 0'dan büyükse Walk oynar)
        animator.SetFloat("Speed", moveInput.sqrMagnitude);

        // Eğer karakter hareket ediyorsa, son bakılan yönü hatırla (Blend Tree için)
        if (moveInput != Vector2.zero)
        {
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);
        }

        // Sprite flip: sola gidiyorsa flipX = true, sağa gidiyorsa false
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && moveInput.x != 0f)
        {
            sr.flipX = moveInput.x < 0f;
        }
    }

    void OnDashPerformed(InputAction.CallbackContext context)
    {
        if (Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time;

        if (animator != null) animator.SetBool("isDashing", true);

        // Ölümsüzlük ver
        if (playerStats != null) playerStats.SetInvincible(true);

        // Phantom Dash Ödül Mantığı
        HandlePhantomDash();
    }

    void EndDash()
    {
        isDashing = false;
        if (animator != null) animator.SetBool("isDashing", false);
        
        if (playerStats != null) playerStats.SetInvincible(false);
    }

    private void HandlePhantomDash()
    {
        if (BossRewardManager.Instance != null && BossRewardManager.Instance.HasReward(BossRewardType.PhantomDash))
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3f);
            foreach (var hit in hits)
            {
                EnemyAI enemy = hit.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(20f);
                    enemy.ApplyKnockback(transform.position, 10f);
                }
                
                BossAI boss = hit.GetComponent<BossAI>();
                if (boss != null)
                {
                    boss.TakeDamage(20f);
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : BaseMovement
{
    public static Player instance;
    public PlayerInput input;
    private SpriteRenderer spriteRenderer;
    private HealthSystem healthSystem;

    // Coyote time and double jumping
    [Header("Player")]
    [SerializeField] float coyoteTime = 0.1f;
    private float airTime = 0;
    private bool canCoyoteJump = false;
    private bool canDoubleJump = false;

    private WallJumping wallJumping;
    private Dashing dashing;

    // Checkpoints, souls, and stats
    private Checkpoint lastCheckpoint;
    public List<Soul> followingSouls = new List<Soul>();
    public int soulsCollected = 0;
    public int deaths = 0;
    [SerializeField] float deathTime = 1f;
    private bool dying = false;
    [SerializeField] StatsUI statsUI;
    
    public bool GroundPounding;
    [SerializeField] private float GroundPoundSpeed = 10f;
    [SerializeField] PauseUI pauseUI;

    // Called before Start()
    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
            input = new PlayerInput();

            foreach (InputAction action in input)
            {
                action.Enable();
            }

            // Add listeners
            input.Player.Crouch.performed += Crouch;
            input.Player.Crouch.canceled += Uncrouch;
            input.Player.Jump.performed += DoubleJump;
        }
        else
        {
            instance.transform.SetPositionAndRotation(transform.position, transform.rotation);
            Destroy(transform.root.gameObject);
        }
    }

    private void OnDisable()
    {
        if(instance == this)
        {
            foreach (InputAction action in input)
            {
                action.Disable();
            }

            // Remove listeners
            input.Player.Crouch.performed -= Crouch;
            input.Player.Crouch.canceled -= Uncrouch;
            input.Player.Jump.performed -= DoubleJump;
            pauseUI.RemoveListener();
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthSystem = GetComponent<HealthSystem>();
        wallJumping = GetComponent<WallJumping>();
        dashing = GetComponent<Dashing>();
    }

    // Called every time the Physics engine updates (not tied to framerate, fixed rate)
    private void FixedUpdate()
    {
        if (Time.timeScale > 0)
        {
            Vector2 newVelocity;
            if (wallJumping.IsJumping)
            {
                newVelocity = rb.velocity;
                dashing.velocity = Vector2.zero;
            }
            else if (dashing.velocity != Vector2.zero)
            {
                newVelocity = dashing.velocity;
                if (dashing.velocity.y == 0 && (dashing.velocity.x > 0 && moveVelocity.x < 0 || dashing.velocity.x < 0 && moveVelocity.x > 0))
                {
                    dashing.velocity = Vector2.zero;
                    newVelocity = moveVelocity;
                }
            }
            else
            {
                newVelocity = moveVelocity;
            }

            if (!Climbing && dashing.velocity.y == 0 && !wallJumping.IsJumping)
                newVelocity += new Vector2(0, rb.velocity.y);

            if (wallJumping.IsSliding && !GroundPounding)
            {
                if(newVelocity.y < -wallJumping.slideSpeed)
                    newVelocity.y = -wallJumping.slideSpeed;
            }

            newVelocity += knockbackVelocity;

            ApplyVelocity(newVelocity);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        MovementUpdate();
        if(Time.timeScale > 0)
        {
            // Read player input for movement
            Crouching = input.Player.Crouch.ReadValue<float>() >= 1f;
            Walking = !Crouching && input.Player.Walk.ReadValue<float>() >= 1f;
            Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
            moveVelocity = moveInput;
            if (Walking)
            {
                moveVelocity *= walkSpeed;
            }
            else if (Crouching)
            {
                moveVelocity *= crouchSpeed;
            }
            else
            {
                moveVelocity *= runSpeed;
            }

            if (Climbing)
                moveVelocity.y = moveInput.y * climbSpeed;
            else
                moveVelocity.y = 0;

            if (input.Player.Jump.ReadValue<float>() >= 1f)
            {
                Jump();
            }

            if(!IsGrounded)
                airTime += Time.deltaTime;

            if(stairs != null && moveInput.y < 0)
            {
                stairs.Descend(_collider);
                stairs = null;
            }
        }
    }

    void Crouch(InputAction.CallbackContext context)
    {
        if (Time.timeScale > 0)
        {
            //movementAudio.PlayCrouch();
            transform.localScale = new Vector3(1, 0.5f, 1);
            transform.position -= new Vector3(0, 0.5f);
            if(!IsGrounded & !GroundPounding) StartCoroutine(GroundPound());
        }
    }

    IEnumerator GroundPound(){
        while(!IsGrounded){
            GroundPounding = true;
            moveVelocity.y = -GroundPoundSpeed;
            moveVelocity.x = 0f;
            rb.velocity = moveVelocity;
            yield return new WaitForEndOfFrame();
        }
        if(IsGrounded) GroundPounding = false;
    }

    void Uncrouch(InputAction.CallbackContext context)
    {
        if (Time.timeScale > 0)
        {
            movementAudio.PlayCrouch();
            transform.localScale = new Vector3(1, 1, 1);
            transform.position += new Vector3(0, 0.5f);
        }
    }

    void Jump()
    {
        if(Time.timeScale > 0 && rb.velocity.y <= 0 && (IsGrounded || (canCoyoteJump && airTime <= coyoteTime)))
        {
            canCoyoteJump = false;
            if(IsGrounded)
                PlayJumpSound();
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        }
    }

    void DoubleJump(InputAction.CallbackContext context)
    {
        if (Time.timeScale > 0 && !IsGrounded && canDoubleJump && (airTime > coyoteTime || !canCoyoteJump) && dashing.velocity == Vector2.zero)
        {
            canDoubleJump = false;
            movementAudio.PlayDoubleJump();
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        }
    }

    public void KillPlayer()
    {
        if (!dying)
            StartCoroutine(DieAnimation());
    }

    IEnumerator DieAnimation()
    {
        dying = true; // Stop code from playing animation over and over again
        rb.constraints = RigidbodyConstraints2D.FreezeAll; // Stop player movement

        // Change sprite color to red
        Color oldColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(deathTime);
        // Respawn player at lastCheckpoint
        spriteRenderer.color = oldColor;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.position = lastCheckpoint.transform.position;

        // Reset all souls that were following
        foreach (Soul followingSoul in followingSouls)
        {
            followingSoul.ResetSoul();
        }
        followingSouls.Clear();
        deaths++;
        statsUI.PopupUI(deaths, soulsCollected, 0.5f); // Show stats
        dying = false;
        IsGrounded = false;
        healthSystem.ResetHealth();
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        checkpoint.Save(followingSouls.Count);

        foreach (Soul followingSoul in followingSouls)
        {
            soulsCollected++;
            followingSoul.Fade();
        }
        // If we saved new souls show stats
        if (followingSouls.Count > 0)
            statsUI.PopupUI(deaths, soulsCollected, 0.5f);
        followingSouls.Clear();

        // If we're setting a new checkpoint
        if (lastCheckpoint != checkpoint)
        {
            // Show stats
            statsUI.PopupUI(deaths, soulsCollected, 0.5f);

            // If the last checkpoint isn't null, unselect it
            if (lastCheckpoint != null)
                lastCheckpoint.Deselect();

            // Select this checkpoint
            lastCheckpoint = checkpoint;
        }
    }

    public void PickupSoul(Soul soul)
    {
        if (soul.target == null)
        {
            if (followingSouls.Count > 0)
            {
                // Get this soul to start following the last soul in the chain
                soul.StartFollow(followingSouls[^1].transform);
                followingSouls.Add(soul);
            }
            else
            {
                // This soul is the first soul so its first in the chain and follows the player
                soul.StartFollow(transform);
                followingSouls.Add(soul);
            }
        }
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        float normalAngle = Vector2.Angle(collision.GetContact(0).normal, Vector2.up);
        if (TryLand(collision, normalAngle))
        {
            canDoubleJump = true;
            canCoyoteJump = true;
            airTime = 0;
            wallJumping.ResetJumps();
        }
        CheckWall(collision, normalAngle);

        switch (collision.transform.tag)
        {
            case "DeathZone":
                KillPlayer();
                break;
        }
    }

    public override void OnCollisionExit2D(Collision2D collision)
    {
        ExitCollision(collision);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "DeathZone":
                KillPlayer();
                break;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : BaseMovement
{
    public static PlayerInput playerInput;

    public static Player player;
    private SpriteRenderer spriteRenderer;
    private HealthSystem healthSystem;

    // Coyote time and double jumping
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

    // Called before Start()
    private void OnEnable()
    {
        playerInput = new PlayerInput();

        player = this;

        foreach (InputAction action in playerInput)
        {
            action.Enable();
        }

        // Add listeners
        playerInput.Player.Crouch.performed += Crouch;
        playerInput.Player.Crouch.canceled += Uncrouch;
        playerInput.Player.Jump.performed += DoubleJump;
    }

    private void OnDisable()
    {
        foreach (InputAction action in playerInput)
        {
            action.Disable();
        }

        // Remove listeners
        playerInput.Player.Crouch.performed -= Crouch;
        playerInput.Player.Crouch.canceled -= Uncrouch;
        playerInput.Player.Jump.performed -= DoubleJump;
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
            if (dashing.velocity != Vector2.zero)
            {
                newVelocity = dashing.velocity;
                if (dashing.velocity.y == 0 && (dashing.velocity.x > 0 && moveVelocity.x < 0 || dashing.velocity.x < 0 && moveVelocity.x > 0))
                {
                    dashing.velocity = Vector2.zero;
                    newVelocity = moveVelocity;
                }
            }
            else if(wallJumping.IsJumping)
            {
                newVelocity = rb.velocity;
            }
            else
            {
                newVelocity = moveVelocity;
            }

            if (!Climbing && dashing.velocity.y == 0 && !wallJumping.IsJumping)
                newVelocity += new Vector2(0, rb.velocity.y);

            if (wallJumping.IsSliding)
            {
                if(newVelocity.y < -wallJumping.slideSpeed)
                    newVelocity.y = -wallJumping.slideSpeed;
            }

            newVelocity += knockbackVelocity;

            rb.velocity = newVelocity;
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if(Time.timeScale > 0)
        {
            // Read player input for movement
            Crouching = playerInput.Player.Crouch.ReadValue<float>() >= 1f;
            Walking = !Crouching && playerInput.Player.Walk.ReadValue<float>() >= 1f;
            Vector2 moveInput = playerInput.Player.Move.ReadValue<Vector2>();
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

            if (playerInput.Player.Jump.ReadValue<float>() >= 1f)
            {
                Jump();
            }

            if(!IsGrounded)
                airTime += Time.deltaTime;

            if(stairs != null && moveInput.y < 0)
            {
                stairs.Descend(myCollider);
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
        if(Time.timeScale > 0 && dashing.velocity == Vector2.zero && rb.velocity.y <= 0 && (IsGrounded || (canCoyoteJump && airTime <= coyoteTime)))
        {
            canCoyoteJump = false;
            if(IsGrounded)
                PlayJumpSound();
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }
    }

    void DoubleJump(InputAction.CallbackContext context)
    {
        if (Time.timeScale > 0 && !IsGrounded && canDoubleJump && (airTime > coyoteTime || !canCoyoteJump) && dashing.velocity == Vector2.zero)
        {
            canDoubleJump = false;
            movementAudio.PlayDoubleJump();
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(TryLand(collision))
        {
            canDoubleJump = true;
            canCoyoteJump = true;
            airTime = 0;
            wallJumping.ResetJumps();
        }
        CheckWall(collision);

        switch (collision.transform.tag)
        {
            case "DeathZone":
                KillPlayer();
                break;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : BaseMovement
{
    public static PlayerInput playerInput;
    SpriteRenderer spriteRenderer;
    HealthSystem healthSystem;

    [SerializeField] float coyoteTime = 0.1f;
    float airTime = 0;
    Vector2 moveVelocity;

    bool canCoyoteJump = false;
    bool canDoubleJump = false;

    [SerializeField] float dashSpeed = 10;
    [SerializeField] float dashDuration = 0.25f;
    [SerializeField] float dashCooldown = 0.5f;
    [SerializeField] float dashDrag = 0.5f;
    [SerializeField] SpriteRenderer dashIndicator;
    bool canDash = true;
    bool tryDash = false;
    Vector2 dashVelocity = Vector2.zero;
    Vector2 knockbackVelocity;

    [SerializeField] Gun gun;

    Checkpoint lastCheckpoint;
    public List<Soul> followingSouls = new List<Soul>();
    public int soulsSaved = 0;
    public int deaths = 0;
    [SerializeField] float deathTime = 1f;
    bool dying = false;
    
    [SerializeField] StatsUI statsUI;

    // Called before Start()
    private void OnEnable()
    {
        playerInput = new PlayerInput();

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
    }

    // Called every time the Physics engine updates (not tied to framerate, fixed rate)
    private void FixedUpdate()
    {
        if(Time.timeScale > 0)
        {
            Vector2 newVelocity;
            if (dashVelocity != Vector2.zero)
            {
                newVelocity = dashVelocity;
                if (dashVelocity.y == 0 && (dashVelocity.x > 0 && moveVelocity.x < 0 || dashVelocity.x < 0 && moveVelocity.x > 0))
                {
                    dashVelocity = Vector2.zero;
                    newVelocity = moveVelocity;
                }
            }
            else
            {
                newVelocity = moveVelocity;
            }

            if (!Climbing && dashVelocity.y == 0)
                newVelocity += new Vector2(0, rb.velocity.y);
            newVelocity += knockbackVelocity

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

            if (playerInput.Player.Jump.ReadValue<float>() >= 1f && canCoyoteJump)
            {
                Jump();
            }

            // Check for dash
            if (playerInput.Player.Dash.ReadValue<float>() >= 1f && moveInput != Vector2.zero)
            {
                tryDash = true;
                if(canDash)
                    StartCoroutine(Dash(moveInput));
            }
            else
            {
                tryDash = false;
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
        if(Time.timeScale > 0)
        {
            movementAudio.PlayCrouch();
            transform.localScale = new Vector3(1, 0.5f, 1);
            transform.position -= new Vector3(0, 0.5f);
        }
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
        if(Time.timeScale > 0 && dashVelocity == Vector2.zero && (IsGrounded || (canCoyoteJump && airTime <= coyoteTime)))
        {
            canCoyoteJump = false;
            if(IsGrounded)
                movementAudio.PlayJump(ground.tag);
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }
    }

    void DoubleJump(InputAction.CallbackContext context)
    {
        if (Time.timeScale > 0 && !IsGrounded && canDoubleJump && (airTime > coyoteTime || !canCoyoteJump) && dashVelocity == Vector2.zero)
        {
            canDoubleJump = false;
            movementAudio.PlayDoubleJump();
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }
    }

    IEnumerator Dash(Vector2 moveInput)
    {
        canDash = false;
        movementAudio.PlayDash();
        dashVelocity = moveInput * dashSpeed;
        dashIndicator.color = Color.red;
        yield return new WaitForSeconds(dashDuration);
        if (!tryDash || IsGrounded)
        {
            // Stop dash momentum if we aren't bunny hopping
            dashVelocity = Vector2.zero;
            rb.velocity = Vector2.zero;
        }
        else
        {
            // If we're bunny hopping, preserve x velocity but stop vertical dash momentum
            if(dashVelocity.y != 0)
            {
                dashVelocity.y = 0;
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
        }
        yield return new WaitForSeconds(dashCooldown - dashDuration);
        while (!IsGrounded)
        {
            dashVelocity.x = Mathf.Lerp(dashVelocity.x, 0, Time.deltaTime * dashDrag);
            yield return new WaitForFixedUpdate();
        }
        dashVelocity = Vector2.zero;
        dashIndicator.color = Color.green;
        canDash = true;
    }

    public IEnumerator ApplyNailKnockback(Vector2 velocity, float KnockbackDuration, float drag){
        knockbackVelocity = velocity;
        float timer = KnockbackDuration;
        while(timer > 0){
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, Time.deltaTime*drag);
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        knockbackVelocity = Vector2.zero;
    }

    public void KillPlayer()
    {
        if(!dying)
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
        statsUI.PopupUI(deaths, soulsSaved, 0.5f); // Show stats
        dying = false;
        IsGrounded = false;
        ground = null;
        healthSystem.ResetHealth();
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        checkpoint.Save(followingSouls.Count);

        foreach (Soul followingSoul in followingSouls)
        {
            soulsSaved++;
            followingSoul.Fade();
        }
        // If we saved new souls show stats
        if (followingSouls.Count > 0)
            statsUI.PopupUI(deaths, soulsSaved, 0.5f);
        followingSouls.Clear();

        // If we're setting a new checkpoint
        if (lastCheckpoint != checkpoint)
        {
            // Show stats
            statsUI.PopupUI(deaths, soulsSaved, 0.5f);

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
        if (!collision.transform.name.Contains("Trap"))
        {
            if(Vector2.Angle(collision.GetContact(0).normal, Vector2.up) < 80)
            {
                Land(collision);
                canDoubleJump = true;
                canCoyoteJump = true;
                airTime = 0;
                rb.sharedMaterial.friction = 0.5f; // Prevent player from sliding off ramps and flying super far
            }
        }
        switch (collision.transform.tag)
        {
            case "DeathZone":
                KillPlayer();
                break;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(ground == collision.transform)
        {
            ExitGround();
            rb.sharedMaterial.friction = 0; // Prevent player from hanging onto edges of platforms
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch(other.tag)
        {
            case "DeathZone":
                KillPlayer();
                break;
        }
    }
}

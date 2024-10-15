using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public bool isWallSliding;
    public float wallSlidingSpeed = 0.2f;
    private float wallJumpDuration = 0.2f;
    private bool isWallJumping;
    private float wallJumpingDirection;
    public float wallSlideCounter;
    public float wallJumpAmount = 3f;
    public Vector2 wallJumpingPower = new Vector2(10f, 12f);

    public static PlayerInput playerInput;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    HealthSystem healthSystem;

    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float crouchSpeed = 1f;
    [SerializeField] float jumpVelocity = 5f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    bool isGrounded = false;
    Vector2 moveVelocity;

    bool canDoubleJump = false;

    [SerializeField] float dashSpeed = 15;
    [SerializeField] float dashDuration = 0.25f;
    [SerializeField] float dashCooldown = 0.5f;
    [SerializeField] SpriteRenderer dashIndicator;
    bool canDash = true;
    Vector2 dashVelocity = Vector2.zero;
    Vector2 wallJumpVelocity = Vector2.zero;

    [SerializeField] Gun gun;

    [SerializeField] MovementAudio movementAudio;
    Transform ground;
    float footstepTimer = 0;

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
        playerInput.Player.Jump.performed += CheckDoubleJump;
        playerInput.Player.Fire.performed += Fire;
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
        playerInput.Player.Jump.performed -= CheckDoubleJump;
        playerInput.Player.Fire.performed -= Fire;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthSystem = GetComponent<HealthSystem>();
    }

    // Called every time the Physics engine updates (not tied to framerate, fixed rate)
    private void FixedUpdate()
    {
        if (Time.timeScale > 0)
        {
            if (dashVelocity != Vector2.zero)
            {
                rb.velocity = dashVelocity;
            }
            else if (wallJumpVelocity != Vector2.zero)
            {
                rb.velocity = wallJumpVelocity;
                
            }
            else
            {
                rb.velocity = moveVelocity + new Vector2(0, rb.velocity.y);
            }
            if (rb.velocity.y > 0)
            {
                rb.gravityScale = 3;
            }
            else
            {
                rb.gravityScale = 3;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0)
        {
            // Read player input for movement
            bool crouching = playerInput.Player.Crouch.ReadValue<float>() >= 1f;
            bool walking = !crouching && playerInput.Player.Walk.ReadValue<float>() >= 1f;
            Vector2 moveInput = playerInput.Player.Move.ReadValue<Vector2>();
            moveVelocity = moveInput;
            moveVelocity.y = 0; // Prevent player from becoming a rocket
            if (walking && isWallJumping != true)
            {
                moveVelocity *= walkSpeed;
            }
            else if (crouching)
            {
                moveVelocity *= crouchSpeed;
            }
            else
            {
                moveVelocity *= runSpeed;
            }

            if (wallJumpVelocity == Vector2.zero && isGrounded)
            {
                wallSlideCounter = 2f;
                wallJumpAmount = 3;
                isWallJumping = false;
            }
            // When we're moving
            if (moveVelocity != Vector2.zero)
            {
                // Rotate player in direction of movement
                transform.right = new Vector2(moveVelocity.x, 0);

                // Handle footsteps
                if (isGrounded && !crouching)
                {
                    float stepInterval = walking ? movementAudio.walkStepInterval : movementAudio.runStepInterval;
                    if (footstepTimer >= stepInterval)
                    {
                        movementAudio.PlayFootstep(ground.tag, walking);
                        footstepTimer = 0;
                    }

                    footstepTimer += Time.deltaTime;
                }
            }

            // Check for dash
            if (canDash && playerInput.Player.Dash.ReadValue<float>() >= 1f && moveInput != Vector2.zero)
            {
                Dash(moveInput);
            }

            if (playerInput.Player.Jump.ReadValue<float>() >= 1f)
            {
                Jump();
            }

            wallSlide();
            WallJump();
        }
    }

    void Crouch(InputAction.CallbackContext context)
    {
        if (Time.timeScale > 0)
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
        if (Time.timeScale > 0 && isGrounded && dashVelocity == Vector2.zero)
        {
            if (rb.velocity.y == 0)
                movementAudio.PlayJump(ground.tag);
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }
    }

    void CheckDoubleJump(InputAction.CallbackContext context)
    {
        if (Time.timeScale > 0 && !isGrounded && canDoubleJump && dashVelocity == Vector2.zero)
        {
            movementAudio.PlayDoubleJump();
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
            canDoubleJump = false;
        }
    }

    void Dash(Vector2 moveInput)
    {
        if (Time.timeScale > 0)
        {
            movementAudio.PlayDash();
            dashVelocity = moveInput * dashSpeed;
            StartCoroutine(DashCooldown());
        }
    }

    IEnumerator DashCooldown()
    {
        canDash = false;
        dashIndicator.color = Color.red;
        yield return new WaitForSeconds(dashDuration);
        rb.velocity = Vector2.zero;
        dashVelocity = Vector2.zero;
        yield return new WaitForSeconds(dashCooldown - dashDuration);
        while (!isGrounded)
        {
            yield return new WaitForFixedUpdate();
        }
        dashIndicator.color = Color.green;
        canDash = true;
    }

    void Fire(InputAction.CallbackContext context)
    {
        if (Time.timeScale > 0)
            gun.Fire(rb.velocity);
    }

    //Checks if player touches wall
    private bool isWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    //If player is off ground and is touching a wall then allows for wall sliding
    private void wallSlide()
    {
        if (isWalled() && !isGrounded && wallSlideCounter >= 0)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            wallSlideCounter -= Time.deltaTime;
            isWallJumping = false;
            if (transform.rotation.eulerAngles.y >= 180)
            {
                wallJumpingDirection = 1f;
            }
            else
            {
                wallJumpingDirection = -1f;
            }
            CancelInvoke(nameof(StopWallJumping));
            if (playerInput.Player.Jump.ReadValue<float>() >= 1f && wallSlideCounter >= 0f && isWallSliding == true && wallJumpAmount > 0)
            {
                wallJumpVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
                
                Invoke(nameof(StopWallJumping), wallJumpDuration);
            }
        }
     

    }

    private void StopWallJumping()
    {
        
        wallJumpVelocity = Vector2.zero;
        wallJumpAmount -= 1f;
        isWallJumping = true;
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
        statsUI.PopupUI(deaths, soulsSaved, 0.5f); // Show stats
        dying = false;
        isGrounded = false;
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
        if (!collision.transform.name.Contains("Trap") && Vector2.Angle(collision.GetContact(0).normal, Vector2.up) < 80)
        {
            ground = collision.transform;
            if (!isGrounded)
                movementAudio.PlayLand(ground.tag, Mathf.Abs(collision.relativeVelocity.y) * 0.1f);
            isGrounded = true;
            canDoubleJump = true;
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
        if (ground == collision.transform)
        {
            isGrounded = false;
            ground = null;
        }
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

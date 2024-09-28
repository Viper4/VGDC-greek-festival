using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static PlayerInput playerInput;
    Rigidbody2D rb;

    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float crouchSpeed = 1f;
    [SerializeField] float jumpVelocity = 2f;
    bool isGrounded = false;
    Vector2 moveVelocity;

    bool canDoubleJump = false;

    [SerializeField] float dashSpeed = 10;
    [SerializeField] float dashDuration = 0.25f;
    [SerializeField] float dashCooldown = 0.5f;
    [SerializeField] SpriteRenderer dashIndicator;
    bool canDash = true;
    Vector2 dashVelocity = Vector2.zero;

    [SerializeField] Gun gun;

    [SerializeField] MovementAudio movementAudio;
    Transform ground;
    float footstepTimer = 0;

    public float timePlayed = 0;
    Transform lastCheckpoint;
    public List<Spirit> followingSpirits = new List<Spirit>();
    public int spiritsSaved = 0;
    public int deaths = 0;
    
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
    }

    // Called every time the Physics engine updates (not tied to framerate, fixed rate)
    private void FixedUpdate()
    {
        if(Time.timeScale > 0)
        {
            if (dashVelocity != Vector2.zero)
            {
                rb.velocity = dashVelocity;
            }
            else
            {
                rb.velocity = moveVelocity + new Vector2(0, rb.velocity.y);
            }
            if (rb.velocity.y > 0)
            {
                rb.gravityScale = 1;
            }
            else
            {
                rb.gravityScale = 2;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale > 0)
        {
            // Read player input and set moveVelocity
            bool crouching = playerInput.Player.Crouch.ReadValue<float>() >= 1f;
            bool walking = !crouching && playerInput.Player.Walk.ReadValue<float>() >= 1f;
            Vector2 moveInput = playerInput.Player.Move.ReadValue<Vector2>();
            moveVelocity = moveInput;
            moveVelocity.y = 0; // Prevent player from becoming a rocket
            if (walking)
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

            timePlayed += Time.deltaTime;
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
        dashIndicator.color = Color.red;
        canDash = false;
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
        if(Time.timeScale > 0)
            gun.Fire(rb.velocity);
    }

    void KillPlayer()
    {
        transform.SetPositionAndRotation(lastCheckpoint.position, lastCheckpoint.rotation);
        foreach (Spirit followingSpirit in followingSpirits)
        {
            followingSpirit.ResetSpirit();
        }
        followingSpirits.Clear();
        deaths++;
        statsUI.PopupUI(deaths, spiritsSaved, 0.5f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("DeathZone"))
        {
            KillPlayer();
            return;
        }

        // Recharge isGrounded if the thing we land on isn't a trap and isn't too steep
        if (!collision.transform.name.Contains("Trap"))
        {
            if (Vector2.Angle(collision.GetContact(0).normal, Vector2.up) < 80)
            {
                ground = collision.transform;
                if (!isGrounded)
                    movementAudio.PlayLand(ground.tag, Mathf.Abs(collision.relativeVelocity.y) * 0.1f);
                isGrounded = true;
                canDoubleJump = true;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(ground == collision.transform)
        {
            isGrounded = false;
            ground = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        switch(collider.tag)
        {
            case "Spirit":
                Spirit spirit = collider.GetComponent<Spirit>();
                if (spirit.target == null)
                {
                    if (followingSpirits.Count > 0)
                    {
                        spirit.StartFollow(followingSpirits[^1].transform);
                        followingSpirits.Add(spirit);
                    }
                    else
                    {
                        spirit.StartFollow(transform);
                        followingSpirits.Add(spirit);
                    }
                }
                break;
            case "Checkpoint":
                foreach(Spirit followingSpirit in followingSpirits)
                {
                    spiritsSaved++;
                    followingSpirit.Fade();
                }
                followingSpirits.Clear();
                lastCheckpoint = collider.transform;
                collider.GetComponent<SpriteRenderer>().enabled = false;
                collider.GetComponent<Collider2D>().enabled = false;
                statsUI.PopupUI(deaths, spiritsSaved, 0.5f);
                break;
            case "DeathZone":
                KillPlayer();
                break;
        }
    }
}

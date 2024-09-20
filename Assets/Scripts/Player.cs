using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    PlayerInput playerInput;
    Rigidbody2D rb;

    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float crouchSpeed = 1f;
    [SerializeField] float jumpVelocity = 2f;
    bool isGrounded = false;
    float velocityY = 0;
    Vector2 moveVelocity;

    bool canDoubleJump = false;

    [SerializeField] float dashSpeed = 10;
    [SerializeField] float dashDuration = 0.25f;
    [SerializeField] float dashCooldown = 0.5f;
    [SerializeField] float dashTapThreshold = 0.15f;
    float dashTapTime = 1f;
    bool canDash = true;
    Vector2 lastDashDirection = Vector2.zero;
    Vector2 dashVelocity = Vector2.zero;

    [SerializeField] Gun gun;

    private void Awake()
    {
        playerInput = new PlayerInput();
        dashVelocity = Vector2.zero;
    }

    private void OnEnable()
    {
        foreach (InputAction action in playerInput)
        {
            action.Enable();
        }

        playerInput.Player.Crouch.performed += Crouch;
        playerInput.Player.Jump.performed += CheckDoubleJump;
        playerInput.Player.Fire.performed += Fire;
        playerInput.Player.Move.performed += CheckDash;
    }

    private void OnDisable()
    {
        foreach (InputAction action in playerInput)
        {
            action.Disable();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Called every time the Physics engine updates (not tied to framerate, fixed rate)
    private void FixedUpdate()
    {
        if(dashVelocity != Vector2.zero)
        {
            rb.velocity = dashVelocity;
        }
        else
        {
            rb.velocity = moveVelocity + new Vector2(0, rb.velocity.y);
        }
        velocityY = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Read player input and set moveVelocity. velocityY set separately so we dont multiply velocityY by run/crouch/walk speed
        bool running = playerInput.Player.Run.ReadValue<float>() >= 1f;
        bool crouching = playerInput.Player.Crouch.ReadValue<float>() >= 1f;
        if (playerInput.Player.Crouch.WasReleasedThisFrame())
            Uncrouch();
        moveVelocity = playerInput.Player.Move.ReadValue<Vector2>();
        moveVelocity.y = 0; // Prevent player from becoming a rocket
        if (running)
        {
            moveVelocity *= runSpeed;
        }
        else if (crouching)
        {
            moveVelocity *= crouchSpeed;
        }
        else
        {
            moveVelocity *= walkSpeed;
        }

        if(playerInput.Player.Jump.ReadValue<float>() >= 1f)
        {
            Jump();
        }

        // Rotate player in direction of movement
        if (moveVelocity != Vector2.zero)
        {
            transform.right = new Vector2(moveVelocity.x, 0);
        }
    }

    void Crouch(InputAction.CallbackContext context)
    {
        transform.localScale = new Vector3(1, 0.5f, 1);
        transform.position -= new Vector3(0, 0.5f);
    }

    void Uncrouch()
    {
        transform.localScale = new Vector3(1, 1, 1);
        transform.position += new Vector3(0, 0.5f);
    }

    void Jump()
    {
        if (velocityY <= 0 && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }
    }

    void CheckDoubleJump(InputAction.CallbackContext context)
    {
        if (!isGrounded && canDoubleJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
            canDoubleJump = false;
        }
    }

    void CheckDash(InputAction.CallbackContext context)
    {
        if (canDash)
        {
            Vector2 dashDirection = playerInput.Player.Move.ReadValue<Vector2>();
            if (Time.unscaledTime - dashTapTime < dashTapThreshold && (lastDashDirection - dashDirection).sqrMagnitude < 0.75f)
            {
                dashVelocity = dashDirection * dashSpeed;
                canDash = false;
                StartCoroutine(DashCooldown());
            }
            dashTapTime = Time.unscaledTime;
            lastDashDirection = dashDirection;
        }
    }

    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashDuration);
        rb.velocity = Vector2.zero;
        dashVelocity = Vector2.zero;
        lastDashDirection = Vector2.zero;
        yield return new WaitForSeconds(dashCooldown - dashDuration);
        while(!isGrounded)
        {
            yield return new WaitForFixedUpdate();
        }
        canDash = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(Vector2.Angle(collision.GetContact(0).normal, Vector2.up) < 80)
        {
            isGrounded = true;
            canDoubleJump = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    void Fire(InputAction.CallbackContext context)
    {
        gun.Fire(rb.velocity);
    }
}

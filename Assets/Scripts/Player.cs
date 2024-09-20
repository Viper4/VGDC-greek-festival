using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    PlayerInput playerInput;
    Rigidbody2D rb;
    Collider2D _collider;

    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float crouchSpeed = 1f;
    [SerializeField] float jumpVelocity = 2f;
    float velocityY = 0;
    Vector2 moveVelocity;

    [SerializeField] LayerMask groundLayers;

    [SerializeField] Gun gun;

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        foreach (InputAction action in playerInput)
        {
            action.Enable();
        }

        playerInput.Player.Fire.performed += Fire;
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
        _collider = GetComponent<Collider2D>();
    }

    // Called every time the Physics engine updates (not tied to framerate, fixed rate)
    private void FixedUpdate()
    {
        rb.velocity = moveVelocity + new Vector2(0, rb.velocity.y + velocityY);
        velocityY = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Read player input and set moveVelocity. velocityY set separately so we dont multiply velocityY by run/crouch/walk speed
        bool running = playerInput.Player.Run.ReadValue<float>() >= 1f;
        bool crouching = playerInput.Player.Crouch.ReadValue<float>() >= 1f;
        moveVelocity = playerInput.Player.Move.ReadValue<Vector2>();
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

    // Cast a ray downward 0.01m below player's bounding box to check for ground
    bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, _collider.bounds.extents.y + 0.01f, groundLayers);
    }

    void Jump()
    {
        if (velocityY <= 0 && IsGrounded())
        {
            velocityY += jumpVelocity;
        }
    }

    void Fire(InputAction.CallbackContext context)
    {
        gun.Fire(rb.velocity);
    }
}

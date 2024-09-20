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

    [SerializeField] LayerMask floorLayers;

    private void Awake()
    {
        playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        playerInput.Player.Move.Enable();
        playerInput.Player.Run.Enable();
        playerInput.Player.Crouch.Enable();
        playerInput.Player.Jump.Enable();

        playerInput.Player.Jump.performed += Jump;
    }

    private void OnDisable()
    {
        playerInput.Player.Move.Disable();
        playerInput.Player.Run.Disable();
        playerInput.Player.Crouch.Disable();
        playerInput.Player.Jump.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (IsGrounded())
        {
            if(velocityY > 0)
            {
                velocityY += Physics2D.gravity.y;
            }
            else
            {
                velocityY = 0;
            }
        }
        else
        {
            velocityY += Physics2D.gravity.y;
        }

        rb.velocity = moveVelocity + new Vector2(0, velocityY);
    }

    // Update is called once per frame
    void Update()
    {
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
    }

    bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, _collider.bounds.extents.y + 0.01f, floorLayers);
    }

    void Jump(InputAction.CallbackContext context)
    {
        Debug.Log("Jump");
        velocityY += jumpVelocity;
    }
}

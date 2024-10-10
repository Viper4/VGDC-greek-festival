using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseMovement : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Collider2D myCollider;
    [HideInInspector] public Stairs stairs;

    float gravityScale = 1f;
    public float runSpeed = 5f;
    public float walkSpeed = 2f;
    public float crouchSpeed = 1f;
    public float jumpVelocity = 2f;
    public bool Walking { get; set; }
    public bool Crouching { get; set; }

    [HideInInspector] public Transform ground;
    public bool IsGrounded { get; set; }

    public MovementAudio movementAudio;
    float footstepTimer = 0;

    public float climbSpeed = 2f;
    bool climbing = false;
    public bool Climbing 
    {
        get
        {
            return climbing;
        }
        set
        {
            climbing = value;
            if(value)
                rb.gravityScale = 0f;
            else
                rb.gravityScale = gravityScale;
        } 
    }

    public virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        gravityScale = rb.gravityScale;
    }

    public virtual void Update()
    {
        if (Time.timeScale > 0)
        {
            // When we're moving
            if (rb.velocity.x != 0)
            {
                // Rotate in direction of movement
                transform.right = new Vector2(rb.velocity.x, 0);

                // Handle footsteps
                if (movementAudio != null && IsGrounded && !Crouching)
                {
                    float stepInterval = Walking ? movementAudio.walkStepInterval : movementAudio.runStepInterval;
                    if (footstepTimer >= stepInterval)
                    {
                        movementAudio.PlayFootstep(ground.tag, Walking);
                        footstepTimer = 0;
                    }

                    footstepTimer += Time.deltaTime;
                }
            }
        }
    }

    public void GoDown()
    {
        if(stairs != null)
        {
            stairs.Descend(myCollider);
            stairs = null;
        }
    }

    public void Land(Collision2D collision)
    {
        ground = collision.transform;
        if (movementAudio != null && !IsGrounded)
            movementAudio.PlayLand(ground.tag, Mathf.Abs(collision.relativeVelocity.y) * 0.1f);
        IsGrounded = true;
    }

    public void ExitGround()
    {
        IsGrounded = false;
        ground = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.transform.name.Contains("Trap"))
        {
            if (Vector2.Angle(collision.GetContact(0).normal, Vector2.up) < 80)
            {
                Land(collision);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (ground == collision.transform)
        {
            ExitGround();
        }
    }
}

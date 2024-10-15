using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UI;

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
    [HideInInspector] public Vector2 moveVelocity;

    Transform ground;
    bool isGrounded = false;
    public bool IsGrounded 
    { 
        get
        { 
            return isGrounded;
        } 
        set
        {
            if (!value)
                ground = null;
            isGrounded = value;
        } 
    }

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

    public MovementAudio movementAudio;
    float footstepTimer = 0;

    [HideInInspector] public Vector2 knockbackVelocity;

    [HideInInspector] public Transform wall;

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
            if (Mathf.Abs(moveVelocity.x) > 0.1f)
            {
                // Rotate in direction of movement
                transform.right = new Vector2(moveVelocity.x, 0);

                // Handle footsteps
                if (movementAudio != null && isGrounded && !Crouching)
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

    public void PlayJumpSound()
    {
        movementAudio.PlayJump(ground.tag);
    }

    public void GoDown()
    {
        if(stairs != null)
        {
            stairs.Descend(myCollider);
            stairs = null;
        }
    }

    public bool TryLand(Collision2D collision)
    {
        if (!collision.transform.CompareTag("Trap"))
        {
            if (Vector2.Angle(collision.GetContact(0).normal, Vector2.up) < 80)
            {
                ground = collision.transform;
                if (movementAudio != null && !isGrounded)
                    movementAudio.PlayLand(ground.tag, Mathf.Abs(collision.relativeVelocity.y) * 0.1f);
                isGrounded = true;
                return true;
            }
        }
        return false;
    }

    public void CheckWall(Collision2D collision)
    {
        if (!collision.transform.CompareTag("Trap"))
        {
            if (Vector2.Angle(collision.GetContact(0).normal, Vector2.up) > 80)
            {
                wall = collision.transform;
            }
        }
    }

    public void ExitCollision(Collision2D collision)
    {
        if(collision.transform == ground)
        {
            isGrounded = false;
            ground = null;
        }
        else if(collision.transform == wall)
        {
            wall = null;
        }
    }

    public IEnumerator ApplyKnockback(Vector2 velocity, float duration, float drag)
    {
        knockbackVelocity = velocity;
        float timer = duration;
        while (timer > 0)
        {
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, Time.deltaTime * drag);
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        knockbackVelocity = Vector2.zero;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryLand(collision);
        CheckWall(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        ExitCollision(collision);
    }
}

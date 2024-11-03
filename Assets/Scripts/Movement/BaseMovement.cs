using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseMovement : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Collider2D _collider;
    [HideInInspector] public Stairs stairs;

    private float gravityScale = 1f;
    [Header("BaseMovement")]
    public float runSpeed = 5f;
    public float walkSpeed = 2f;
    public float crouchSpeed = 1f;
    public float jumpSpeed = 2f;
    public bool Walking { get; set; }
    public bool Crouching { get; set; }
    [HideInInspector] public Vector2 moveVelocity;
    public bool rotateWithMovement = true;
    [SerializeField, Tooltip("Seconds in the future to predict for falling, obstructions, etc.")] private float predictionTime = 0.5f;

    [SerializeField] private string[] nonGroundTags = new string[] { "Trap", "Enemy", "Projectile", "Soul" };
    private HashSet<string> nonGroundTagsSet = new HashSet<string>();
    private Transform ground;
    private bool isGrounded = false;
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
    [SerializeField] float fallCheckDistance = 3f;

    public float climbSpeed = 2f;
    private bool climbing = false;
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
    private float footstepTimer = 0;

    [System.Serializable]
    public struct KnockbackInfo
    {
        public Vector2 velocity;
        public float duration;
        public float drag;

        public KnockbackInfo(Vector2 velocity, float duration, float drag)
        {
            this.velocity = velocity;
            this.duration = duration;
            this.drag = drag;
        }
    }
    [HideInInspector] public Vector2 knockbackVelocity;
    private Coroutine knockbackRoutine;
    public float immunityTime = 0.3f;
    private float immunityTimer = 0;

    [SerializeField] private string[] nonWallTags = new string[] { "Trap", "Enemy", "Projectile", "Soul" };
    private HashSet<string> nonWallTagsSet = new HashSet<string>();
    [HideInInspector] public Transform wall;

    public LayerMask collisionLayers;

    public virtual void Start()
    {
        foreach(string tag in nonGroundTags)
        {
            if (!nonGroundTagsSet.Contains(tag))
            {
                nonGroundTagsSet.Add(tag);
            }
        }
        foreach(string tag in nonWallTags)
        {
            if (!nonWallTagsSet.Contains(tag))
            {
                nonWallTagsSet.Add(tag);
            }
        }
        rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        gravityScale = rb.gravityScale;
    }

    public void MovementUpdate()
    {
        if (Time.timeScale > 0)
        {
            // When we're moving
            if (Mathf.Abs(moveVelocity.x) > 0.1f)
            {
                // Rotate in direction of movement
                if(rotateWithMovement)
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
            immunityTimer += Time.deltaTime;
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
            stairs.Descend(_collider);
            stairs = null;
        }
    }

    public bool TryLand(Collision2D collision, float angle)
    {
        if (!nonGroundTagsSet.Contains(collision.transform.tag))
        {
            if (angle < 80)
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

    public void CheckWall(Collision2D collision, float angle)
    {
        if (!nonWallTagsSet.Contains(collision.transform.tag))
        {
            if (angle > 80 && angle < 120)
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

    public void ApplyKnockback(KnockbackInfo knockback, bool overrideImmunity)
    {
        if(overrideImmunity || immunityTimer > immunityTime)
        {
            if (knockbackRoutine != null)
                StopCoroutine(knockbackRoutine);
            knockbackRoutine = StartCoroutine(Knockback(knockback));
            if(!overrideImmunity)
                immunityTime = 0;
        }
    }

    private IEnumerator Knockback(KnockbackInfo knockback)
    {
        knockbackVelocity = knockback.velocity;
        float timer = knockback.duration;
        while (timer > 0)
        {
            knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, Time.deltaTime * knockback.drag);
            timer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        knockbackVelocity = Vector2.zero;
    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        float normalAngle = Vector2.Angle(collision.GetContact(0).normal, Vector2.up);
        TryLand(collision, normalAngle);
        CheckWall(collision, normalAngle);
    }

    public virtual void OnCollisionExit2D(Collision2D collision)
    {
        ExitCollision(collision);
    }

    public RaycastHit2D GetFutureObstruction(Vector3 velocity)
    {
        Vector2 futurePosition = transform.position + (velocity * predictionTime);
        return Physics2D.Linecast(transform.position, futurePosition, collisionLayers);
    }

    public RaycastHit2D GetContactObstruction(Vector2 direction)
    {
        Vector2 futurePosition = _collider.bounds.center + new Vector3((_collider.bounds.extents.x + 0.1f) * direction.x, (_collider.bounds.extents.y + 0.1f) * direction.y);
        return Physics2D.Linecast(transform.position, futurePosition, collisionLayers);
    }

    public void ApplyVelocity(Vector2 newVelocity)
    {
        RaycastHit2D obstruction = GetContactObstruction(newVelocity.normalized);
        if (obstruction.transform != null && obstruction.transform.TryGetComponent(out Pushable pushable))
        {
            if (pushable.immovable)
            {
                newVelocity = Vector2.zero;
            }
            else
            {
                Vector2 resistancePercentage = new Vector2(Mathf.Clamp01(pushable.resistance / Mathf.Abs(newVelocity.x)), Mathf.Clamp01(pushable.resistance / Mathf.Abs(newVelocity.y)));
                newVelocity.x *= 1 - resistancePercentage.x;
                newVelocity.y *= 1 - resistancePercentage.y;
            }
        }
        rb.velocity = newVelocity;
    }

    public RaycastHit2D CheckFall(Vector3 velocity)
    {
        Vector2 futurePosition = transform.position + (velocity * predictionTime);
        RaycastHit2D groundHit = Physics2D.Linecast(futurePosition, futurePosition + new Vector2(0, -fallCheckDistance), collisionLayers);
        Debug.DrawLine(futurePosition, futurePosition + new Vector2(0, -fallCheckDistance), Color.green, 1f);
        return groundHit;
    }
}

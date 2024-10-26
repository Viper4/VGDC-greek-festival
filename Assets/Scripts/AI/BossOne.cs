using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BossOne : Enemy
{
    enum BossState{
        Slash, Jump, Charge, Idle
    }

    BossState currentState = BossState.Idle;
    BossState lastState;
    [SerializeField, Header("Boss One")] float minWalk = 0.5f;
    [SerializeField] float maxWalk = 2f;
    [SerializeField] float idleTime = 2.5f;
    [SerializeField] float slashIdleTime = 1.0f;
    private float originalIdleTime;

    [SerializeField, Tooltip("Order of enum is Slash, Jump, Charge, Idle")] float[] attackReactionTimes;

    [SerializeField] float arcTime = 3.0f;

    [SerializeField] KnockbackInfo defaultKnockback;
    [SerializeField] KnockbackInfo jumpKnockback;
    [SerializeField] KnockbackInfo chargeKnockback;

    [SerializeField] float chargeSpeed = 6f;

    [SerializeField] float chargeAccelerationTime = .8f;

    [SerializeField] GameObject shield;
    [SerializeField] Animator animator;

    public override void Start()
    {
        base.Start();
        originalIdleTime = idleTime;        
        StartCoroutine(StartIdle());
    }

    private void FixedUpdate()
    {
        if (_collider.bounds.Intersects(Player.instance._collider.bounds))
        {
            KnockbackPlayer();
        }
    }

    IEnumerator StartIdle(){
        yield return new WaitForEndOfFrame();
        Physics2D.IgnoreCollision(Player.instance._collider, _collider, false);
        shield.GetComponent<Renderer>().material.color = Color.white;
        RotateTowardsPlayer();
        currentState = GetRandomEnum<BossState>();
        // Keep picking random attack until we get one that isn't the same as the last and isn't idle unless it's slash
        while((currentState == lastState || currentState == BossState.Idle) && currentState != BossState.Slash)
        {
            currentState = GetRandomEnum<BossState>();
        }
        Vector2 randomDirection = Random.Range(0, 2) == 0 ? Vector2.right : Vector2.left;
        moveVelocity = randomDirection * walkSpeed;
        float idleTimer = 0;
        float walkTime = Random.Range(Mathf.Min(minWalk, idleTime), Mathf.Max(maxWalk, idleTime));
        while(idleTimer < idleTime)
        {
            if(idleTimer < walkTime)
            {
                rb.velocity = new Vector2(moveVelocity.x, rb.velocity.y);
            }
            if (idleTimer + attackReactionTimes[(int)currentState] > idleTime)
            {
                switch (currentState)
                {
                    case BossState.Slash:
                        shield.GetComponent<Renderer>().material.color = Color.red;
                        break;
                    case BossState.Jump:
                        shield.GetComponent<Renderer>().material.color = Color.yellow;
                        break;
                    case BossState.Charge:
                        shield.GetComponent<Renderer>().material.color = Color.black;
                        break;
                }
            }
            idleTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        // Start attack
        switch(currentState)
        {
            case BossState.Slash:
                idleTime = slashIdleTime;
                lastState = BossState.Slash;
                Debug.Log("Slash");
                StartCoroutine(Slash());
                break;
            case BossState.Jump:
                idleTime = originalIdleTime;
                lastState = BossState.Jump;
                Debug.Log("Jump");
                StartCoroutine(Jump());
                break;
            case BossState.Charge:
                idleTime = originalIdleTime;
                lastState = BossState.Charge;
                Debug.Log("Charge");
                StartCoroutine(Charge());
                break;
            }
    }

    void RotateTowardsPlayer(){
        Vector2 PlayerDir = Player.instance.transform.position - transform.position;
        PlayerDir.y = 0;
        transform.right = PlayerDir.normalized;
    }

    public void KnockbackPlayer()
    {
        Vector2 direction = Player.instance.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();

        KnockbackInfo knockback;
        switch (currentState)
        {
            case BossState.Jump:
                knockback = jumpKnockback;
                knockback.velocity = new Vector2(knockback.velocity.x * direction.x, knockback.velocity.y);
                break;
            case BossState.Charge:
                knockback = chargeKnockback;
                knockback.velocity = new Vector2(direction.x * (knockback.velocity.x + moveVelocity.x), knockback.velocity.y);
                break;
            default:
                knockback = defaultKnockback;
                knockback.velocity = new Vector2(knockback.velocity.x * direction.x, knockback.velocity.y);
                break;
        }
        Player.instance.ApplyKnockback(knockback, false);
    }

    IEnumerator Slash()
    {
        animator.SetTrigger("Slash");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        StartCoroutine(StartIdle());
    }

    IEnumerator Charge()
    {
        RotateTowardsPlayer();
        moveVelocity = transform.right * chargeSpeed;
        float chargeTimer = 0;
        while (chargeTimer < chargeAccelerationTime)
        {
            rb.velocity = new Vector2(Mathf.Lerp(0, moveVelocity.x, chargeTimer / chargeAccelerationTime), 0);
            chargeTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        while (wall == null)
        {
            rb.velocity = moveVelocity;
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(chargeKnockback.duration);
        StartCoroutine(StartIdle());
    }

    IEnumerator Jump()
    {
        Physics2D.IgnoreCollision(Player.instance._collider, _collider, true);
        float distanceX = Player.instance.transform.position.x - transform.position.x;
        float distanceY = Player.instance.transform.position.y - transform.position.y;
        float velocityX = distanceX / arcTime;
        float velocityY = distanceY / arcTime + 0.5f * rb.gravityScale * -Physics2D.gravity.y * arcTime;
        rb.velocity = new Vector2(velocityX, velocityY);
        yield return new WaitForSeconds(arcTime / 2);
        animator.SetTrigger("Slash");
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength);
        yield return new WaitUntil(()=> IsGrounded);
        yield return new WaitForSeconds(jumpKnockback.duration);
        StartCoroutine(StartIdle());
    }

    public static T GetRandomEnum<T>()
    {
        System.Array A = System.Enum.GetValues(typeof(T));
        T V = (T)A.GetValue(Random.Range(0, A.Length));
        return V;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            KnockbackPlayer();
        }
    }
}
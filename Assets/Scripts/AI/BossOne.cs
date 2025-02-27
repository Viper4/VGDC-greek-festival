using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BossOne : BaseMovement, ISaveable
{
    private enum BossState
    {
        Slash, Jump, Charge, Idle
    }

    private BossState currentState = BossState.Idle;
    private BossState lastState;
    [SerializeField, Header("Boss One")] private float minWalk = 0.5f;
    [SerializeField] private float maxWalk = 2f;
    [SerializeField] private float idleTime = 2.5f;
    [SerializeField] private float slashIdleTime = 1.0f;
    private float originalIdleTime;

    [SerializeField, Tooltip("Order of enum is Slash, Jump, Charge, Idle")] private float[] attackReactionTimes;

    [SerializeField] private float arcTime = 3.0f;

    [SerializeField] private KnockbackInfo defaultKnockback;
    [SerializeField] private float defaultBodyDamage = 15f;
    [SerializeField] private KnockbackInfo jumpKnockback;
    [SerializeField] private float jumpDamage = 25f;
    [SerializeField] private KnockbackInfo chargeKnockback;
    [SerializeField] private float chargeDamage = 25f;

    [SerializeField] private float chargeSpeed = 6f;

    [SerializeField] private float chargeAccelerationTime = .8f;

    [SerializeField] private GameObject shield;
    [SerializeField] private Animator animator;
    private AudioSource audioSource;
    private HealthTrigger healthTrigger;
    private HealthSystem healthSystem;

    private Vector3 spawnPosition;

    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip chargeImpactSound;
    [SerializeField] private float shakeMagnitude = 0.4f;
    [SerializeField] private float shakeSpeed = 12f;
    [SerializeField] private float shakeDuration = 0.6f;

    private void GetComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        healthTrigger = GetComponent<HealthTrigger>();
        healthSystem = GetComponent<HealthSystem>();
    }

    public override void Start()
    {
        base.Start();
        GetComponents();
        originalIdleTime = idleTime;
        spawnPosition = transform.position;
        onLand.AddListener(Land);
    }

    private void FixedUpdate()
    {
        if (_collider.bounds.Intersects(Player.instance._collider.bounds))
        {
            KnockbackPlayer();
        }
    }

    private void Update()
    {
        MovementUpdate();
    }

    public void StartBoss()
    {
        StartCoroutine(StartBossDelay());
    }

    private IEnumerator StartBossDelay()
    {
        // When the boss gameObject is set active Start() doesn't get called so we need to wait
        yield return new WaitForEndOfFrame();
        healthSystem.ResetHealth();
        StartCoroutine(StartIdle());
    }

    IEnumerator StartIdle(){
        yield return new WaitForEndOfFrame();
        healthTrigger.healthAmount = -defaultBodyDamage;
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
        Walking = true;
        moveVelocity = randomDirection * walkSpeed;
        float idleTimer = 0;
        float walkTime = Random.Range(Mathf.Min(minWalk, idleTime), Mathf.Max(maxWalk, idleTime));
        while(idleTimer < idleTime)
        {
            if (idleTimer < walkTime)
            {
                moveVelocity.y = rb.velocity.y;
                ApplyVelocity(moveVelocity);
            }
            if (idleTimer + attackReactionTimes[(int)currentState] > idleTime)
            {
                moveVelocity = Vector2.zero;
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
                StartCoroutine(Slash());
                break;
            case BossState.Jump:
                idleTime = originalIdleTime;
                lastState = BossState.Jump;
                StartCoroutine(Jump());
                break;
            case BossState.Charge:
                idleTime = originalIdleTime;
                lastState = BossState.Charge;
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
        Walking = false;
        healthTrigger.healthAmount = -chargeDamage;
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
        healthTrigger.healthAmount = -jumpDamage;
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

    public void ResetBoss()
    {
        if (gameObject.activeSelf)
        {
            transform.position = spawnPosition;
            gameObject.SetActive(false);
        }
    }

    private void Land()
    {
        if (currentState == BossState.Charge)
        {
            audioSource.PlayOneShot(chargeImpactSound);
        }
        else
        {
            audioSource.PlayOneShot(landSound);
        }
        Camera.main.GetComponent<CameraControl>().StartCameraShake(shakeMagnitude, shakeSpeed, shakeDuration);
    }

    public void PlayAudioClip(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }

    public override object CaptureState()
    {
        if (rb == null)
        {
            GetComponents();
        }
        float[] position = { transform.position.x, transform.position.y, transform.position.z };
        float[] eulerAngles = { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z };
        float[] velocity = new float[] { rb.velocity.x, rb.velocity.y };
        float health = healthSystem.health;
        return new object[] { position, eulerAngles, velocity, health };
    }

    public override void RestoreState(object state)
    {
        if (rb == null)
        {
            GetComponents();
        }
        object[] data = (object[])state;
        float[] position = (float[])data[0];
        float[] eulerAngles = (float[])data[1];
        float[] velocity = (float[])data[2];
        healthSystem.health = (float)data[3];
        healthSystem.HealthUpdate();
        transform.position = new Vector3(position[0], position[1], position[2]);
        transform.eulerAngles = new Vector3(eulerAngles[0], eulerAngles[1], eulerAngles[2]);
        rb.velocity = new Vector2(velocity[0], velocity[1]);
    }

    public override void Delete()
    {
        if (healthSystem == null)
        {
            GetComponents();
        }
        healthSystem.health = 0;
        healthSystem.HealthUpdate();
        Destroy(gameObject);
    }
}
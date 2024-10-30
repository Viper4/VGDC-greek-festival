using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealthTrigger : Trigger
{
    AudioSource audioSource;

    public float healthAmount = 1;
    [SerializeField] bool applyKnockback = false;
    public BaseMovement.KnockbackInfo knockback;
    [SerializeField] float maxHealthAmount = 0;
    [SerializeField] int uses = 1;
    [SerializeField] float healthSystemCooldown = 0f;
    [SerializeField] float cooldown = 0.5f;
    [SerializeField] bool overrideHealthSystemCooldown = false;
    HealthSystem triggerHealthSystem;
    [HideInInspector] public bool player = false;

    bool canUse = true;

    private void Start()
    {
        InitializeHashSet();
        TryGetComponent(out audioSource);
    }

    IEnumerator UseCooldown()
    {
        canUse = false;
        yield return new WaitForSeconds(cooldown);
        canUse = true;
    }

    void Use()
    {
        if (canUse)
        {
            if (applyKnockback && triggerHealthSystem.TryGetComponent(out BaseMovement baseMovement))
            {
                BaseMovement.KnockbackInfo knockbackInfo = knockback;
                knockbackInfo.velocity = transform.right * knockbackInfo.velocity.x + transform.up * knockbackInfo.velocity.y;
                baseMovement.ApplyKnockback(knockbackInfo, true);
            }
            bool changedMaxHealth = triggerHealthSystem.AddMaxHealth(maxHealthAmount, healthSystemCooldown, overrideHealthSystemCooldown);
            bool changedHealth = triggerHealthSystem.AddHealth(healthAmount, healthSystemCooldown, overrideHealthSystemCooldown);
            if (changedHealth || changedMaxHealth)
            {
                if (player)
                    Player.instance.CheckKill(triggerHealthSystem);
                if (audioSource != null)
                    audioSource.Play();
                uses--;
                if (uses == 0)
                    Destroy(gameObject);
            }
            StartCoroutine(UseCooldown());
        }
    }

    private void FixedUpdate()
    {
        // If the target is still triggering this, keep using the health trigger
        if (triggerHealthSystem != null && cooldown > 0)
        {
            Use();
        }
    }

    public override void TriggerEnter(Collider2D collider)
    {
        base.TriggerEnter(collider);
        if (collider.TryGetComponent(out triggerHealthSystem))
        {
            Use();
        }
    }

    public override void TriggerExit(Collider2D collider)
    {
        base.TriggerExit(collider);
        if (collider.TryGetComponent(out HealthSystem exitingHealthSystem) && exitingHealthSystem == triggerHealthSystem)
        {
            triggerHealthSystem = null;
        }
    }

    public override void CollisionEnter(Collision2D collision)
    {
        base.CollisionEnter(collision);
        if (collision.transform.TryGetComponent(out triggerHealthSystem))
        {
            Use();
        }
    }

    public override void CollisionExit(Collision2D collision)
    {
        base.CollisionExit(collision);
        if(collision.transform.TryGetComponent(out HealthSystem exitingHealthSystem) && exitingHealthSystem == triggerHealthSystem)
        {
            triggerHealthSystem = null;
        }
    }
}

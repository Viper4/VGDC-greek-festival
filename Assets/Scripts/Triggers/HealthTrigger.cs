using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealthTrigger : Trigger
{
    AudioSource audioSource;

    [SerializeField] float healthAmount = 1;
    [SerializeField] float maxHealthAmount = 0;
    [SerializeField] int uses = 1;
    [SerializeField] float healthSystemCooldown = 0f;
    [SerializeField] float cooldown = 0.5f;
    [SerializeField] bool overrideHealthSystemCooldown = false;
    HealthSystem triggerHealthSystem;

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
            bool changedMaxHealth = triggerHealthSystem.AddMaxHealth(maxHealthAmount, healthSystemCooldown, overrideHealthSystemCooldown);
            bool changedHealth = triggerHealthSystem.AddHealth(healthAmount, healthSystemCooldown, overrideHealthSystemCooldown);
            if (changedHealth || changedMaxHealth)
            {
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

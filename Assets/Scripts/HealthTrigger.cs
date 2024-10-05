using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealthTrigger : Trigger
{
    AudioSource audioSource;

    [SerializeField] float healthAmount = 1;
    [SerializeField] float maxHealthAmount = 0;
    [SerializeField] int uses = 1;
    [SerializeField] float cooldown = 0.5f;
    [SerializeField] bool overrideCooldown = false;
    HealthSystem triggerHealthSystem;

    private void Start()
    {
        InitializeHashSet();
        TryGetComponent(out audioSource);
    }

    void Use()
    {
        if (!triggerHealthSystem.gameObject.activeSelf)
            return;
        bool changedMaxHealth = triggerHealthSystem.AddMaxHealth(maxHealthAmount, cooldown, overrideCooldown);
        bool changedHealth = triggerHealthSystem.AddHealth(healthAmount, cooldown, overrideCooldown);
        if (changedHealth || changedMaxHealth)
        {
            if (audioSource != null)
                audioSource.Play();
            uses--;
            if (uses == 0)
                Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        // If the player is still triggering this, keep using the health trigger
        if (triggerHealthSystem != null)
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

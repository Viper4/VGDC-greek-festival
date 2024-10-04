using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthTrigger : MonoBehaviour
{
    [SerializeField] float healthAmount = 1;
    [SerializeField] float maxHealthAmount = 0;
    [SerializeField] int uses = 1;
    [SerializeField] float cooldown = 0.5f;
    [SerializeField] bool overrideCooldown = false;
    HealthSystem playerHealth;

    void Use()
    {
        bool changedMaxHealth = playerHealth.AddMaxHealth(maxHealthAmount, cooldown, overrideCooldown);
        bool changedHealth = playerHealth.AddHealth(healthAmount, cooldown, overrideCooldown);
        if (changedHealth || changedMaxHealth)
        {
            uses--;
            if (uses == 0)
                Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        // If the player is still triggering this, keep using the health trigger
        if (playerHealth != null)
        {
            Use();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerHealth = collision.GetComponent<HealthSystem>();
            Use();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            playerHealth = collision.transform.GetComponent<HealthSystem>();
            Use();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        playerHealth = null;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        playerHealth = null;
    }
}

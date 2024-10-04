using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    float originalMaxHealth;
    [SerializeField] float maxHealth = 10;
    [SerializeField] float health;
    [SerializeField] Transform healthBar;
    [SerializeField] Vector3 healthBarScale;
    [SerializeField] UnityEvent<float> onHealthUpdate;
    [SerializeField] UnityEvent<float> onMaxHealthUpdate;
    [SerializeField] UnityEvent onDeath;
    bool canUpdateHealth = true;

    IEnumerator Start()
    {
        originalMaxHealth = maxHealth;
        yield return new WaitForEndOfFrame(); // Wait until UI elements are ready
        ResetHealth(false);  // In case we want to start not at max health
    }

    IEnumerator ResetHealthRoutine(bool updateHealth)
    {
        maxHealth = originalMaxHealth;
        onMaxHealthUpdate?.Invoke(maxHealth);
        if (updateHealth)
            health = maxHealth;
        yield return new WaitForEndOfFrame(); // Wait for content size fitter
        onHealthUpdate?.Invoke(1);
        if (healthBar != null)
            healthBar.localScale = healthBarScale;
    }

    public void ResetHealth(bool updateHealth = true)
    {
        StartCoroutine(ResetHealthRoutine(updateHealth));
    }

    IEnumerator HealthCooldown(float time)
    {
        canUpdateHealth = false;
        yield return new WaitForSeconds(time);
        canUpdateHealth = true;
    }

    public bool AddHealth(float amount, float cooldown, bool overrideCooldown = false)
    {
        if (amount == 0)
            return false;
        if(canUpdateHealth || overrideCooldown)
        {
            StartCoroutine(HealthCooldown(cooldown));
            health += amount;
            if (health <= 0)
            {
                health = 0;
                onDeath?.Invoke();
            }
            else if (health > maxHealth)
            {
                health = maxHealth;
            }
            float healthPercent = health / maxHealth;
            onHealthUpdate?.Invoke(healthPercent);
            if (healthBar != null)
                healthBar.localScale = new Vector3(healthPercent * healthBarScale.x, healthBarScale.y, healthBarScale.z);
            return true;
        }
        return false;
    }

    public bool AddMaxHealth(float amount, float cooldown, bool overrideCooldown = false)
    {
        if (amount == 0)
            return false;
        if (canUpdateHealth || overrideCooldown)
        {
            StartCoroutine(HealthCooldown(cooldown));
            maxHealth += amount;
            if (maxHealth < 0)
            {
                maxHealth = 0;
                onDeath?.Invoke();
            }
            if (health > maxHealth)
                health = maxHealth;
            float healthPercent = health / maxHealth;
            onMaxHealthUpdate?.Invoke(maxHealth);
            onHealthUpdate?.Invoke(healthPercent);
            if (healthBar != null)
                healthBar.localScale = new Vector3(healthPercent * healthBarScale.x, healthBarScale.y, healthBarScale.z);
            return true;
        }
        return false;
    }
}

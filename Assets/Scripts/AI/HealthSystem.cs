using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    float originalMaxHealth;
    [SerializeField] bool invincible = false;
    public float maxHealth = 10;
    public float health;
    [SerializeField] SpriteRenderer[] healthBarSprites;
    [SerializeField] float healthBarFadeSpeed = 1f;
    [SerializeField] bool healthBarAlwaysVisible = false;
    [SerializeField] Vector3 healthBarScale;
    public bool PlayerNearby { get; set; }
    Coroutine fadeCoroutine;

    [SerializeField] UnityEvent<float> onHealthUpdate;
    [SerializeField] UnityEvent<float> onMaxHealthUpdate;
    [SerializeField] UnityEvent onDeath;
    bool canUpdateHealth = true;
    bool immune = false;

    IEnumerator Start()
    {
        originalMaxHealth = maxHealth;
        yield return new WaitForEndOfFrame(); // Wait until UI elements are ready
        ResetHealth(false);  // In case we want to start not at max health
    }

    IEnumerator ResetHealthRoutine(bool updateHealth)
    {
        immune = true;
        maxHealth = originalMaxHealth;
        onMaxHealthUpdate?.Invoke(maxHealth);
        if (updateHealth)
            health = maxHealth;
        yield return new WaitForEndOfFrame(); // Wait for content size fitter
        onHealthUpdate?.Invoke(1);
        if (healthBarSprites != null)
        {
            foreach (SpriteRenderer sprite in healthBarSprites)
            {
                sprite.transform.localScale = healthBarScale;
            }
        }
        yield return new WaitForFixedUpdate();
        immune = false;
    }

    public void ResetHealth(bool updateHealth = true)
    {
        StopAllCoroutines();
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
        if (invincible || immune || amount == 0 || (!canUpdateHealth && !overrideCooldown))
            return false;
        float previousHealth = health;
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
        if (health == previousHealth)
            return false;
        StartCoroutine(HealthCooldown(cooldown));
        float healthPercent = health / maxHealth;
        onHealthUpdate?.Invoke(healthPercent);
        if (healthBarSprites.Length > 0)
        {
            healthBarSprites[0].transform.localScale = new Vector3(healthPercent * healthBarScale.x, healthBarScale.y, healthBarScale.z);

            // Popup health bar for 1 second then fade it out
            foreach (SpriteRenderer sprite in healthBarSprites)
            {
                Color newColor = sprite.color;
                newColor.a = 1;
                sprite.color = newColor;
            }
            if (!PlayerNearby)
                FadeHealthBarOut(1f);
        }
        return true;
    }

    public bool AddMaxHealth(float amount, float cooldown, bool overrideCooldown = false)
    {
        if (invincible || immune || amount == 0 || (!canUpdateHealth && !overrideCooldown))
            return false;
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

        if(healthBarSprites.Length > 0)
        {
            healthBarSprites[0].transform.localScale = new Vector3(healthPercent * healthBarScale.x, healthBarScale.y, healthBarScale.z);

            // Popup health bar for 1 second then fade it out
            foreach (SpriteRenderer sprite in healthBarSprites)
            {
                Color newColor = sprite.color;
                newColor.a = 1;
                sprite.color = newColor;
            }
            if(!PlayerNearby)
                FadeHealthBarOut(1f);
        }
        return true;
    }

    IEnumerator FadeHealthBar(bool fadeIn, float delay)
    {
        yield return new WaitForSeconds(delay);
        float endAlpha = fadeIn ? 1.0f : 0.0f;
        while (healthBarSprites[0].color.a != endAlpha)
        {
            float alpha = Mathf.MoveTowards(healthBarSprites[0].color.a, endAlpha, Time.deltaTime * healthBarFadeSpeed);
            foreach(SpriteRenderer sprite in healthBarSprites)
            {
                Color newColor = sprite.color;
                newColor.a = alpha;
                sprite.color = newColor;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void FadeHealthBarIn(float delay)
    {
        if (!healthBarAlwaysVisible && gameObject.activeInHierarchy)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeHealthBar(true, delay));
        }
    }

    public void FadeHealthBarOut(float delay)
    {
        if (!healthBarAlwaysVisible && gameObject.activeInHierarchy)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeHealthBar(false, delay));
        }
    }
}

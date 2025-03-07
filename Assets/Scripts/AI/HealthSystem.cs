using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private bool invincible = false;
    public float originalMaxHealth { get; private set; }
    public float maxHealth = 10;
    public float health;
    [SerializeField] private SpriteRenderer[] healthBarSprites;
    [SerializeField] private Image bossBarImage;
    [SerializeField] private float bossBarWidth;
    [SerializeField] private float healthBarFadeSpeed = 1f;
    [SerializeField] private bool healthBarAlwaysVisible = false;
    [SerializeField] private Vector3 healthBarScale;
    public bool PlayerNearby { get; set; }
    private Coroutine fadeCoroutine;

    [SerializeField] private UnityEvent<float> onHealthUpdate; // Percentage from 0 to 1 of maxHealth
    [SerializeField] private UnityEvent<float> onMaxHealthUpdate; // Exact max health value
    [SerializeField] private UnityEvent onDeath;
    private bool canUpdateHealth = true;

    public float KillHealAmount;

    private IEnumerator Start()
    {
        originalMaxHealth = maxHealth;
        yield return new WaitForEndOfFrame(); // Wait until UI elements are ready
        ResetHealth(false);  // In case we want to start not at max health
    }

    public void ResetHealth(bool updateHealth = true)
    {
        StopAllCoroutines(); // In case a routine is trying to change health, stop everything
        maxHealth = originalMaxHealth;
        onMaxHealthUpdate?.Invoke(maxHealth);
        if (updateHealth)
            health = maxHealth;
        float healthPercent = health / maxHealth;
        onHealthUpdate?.Invoke(healthPercent);
        if (healthBarSprites != null)
        {
            for (int i = 0; i < healthBarSprites.Length; i++)
            {
                if (i == 0)
                {
                    healthBarSprites[i].transform.localScale = new Vector3(healthPercent * healthBarScale.x, healthBarScale.y, healthBarScale.z);
                }
                else
                {
                    healthBarSprites[i].transform.localScale = healthBarScale;
                }
            }
        }
        if (bossBarImage != null)
        {
            bossBarImage.rectTransform.sizeDelta = new Vector2(healthPercent * bossBarWidth, bossBarImage.rectTransform.sizeDelta.y);
        }
    }

    private IEnumerator HealthCooldown(float time)
    {
        canUpdateHealth = false;
        yield return new WaitForSeconds(time);
        canUpdateHealth = true;
    }

    public void HealthUpdate(bool invokeEvent = true)
    {
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
        if(invokeEvent)
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
        if (bossBarImage != null)
        {
            bossBarImage.rectTransform.sizeDelta = new Vector2(healthPercent * bossBarWidth, bossBarImage.rectTransform.sizeDelta.y);
        }
    }

    public bool AddHealth(float amount, float cooldown, bool overrideCooldown = false)
    {
        if (invincible || amount == 0 || (!canUpdateHealth && !overrideCooldown))
            return false;
        health += amount;
        StartCoroutine(HealthCooldown(cooldown));
        HealthUpdate();
        return true;
    }

    public bool SetHealth(float amount, float cooldown, bool overrideCooldown = false)
    {
        if (invincible || amount == health || (!canUpdateHealth && !overrideCooldown))
            return false;
        health = amount;
        StartCoroutine(HealthCooldown(cooldown));
        HealthUpdate();
        return true;
    }

    private void MaxHealthUpdate()
    {
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
        if (bossBarImage != null)
        {
            bossBarImage.rectTransform.sizeDelta = new Vector2(healthPercent * bossBarWidth, bossBarImage.rectTransform.sizeDelta.y);
        }
    }

    public bool AddMaxHealth(float amount, float cooldown, bool overrideCooldown = false)
    {
        if (invincible || amount == 0 || (!canUpdateHealth && !overrideCooldown))
            return false;
        maxHealth += amount;
        StartCoroutine(HealthCooldown(cooldown));
        MaxHealthUpdate();
        return true;
    }

    public bool SetMaxHealth(float amount, float cooldown, bool overrideCooldown = false)
    {
        if (invincible || amount == maxHealth || (!canUpdateHealth && !overrideCooldown))
            return false;
        maxHealth = amount;
        StartCoroutine(HealthCooldown(cooldown));
        MaxHealthUpdate();
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

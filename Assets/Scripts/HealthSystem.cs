using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] int maxHearts = 10;
    [SerializeField] int hearts;
    bool canDamage = true;
    [SerializeField] Transform healthBar;
    [SerializeField] Vector3 healthBarScale;
    [SerializeField] UnityEvent onDamage;
    [SerializeField] UnityEvent onDeath;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetHealth()
    {
        hearts = maxHearts;
        healthBar.localScale = new Vector3((hearts / (float)maxHearts) * healthBarScale.x, healthBarScale.y, healthBarScale.z);
    }

    public void Damage(int amount, float damageCooldown)
    {
        if (canDamage)
        {
            StartCoroutine(DamageCooldown(damageCooldown));

            hearts -= amount;
            if (hearts <= 0)
            {
                hearts = 0;
                onDeath?.Invoke();
            }
            else
            {
                onDamage?.Invoke();
            }
            healthBar.localScale = new Vector3((hearts / (float)maxHearts) * healthBarScale.x, healthBarScale.y, healthBarScale.z);
        }
    }

    IEnumerator DamageCooldown(float time)
    {
        canDamage = false;
        yield return new WaitForSeconds(time);
        canDamage = true;
    }
}

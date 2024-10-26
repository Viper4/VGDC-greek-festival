using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapFloor : Trigger
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Collider2D trapCollider;

    [SerializeField] GameObject breakParticles;
    [SerializeField] float breakDelay = 0f;
    [SerializeField] Color startColor;
    [SerializeField] Color endColor;
    float breakTimer = 0f;
    AudioSource breakAudio;

    [SerializeField] float mendSpeed = 0.1f;

    [SerializeField] float respawnDelay = 5f;
    bool broken = false;

    private void Start()
    {
        InitializeHashSet();
        TryGetComponent(out breakAudio);
    }

    private void Update()
    {
        if (!broken)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, breakTimer / breakDelay);

            if (collidersInTrigger <= 0 && breakTimer > 0)
            {
                breakTimer -= Time.deltaTime * mendSpeed;
            }
            else
            {
                if (breakTimer >= breakDelay)
                {
                    Break();
                }
                breakTimer += Time.deltaTime;
            }
        }
    }

    private void Break()
    {
        broken = true;
        GameObject newParticles = Instantiate(breakParticles, transform.position, transform.rotation);
        newParticles.transform.eulerAngles = new Vector3(90, newParticles.transform.eulerAngles.y, newParticles.transform.eulerAngles.z);
        ParticleSystem particleSystem = newParticles.GetComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.startColor = startColor;
        var shape = particleSystem.shape;
        shape.radius = transform.localScale.x * 0.5f;
        particleSystem.Play();
        if (breakAudio != null)
            breakAudio.Play();
        if(respawnDelay < 0)
        {
            Destroy(particleSystem.gameObject, 5);
            Destroy(gameObject);
        }
        else
        {
            spriteRenderer.enabled = false;
            trapCollider.enabled = false;
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        breakTimer = 0;
        spriteRenderer.enabled = true;
        trapCollider.enabled = true;
        broken = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapFloor : Trigger
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D trapCollider;

    [SerializeField] private GameObject breakParticles;
    [SerializeField] private float breakDelay = 0f;
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    private float breakTimer = 0f;
    private AudioSource breakAudio;

    [SerializeField] private float mendSpeed = 0.1f;

    [SerializeField] private float respawnDelay = 5f;
    private bool broken = false;

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
        if(respawnDelay <= 0)
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

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        breakTimer = 0;
        spriteRenderer.enabled = true;
        trapCollider.enabled = true;
        broken = false;
    }
}

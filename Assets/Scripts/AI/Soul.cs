using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soul : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Color startColor;
    private Vector3 startPosition;

    public Transform target;
    [SerializeField] private float followSpeed = 3.5f;
    [SerializeField] private float followRadius = 5f;
    [SerializeField] private Color followingColor;
    [SerializeField, ColorUsage(true, true)] private Color followingEmission;

    private Transform ground;

    [SerializeField] private float fadeTime = 0.5f;
    private bool fading = false;

    [SerializeField] private SimpleBob simpleBob;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startColor = spriteRenderer.color;
        startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (!fading)
        {
            if (target != null && (target.position - transform.position).sqrMagnitude > followRadius * followRadius)
            {
                // When the target is far away, follow it with no gravity
                rb.gravityScale = 0;
                rb.velocity = Vector2.zero;
                transform.position = Vector2.Lerp(transform.position, target.position - (target.position - transform.position).normalized * (followRadius - 0.25f), followSpeed * Time.deltaTime);
            }
            else if (ground == null)
            {
                // When we're idle, apply slight gravity until we hit the ground
                rb.gravityScale = 0.1f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!other.isTrigger && !fading && ground == null)
        {
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
            ground = other.transform;
        }
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().PickupSoul(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(!other.isTrigger && !fading && ground == other.transform)
        {
            ground = null;
        }
    }

    public void StartFollow(Transform target)
    {
        simpleBob.enabled = false;
        this.target = target;
        spriteRenderer.color = followingColor;
        spriteRenderer.material.SetColor("_EmissionColor", followingEmission);
    }

    public void ResetSoul()
    {
        target = null;
        spriteRenderer.color = startColor;
        transform.position = startPosition;
        simpleBob.enabled = true;
    }

    public void Fade()
    {
        fading = true;
        rb.gravityScale = -0.25f;
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // Fade spirit's sprite color alpha to 0 and delete it after
        float fadeTimer = 0;
        Color fadeStartColor = spriteRenderer.color;
        Color emissionStartColor = spriteRenderer.material.GetColor("_EmissionColor");
        while(fadeTimer < fadeTime)
        {
            spriteRenderer.color = Color.Lerp(fadeStartColor, Color.clear, fadeTimer / fadeTime);
            spriteRenderer.material.SetColor("_EmissionColor", Color.Lerp(emissionStartColor, Color.clear, fadeTimer / fadeTime));
            fadeTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }
}

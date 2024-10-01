using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soul : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    Color startColor;
    Vector3 startPosition;

    public Transform target;
    [SerializeField] float followSpeed = 3.5f;
    [SerializeField] float followRadius = 5f;
    [SerializeField] Color followingColor;

    Transform ground;

    [SerializeField] float fadeTime = 0.5f;
    bool fading = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startColor = spriteRenderer.color;
        startPosition = transform.position;
    }

    void Update()
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!fading && ground == null)
        {
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
            ground = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(!fading && ground == collision.transform)
        {
            ground = null;
        }
    }

    public void StartFollow(Transform target)
    {
        this.target = target;
        spriteRenderer.color = followingColor;
    }

    public void ResetSoul()
    {
        target = null;
        spriteRenderer.color = startColor;
        transform.position = startPosition;
    }

    public void Fade()
    {
        fading = true;
        rb.gravityScale = -0.25f;
        StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        // Fade spirit's sprite color alpha to 0 and delete it after
        float fadeTimer = 0;
        Color fadeStartColor = spriteRenderer.color;
        while(fadeTimer < fadeTime)
        {
            spriteRenderer.color = Color.Lerp(fadeStartColor, Color.clear, fadeTimer / fadeTime);
            fadeTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }
}

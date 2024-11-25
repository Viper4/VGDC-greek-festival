using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSpike : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D _collider;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    [SerializeField] private float destroyDelay = 0.5f;

    public void StartFall(float gravityScale, Collider2D[] ignoreColliders)
    {
        if(rb == null)
            rb = GetComponent<Rigidbody2D>();
        if(_collider == null)
            _collider = GetComponent<Collider2D>();
        rb.isKinematic = false;
        rb.gravityScale = gravityScale;
        foreach(Collider2D col in ignoreColliders)
        {
            Physics2D.IgnoreCollision(_collider, col, true);
        }
        _collider.enabled = true;
    }

    public void DelayedStopFall(float delay)
    {
        Invoke(nameof(StopFall), delay);
    }

    private void StopFall()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        Destroy(gameObject, destroyDelay);
    }
}

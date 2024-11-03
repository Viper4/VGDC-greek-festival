using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step : Trigger
{
    private Collider2D stepCollider;

    private void Start()
    {
        stepCollider = GetComponent<Collider2D>();
    }

    public override void CollisionEnter(Collision2D collision)
    {
        base.CollisionEnter(collision);
        float stepHeight = stepCollider.bounds.max.y - collision.collider.bounds.min.y + 0.1f;
        if (Mathf.Abs(collision.GetContact(0).normal.y) < 0.1f)
            collision.transform.position += Vector3.up * stepHeight;
        if(collision.transform.TryGetComponent(out Rigidbody2D otherRB))
            otherRB.velocity = new Vector2(otherRB.velocity.x, 0);
    }
}

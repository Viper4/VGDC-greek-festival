using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step : Trigger
{
    [SerializeField] float stepHeight = 0.5f;

    public override void CollisionEnter(Collision2D collision)
    {
        base.CollisionEnter(collision);
        if(collision.GetContact(0).normal.y == 0)
            collision.transform.position += Vector3.up * stepHeight;
        if(collision.transform.TryGetComponent(out Rigidbody2D otherRB))
            otherRB.velocity = new Vector2(otherRB.velocity.x, 0);
    }
}

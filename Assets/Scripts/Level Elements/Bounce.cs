using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour
{
    [SerializeField] private float bounceScale = 1f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        if (Vector2.Angle(contact.normal, Vector2.up) > 160f && collision.transform.TryGetComponent<BaseMovement>(out var baseMovement))
        {
            BaseMovement.KnockbackInfo bounceKnockback = new BaseMovement.KnockbackInfo()
            {
                velocity = bounceScale * -collision.relativeVelocity.y * Vector2.up,
                drag = 1,
                duration = 0.1f
            };
            baseMovement.ApplyKnockback(bounceKnockback, true);
        }
    }
}

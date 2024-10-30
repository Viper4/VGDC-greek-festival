using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Ability
{
    [SerializeField] Transform launchOffset;
    [SerializeField] int projectileCount = 3;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float spreadAngle = 10f;
    [SerializeField] float projectileSpeed = 20f;

    void OnEnable()
    {
        Player.instance.input.Player.Ability1.performed += OnInput;
    }

    void OnDisable()
    {
        Player.instance.input.Player.Ability1.performed -= OnInput;
    }

    public override void Use()
    {
        Vector3 initialVelocity = owner.rb.velocity;
        initialVelocity.y = 0;
        Collider2D[] clones = new Collider2D[projectileCount];
        for (int i = 0; i < projectileCount; i++)
        {
            Quaternion launchRotation = Quaternion.AngleAxis((i - projectileCount / 2) * spreadAngle + launchOffset.eulerAngles.z, launchOffset.forward);
            Collider2D clone = Instantiate(projectilePrefab, launchOffset.position, launchRotation).GetComponent<Collider2D>();
            clone.GetComponent<Rigidbody2D>().velocity = initialVelocity + clone.transform.up * projectileSpeed;
            if(clone.TryGetComponent(out HealthTrigger healthTrigger))
            {
                healthTrigger.knockback = knockback;
            }
            clones[i] = clone;
            Physics2D.IgnoreCollision(owner._collider, clone);

            for (int j = 0; j < i; j++)
            {
                Physics2D.IgnoreCollision(clones[j], clones[i]);
            }
        }
    }
}
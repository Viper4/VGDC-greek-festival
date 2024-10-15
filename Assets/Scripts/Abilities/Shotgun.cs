using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shotgun : MonoBehaviour
{
    [SerializeField] Rigidbody2D ownerRigidbody;
    [SerializeField] Transform launchOffset;
    [SerializeField] int projectileCount = 3;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float spreadAngle = 10f;
    [SerializeField] float projectileSpeed = 20f;
    [SerializeField] float cooldown = 3f;
    float timer = 0;

    void OnEnable()
    {
        Player.playerInput.Player.Ability1.performed += Fire;
    }

    private void OnDisable()
    {
        Player.playerInput.Player.Ability1.performed -= Fire;
    }

    private void Start()
    {
        timer = cooldown;
    }

    private void Update()
    {
        timer += Time.deltaTime;
    }

    void Fire(InputAction.CallbackContext context)
    {
        if (timer >= cooldown)
        {
            timer = 0;
            Vector3 initialVelocity = ownerRigidbody.velocity;
            initialVelocity.y = 0;
            Collider2D[] clones = new Collider2D[projectileCount];
            for (int i = 0; i < projectileCount; i++)
            {
                Quaternion launchRotation = Quaternion.AngleAxis((i - projectileCount / 2) * spreadAngle + launchOffset.eulerAngles.z, launchOffset.forward);
                Collider2D clone = Instantiate(projectilePrefab, launchOffset.position, launchRotation).GetComponent<Collider2D>();
                clone.GetComponent<Rigidbody2D>().velocity = initialVelocity + clone.transform.up * projectileSpeed;
                clones[i] = clone;
                Physics2D.IgnoreCollision(ownerRigidbody.GetComponent<Collider2D>(), clone);

                for (int j = 0; j < i; j++)
                {
                    Physics2D.IgnoreCollision(clones[j], clones[i]);
                }
            }
        }
    }
}
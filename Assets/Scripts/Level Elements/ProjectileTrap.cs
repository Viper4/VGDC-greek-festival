using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTrap : MonoBehaviour
{
    Collider2D _collider;
    [SerializeField] Transform[] launchPoints;
    [SerializeField] GameObject projectile;
    [SerializeField] float projectileSpeed = 8f;
    [SerializeField] float fireRate = 1f;
    float timer = 0;
    [SerializeField] float delay = 0f;


    // Start is called before the first frame update
    void Start()
    {
        TryGetComponent(out _collider);
        timer = -delay;
    }

    // Update is called once per frame
    void Update()
    {
        if(timer > fireRate)
        {
            timer = 0;
            foreach(Transform launchPoint in launchPoints)
            {
                Rigidbody2D projectileRB = Instantiate(projectile, launchPoint.position, launchPoint.rotation).GetComponent<Rigidbody2D>();
                projectileRB.velocity = launchPoint.up * projectileSpeed;
                if(_collider != null)
                    Physics2D.IgnoreCollision(_collider, projectileRB.GetComponent<Collider2D>());
            }
        }

        timer += Time.deltaTime;
    }
}

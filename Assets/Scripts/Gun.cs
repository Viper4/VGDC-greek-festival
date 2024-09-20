using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] float fireCooldown;
    [SerializeField] float bulletSpeed = 5f;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float bulletDespawnTime = 10f;

    bool firing = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Fire(Vector2 initialVelocity)
    {
        if(!firing)
            StartCoroutine(FireRoutine(initialVelocity));
    }

    IEnumerator FireRoutine(Vector2 initialVelocity)
    {
        initialVelocity.y = 0;
        firing = true;
        Rigidbody2D bulletRB = Instantiate(bullet, spawnPoint.position, spawnPoint.rotation).GetComponent<Rigidbody2D>();
        bulletRB.velocity = initialVelocity + (new Vector2(spawnPoint.right.x, spawnPoint.right.y) * bulletSpeed);
        Destroy(bulletRB.gameObject, bulletDespawnTime);
        yield return new WaitForSeconds(fireCooldown);
        firing = false;
    }
}

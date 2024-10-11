using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapFloor : Trigger
{
    [SerializeField] GameObject breakParticles;
    AudioSource breakAudio;

    private void Start()
    {
        InitializeHashSet();
        TryGetComponent(out breakAudio);
    }

    public override void CollisionEnter(Collision2D collision)
    {
        base.CollisionEnter(collision);
        GameObject newParticles = Instantiate(breakParticles, transform.position, transform.rotation);
        newParticles.transform.eulerAngles = new Vector3(90, newParticles.transform.eulerAngles.y, newParticles.transform.eulerAngles.z);
        ParticleSystem particleSystem = newParticles.GetComponent<ParticleSystem>();
        var shape = particleSystem.shape;
        shape.radius = transform.localScale.x * 0.5f;
        particleSystem.Play();
        if (breakAudio != null)
            breakAudio.Play();
        Destroy(particleSystem.gameObject, 5);
        Destroy(gameObject);
    }
}

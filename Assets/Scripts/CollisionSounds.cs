using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float strengthThreshold = 2;
    [SerializeField] private float[] pitchRange = { 0.8f, 1.2f };
    [SerializeField] private float volumeMultiplier = 0.1f;

    // Start is called before the first frame update
    private void Start()
    {
        if(audioSource == null)
        {
            audioSource = GetComponentInChildren<AudioSource>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        float collisionStrength = Vector3.Dot(contact.normal, collision.relativeVelocity);
        if (collision.rigidbody != null)
        {
            collisionStrength *= collision.rigidbody.mass;
        }
        if (collisionStrength >= strengthThreshold)
        {
            audioSource.transform.position = contact.point;
            audioSource.pitch = Random.Range(pitchRange[0], pitchRange[1]);
            audioSource.volume = collisionStrength * volumeMultiplier;
            audioSource.Play();
        }
    }
}

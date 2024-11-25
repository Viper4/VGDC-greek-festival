using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSpikes : Trigger
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D trapCollider;

    [SerializeField] private Transform[] spikeSpawnPoints;
    [SerializeField] private float[] fallDelays;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private float spikeGravityScale = 1f;
    [SerializeField] Collider2D[] ignoreColliders;

    [SerializeField] private float activateDelay = 0f;
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    private float activateTimer = 0f;
    private AudioSource fallAudio;

    [SerializeField] private float mendSpeed = 0.1f;
    [SerializeField] private float respawnDelay = 5f;
    private bool used = false;

    private void Start()
    {
        InitializeHashSet();
        TryGetComponent(out fallAudio);
    }

    private void Update()
    {
        if (!used)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, activateTimer / activateDelay);

            if (collidersInTrigger <= 0 && activateTimer > 0)
            {
                activateTimer -= Time.deltaTime * mendSpeed;
            }
            else
            {
                if (activateTimer >= activateDelay)
                {
                    Fall();
                }
                activateTimer += Time.deltaTime;
            }
        }
    }

    private void Fall()
    {
        for(int i = 0; i < spikeSpawnPoints.Length; i++)
        {
            StartCoroutine(DelayedSpikeFall(i));
        }

        used = true;
        if (fallAudio != null)
            fallAudio.Play();
        if(respawnDelay <= 0)
        {
            Destroy(gameObject, Mathf.Max(fallDelays) + 0.1f);
        }
        else
        {
            trapCollider.enabled = false;
            StartCoroutine(Respawn());
        }
    }

    private IEnumerator DelayedSpikeFall(int index)
    {
        yield return new WaitForSeconds(fallDelays[index]);
        spikeSpawnPoints[index].gameObject.SetActive(false);
        FallingSpike spike = Instantiate(spikePrefab, spikeSpawnPoints[index].position, spikeSpawnPoints[index].rotation).GetComponent<FallingSpike>();
        spike.StartFall(spikeGravityScale, ignoreColliders);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        foreach(Transform spawnPoint in spikeSpawnPoints)
        {
            spawnPoint.gameObject.SetActive(true);
        }
        activateTimer = 0;
        trapCollider.enabled = true;
        used = false;
    }
}

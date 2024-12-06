using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;

    [SerializeField] private SpriteRenderer selectionIndicator;
    [SerializeField] private Light2D selectionLight;
    [SerializeField] private Color unselectedColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.green;
    [SerializeField, ColorUsage(true, true)] private Color unselectedEmission = Color.yellow;
    [SerializeField, ColorUsage(true, true)] private Color selectedEmission = Color.green;
    [SerializeField] private AudioClip selectSound;
    private bool unlocked = false;
    public bool canSaveSouls = true;
    [SerializeField] private UnityEvent<int> onSelect;
    [SerializeField] private UnityEvent onPlayerRespawn;

    [SerializeField] private ParticleSystem respawnParticles;

    [SerializeField] bool canSaveState = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer.color = lockedColor;
        selectionIndicator.color = unselectedColor;
        selectionIndicator.material.SetColor("_EmissionColor", unselectedEmission);
    }

    public void Respawn()
    {
        onPlayerRespawn?.Invoke();
        respawnParticles.Play();
    }

    public void Select(int souls, bool save = true)
    {
        if(selectionIndicator.color != selectedColor)
        {
            selectionIndicator.color = selectedColor;
            selectionIndicator.material.SetColor("_EmissionColor", selectedEmission);
            if(selectionLight != null)
                selectionLight.color = selectedColor;
            if(audioSource != null)
                audioSource.PlayOneShot(selectSound);
        }
        if (!unlocked)
        {
            if (!canSaveSouls)
                souls = 0;
            onSelect?.Invoke(souls);
        }

        if(canSaveState && save)
            SaveSystem.instance.Save();
    }

    public void Deselect()
    {
        selectionIndicator.color = unselectedColor;
        selectionIndicator.material.SetColor("_EmissionColor", unselectedEmission);
        if (selectionLight != null)
            selectionLight.color = unselectedColor;
    }

    public void Unlock()
    {
        unlocked = true;
        spriteRenderer.color = unlockedColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().SetCheckpoint(this);
        }
    }
}

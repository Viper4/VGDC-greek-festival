using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    AudioSource audioSource;

    [SerializeField] Color lockedColor = Color.red;
    [SerializeField] Color unlockedColor = Color.green;

    [SerializeField] SpriteRenderer selectionIndicator;
    [SerializeField] Light2D selectionLight;
    [SerializeField] Color unselectedColor = Color.yellow;
    [SerializeField] Color selectedColor = Color.green;
    [SerializeField, ColorUsage(true, true)] Color unselectedEmission = Color.yellow;
    [SerializeField, ColorUsage(true, true)] Color selectedEmission = Color.green;
    [SerializeField] AudioClip selectSound;
    bool unlocked = false;
    public bool canSaveSouls = true;
    [SerializeField] UnityEvent<int> onSelect;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer.color = lockedColor;
        selectionIndicator.color = unselectedColor;
        selectionIndicator.material.SetColor("_EmissionColor", unselectedEmission);
    }

    public void Select(int souls)
    {
        if(selectionIndicator.color != selectedColor)
        {
            selectionIndicator.color = selectedColor;
            selectionIndicator.material.SetColor("_EmissionColor", selectedEmission);
            if(selectionLight != null)
                selectionLight.color = selectedColor;
            audioSource.PlayOneShot(selectSound);
        }
        if (!unlocked)
        {
            if (!canSaveSouls)
                souls = 0;
            onSelect?.Invoke(souls);
        }
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    AudioSource audioSource;

    [SerializeField] Animation nextUnlock;
    [SerializeField] int soulsForUnlock = 3;
    [SerializeField] Color lockedColor = Color.red;
    [SerializeField] Color unlockedColor = Color.green;

    [SerializeField] SpriteRenderer selectionIndicator;
    [SerializeField] Color unselectedColor = Color.yellow;
    [SerializeField] Color selectedColor = Color.green;
    [SerializeField] AudioClip selectSound;
    bool unlocked = false;
    int soulsSaved = 0;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer.color = lockedColor;
        selectionIndicator.color = unselectedColor;
    }

    public void Save(int souls)
    {
        if(selectionIndicator.color != selectedColor)
        {
            selectionIndicator.color = selectedColor;
            audioSource.PlayOneShot(selectSound);
        }
        if (!unlocked)
        {
            soulsSaved += souls;
            if (soulsSaved >= soulsForUnlock)
            {
                if(nextUnlock != null)
                    nextUnlock.Play();
                unlocked = true;
                spriteRenderer.color = unlockedColor;
            }
        }
    }

    public void Deselect()
    {
        selectionIndicator.color = unselectedColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().SetCheckpoint(this);
        }
    }
}

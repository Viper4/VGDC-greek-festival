using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

public class Interact : Trigger, ISaveable
{
    public UnityEvent OnInteract;
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private float fadeTime = 0.5f;
    private Coroutine fadeRoutine;
    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    [SerializeField] private bool disableOnInteract = true;
    private bool interacted = false;

    private void OnEnable()
    {
        Player.instance.input.Player.Interact.performed += (ctx) => TryInteract();
    }

    private void OnDisable()
    {
        Player.instance.input.Player.Interact.performed -= (ctx) => TryInteract();
    }

    private void DoInteract()
    {
        OnInteract?.Invoke();
        if (disableOnInteract)
            interacted = true;
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        popupText.color = startColor;
    }

    private void TryInteract()
    {
        if(collidersInTrigger > 0)
        {
            DoInteract();
        }
    }

    public void ResetInteract()
    {
        interacted = false;
    }

    public override void TriggerEnter(Collider2D collider)
    {
        base.TriggerEnter(collider);
        if (!interacted)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }
            fadeRoutine = StartCoroutine(Popup(true));
        }
    }

    public override void TriggerExit(Collider2D collider)
    {
        base.TriggerExit(collider);
        if (!interacted)
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }
            fadeRoutine = StartCoroutine(Popup(false));
        }
    }

    private IEnumerator Popup(bool fadeIn)
    {
        float timer = 0;
        Color start;
        Color end;
        if(fadeIn)
        {
            start = popupText.color;
            end = endColor;
        }
        else
        {
            start = popupText.color;
            end = startColor;
        }
        while (timer < fadeTime)
        {
            popupText.color = Color.Lerp(start, end, timer / fadeTime);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        popupText.color = end;
    }

    public object CaptureState()
    {
        return interacted;
    }

    public void RestoreState(object state)
    {
        interacted = (bool)state;
        if(interacted)
            DoInteract();
    }

    public void Delete()
    {
        Destroy(gameObject);
    }
}
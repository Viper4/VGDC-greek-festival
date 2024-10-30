using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

public class Interact : Trigger
{
    public UnityEvent OnInteract;
    [SerializeField] TextMeshProUGUI popupText;
    [SerializeField] float fadeTime = 0.5f;
    Coroutine fadeRoutine;
    [SerializeField] Color startColor;
    [SerializeField] Color endColor;
    bool interacted = false;

    void OnEnable()
    {
        Player.instance.input.Player.Interact.performed += DoInteract;
    }

    private void OnDisable()
    {
        Player.instance.input.Player.Interact.performed -= DoInteract;
    }

    void DoInteract(InputAction.CallbackContext context)
    {
        if(collidersInTrigger > 0)
        {
            OnInteract?.Invoke();
            interacted = true;
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);
            popupText.color = startColor;
        }
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

    IEnumerator Popup(bool fadeIn)
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
}
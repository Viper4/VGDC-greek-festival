using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GroundPound : MonoBehaviour
{
    [SerializeField] private float speed = 18f;
    [HideInInspector] public float velocity = 0;
    private Coroutine groundPoundRoutine;

    [SerializeField] private UnityEvent onGroundPound;

    public void StartGroundPound()
    {
        onGroundPound?.Invoke();
        if (groundPoundRoutine != null)
            StopCoroutine(groundPoundRoutine);
        groundPoundRoutine = StartCoroutine(GroundPoundRoutine());
    }

    IEnumerator GroundPoundRoutine()
    {
        velocity = -speed;
        yield return new WaitUntil(() => Player.instance.IsGrounded);
        Player.instance.UncrouchAnimation();
        velocity = 0;
    }

    public void Cancel()
    {
        if (groundPoundRoutine != null)
            StopCoroutine(groundPoundRoutine);
        Player.instance.UncrouchAnimation();
        velocity = 0;
    }
}

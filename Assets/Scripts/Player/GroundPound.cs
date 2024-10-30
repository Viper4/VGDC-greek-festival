using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPound : MonoBehaviour
{
    [SerializeField] float speed = 18f;
    [HideInInspector] public float velocity = 0;

    public void StartGroundPound()
    {
        StartCoroutine(GroundPoundRoutine());
    }

    IEnumerator GroundPoundRoutine()
    {
        velocity = -speed;
        yield return new WaitUntil(() => Player.instance.IsGrounded);
        velocity = 0;
    }
}

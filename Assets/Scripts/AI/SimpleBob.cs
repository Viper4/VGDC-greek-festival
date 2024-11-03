using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBob : MonoBehaviour
{
    private Vector2 startPosition;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 1f;
    private float seed = 0;

    private void Start()
    {
        ResetBob();
    }

    // Update is called once per frame
    private void Update()
    {
        transform.localPosition = startPosition + bobHeight * Mathf.Sin(seed + Time.time * bobSpeed) * Vector2.up;
    }

    public void ResetBob()
    {
        seed = Random.Range(-999f, 999f);
        startPosition = transform.localPosition;
    }
}

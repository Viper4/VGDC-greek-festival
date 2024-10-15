using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBob : MonoBehaviour
{
    Vector2 startPosition;
    [SerializeField] float bobHeight = 0.5f;
    [SerializeField] float bobSpeed = 1f;
    float seed = 0;

    void Start()
    {
        ResetBob();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = startPosition + bobHeight * Mathf.Sin(seed + Time.time * bobSpeed) * Vector2.up;
    }

    public void ResetBob()
    {
        seed = Random.Range(-999f, 999f);
        startPosition = transform.localPosition;
    }
}

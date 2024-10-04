using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    [SerializeField] string[] triggerTags;
    [SerializeField] UnityEvent onTriggerEnter;
    [SerializeField] UnityEvent onTriggerExit;
    [SerializeField] UnityEvent onCollisionEnter;
    [SerializeField] UnityEvent onCollisionExit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        onTriggerEnter?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        onTriggerExit?.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        onCollisionEnter?.Invoke();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        onCollisionExit?.Invoke();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    [SerializeField] string[] triggerTags;
    public HashSet<string> tagHashSet = new HashSet<string>();
    [SerializeField] UnityEvent onTriggerEnter;
    [SerializeField] UnityEvent onTriggerExit;
    [SerializeField] UnityEvent onCollisionEnter;
    [SerializeField] UnityEvent onCollisionExit;
    public int collidersInTrigger = 0;

    private void Start()
    {
        InitializeHashSet();
    }

    public void InitializeHashSet()
    {
        foreach (string tag in triggerTags)
        {
            if (!tagHashSet.Contains(tag))
                tagHashSet.Add(tag);
        }
    }

    public virtual void TriggerEnter(Collider2D collider)
    {
        onTriggerEnter?.Invoke();
        collidersInTrigger++;
    }

    public virtual void TriggerExit(Collider2D collider)
    {
        onTriggerExit?.Invoke();
        collidersInTrigger--;
        if(collidersInTrigger <= 0)
            collidersInTrigger = 0;
    }

    public virtual void CollisionEnter(Collision2D collision)
    {
        onCollisionEnter?.Invoke();
        collidersInTrigger++;
    }

    public virtual void CollisionExit(Collision2D collision)
    {
        onCollisionExit?.Invoke();
        collidersInTrigger--;
        if (collidersInTrigger <= 0)
            collidersInTrigger = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (tagHashSet.Count == 0 || tagHashSet.Contains(collision.tag))
            TriggerEnter(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (tagHashSet.Count == 0 || tagHashSet.Contains(collision.tag))
            TriggerExit(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (tagHashSet.Count == 0 || tagHashSet.Contains(collision.transform.tag))
            CollisionEnter(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (tagHashSet.Count == 0 || tagHashSet.Contains(collision.transform.tag))
            CollisionExit(collision);
    }
}

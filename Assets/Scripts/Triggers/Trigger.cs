using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    [SerializeField] string[] triggerTags;
    public HashSet<string> tagHashSet = new HashSet<string>();
    [SerializeField] UnityEvent<Collider2D> onTriggerEnter;
    [SerializeField] UnityEvent<Collider2D> onTriggerExit;
    [SerializeField] UnityEvent<Collision2D> onCollisionEnter;
    [SerializeField] UnityEvent<Collision2D> onCollisionExit;

    [SerializeField] UnityEvent<Collider2D> onAnyTriggerEnter;
    [SerializeField] UnityEvent<Collider2D> onAnyTriggerExit;
    [SerializeField] UnityEvent<Collision2D> onAnyCollisionEnter;
    [SerializeField] UnityEvent<Collision2D> onAnyCollisionExit;
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
        collidersInTrigger++;
        onTriggerEnter?.Invoke(collider);
    }

    public virtual void TriggerExit(Collider2D collider)
    {
        collidersInTrigger--;
        if(collidersInTrigger <= 0)
            collidersInTrigger = 0;
        onTriggerExit?.Invoke(collider);
    }

    public virtual void CollisionEnter(Collision2D collision)
    {
        collidersInTrigger++;
        onCollisionEnter?.Invoke(collision);
    }

    public virtual void CollisionExit(Collision2D collision)
    {
        collidersInTrigger--;
        if (collidersInTrigger <= 0)
            collidersInTrigger = 0;
        onCollisionExit?.Invoke(collision);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (tagHashSet.Count == 0 || tagHashSet.Contains(collider.tag))
            TriggerEnter(collider);
        onAnyTriggerEnter?.Invoke(collider);
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (tagHashSet.Count == 0 || tagHashSet.Contains(collider.tag))
            TriggerExit(collider);
        onAnyTriggerExit?.Invoke(collider);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (tagHashSet.Count == 0 || tagHashSet.Contains(collision.transform.tag))
            CollisionEnter(collision);
        onAnyCollisionEnter?.Invoke(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (tagHashSet.Count == 0 || tagHashSet.Contains(collision.transform.tag))
            CollisionExit(collision);
        onAnyCollisionExit?.Invoke(collision);
    }

    public void DestroyGameObject(GameObject toDestroy)
    {
        Destroy(toDestroy);
    }
}

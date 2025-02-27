using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : Trigger
{
    [SerializeField] private Collider2D collisionCollider;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private float startAlpha = 1;
    [SerializeField] private float endAlpha = 0.1f;
    [SerializeField] private float maxFadeDistance = 3;
    private List<Collider2D> targets = new List<Collider2D>();

    private void Start()
    {
        spriteRenderer = collisionCollider.GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if(targets.Count > 0)
        {
            float highestY = targets[0].transform.position.y;
            for(int i = 1; i < targets.Count; i++)
            {
                if (targets[i].transform.position.y > highestY)
                {
                    highestY = targets[i].transform.position.y;
                }
            }
            // Assuming transform position is center
            Color color = spriteRenderer.color;
            float t = 1 - Mathf.Clamp01((collisionCollider.transform.position.y - highestY) / maxFadeDistance);
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            spriteRenderer.color = color;
        }
    }

    public override void TriggerEnter(Collider2D collider)
    {
        base.TriggerEnter(collider);
        if(collider.transform.position.y < collisionCollider.transform.position.y)
        {
            targets.Add(collider);
            Physics2D.IgnoreCollision(collisionCollider, collider, true);
        }
        else if(collider.TryGetComponent(out BaseMovement baseMovement))
        {
            baseMovement.stairs = this;
        }
    }

    public override void TriggerExit(Collider2D collider)
    {
        base.TriggerExit(collider);
        Physics2D.IgnoreCollision(collisionCollider, collider, false);
        targets.Remove(collider);
        if(targets.Count == 0)
        {
            Color color = spriteRenderer.color;
            color.a = startAlpha;
            spriteRenderer.color = color;
        }
    }

    public void Descend(Collider2D otherCollider)
    {
        Physics2D.IgnoreCollision(collisionCollider, otherCollider, true);
        if (!targets.Contains(otherCollider))
        {
            targets.Add(otherCollider);
        }
    }

    [ExecuteInEditMode]
    public void GenerateStairs(GameObject stepPrefab, int numSteps, Vector2 stepScale)
    {
        if (numSteps <= 0 || stepPrefab == null)
            return;
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.Contains("Step"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
        for(int i = 0; i < numSteps; i++)
        {
            GameObject newStep = Instantiate(stepPrefab, transform.position, transform.rotation, transform);
            Collider2D stepCollider = newStep.GetComponent<Collider2D>();
            newStep.transform.localScale = stepScale;
            newStep.transform.position += new Vector3(stepCollider.bounds.size.x * stepScale.x, stepCollider.bounds.extents.y * stepScale.y) * i;
            newStep.transform.localScale = new Vector3(stepScale.x, (i + 1) * stepCollider.bounds.size.y * stepScale.y);
            newStep.name = "Step " + i;
        }

        if(!TryGetComponent(out BoxCollider2D triggerCollider))
        {
            triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        triggerCollider.isTrigger = true;

        triggerCollider.offset = new Vector2((numSteps - 1) * stepScale.x * 0.5f, (numSteps - 1) * stepScale.y * 0.5f);
        triggerCollider.size = new Vector2(numSteps * stepScale.x, numSteps * stepScale.y);
    }
}

using System.Collections;
using UnityEngine;

public class DelayedSpikes : Trigger
{
    [SerializeField] private float delay = 0.75f;
    [SerializeField] private float spikeSpeed = 1;
    [SerializeField] private Vector2 retractedPosition;
    [SerializeField] private Vector2 extendedPosition;
    [SerializeField] private Transform spikeParent;

    private void Start()
    {
        InitializeHashSet();
    }

    private IEnumerator ExtendSpikes()
    {
        yield return new WaitForSeconds(delay);
        while (new Vector2(spikeParent.localPosition.x, spikeParent.localPosition.y) != extendedPosition)
        {
            spikeParent.localPosition = Vector2.MoveTowards(spikeParent.localPosition, extendedPosition, spikeSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator RetractSpikes()
    {
        while (new Vector2(spikeParent.localPosition.x, spikeParent.localPosition.y) != retractedPosition)
        {
            spikeParent.localPosition = Vector2.MoveTowards(spikeParent.localPosition, retractedPosition, spikeSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    public override void TriggerEnter(Collider2D collider)
    {
        base.TriggerExit(collider);
        StopAllCoroutines();
        StartCoroutine(ExtendSpikes());
    }

    public override void TriggerExit(Collider2D collider)
    {
        base.TriggerExit(collider);
        if (collidersInTrigger == 0)
        {
            StopAllCoroutines();
            StartCoroutine(RetractSpikes());
        }
    }
}

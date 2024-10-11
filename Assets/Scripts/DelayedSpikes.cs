using System.Collections;
using UnityEngine;

public class DelayedSpikes : Trigger
{
    [SerializeField] float delay = 0.75f;
    [SerializeField] float spikeSpeed = 1;
    [SerializeField] Vector2 retractedPosition;
    [SerializeField] Vector2 extendedPosition;
    [SerializeField] Transform spikeParent;

    private void Start()
    {
        InitializeHashSet();
    }

    IEnumerator ExtendSpikes()
    {
        yield return new WaitForSeconds(delay);
        while (new Vector2(spikeParent.localPosition.x, spikeParent.localPosition.y) != extendedPosition)
        {
            spikeParent.localPosition = Vector2.MoveTowards(spikeParent.localPosition, extendedPosition, spikeSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator RetractSpikes()
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

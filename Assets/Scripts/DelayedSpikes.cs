using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedSpikes : MonoBehaviour
{
    [SerializeField] string triggerTag;
    [SerializeField] float delay = 0.75f;
    [SerializeField] float spikeSpeed = 1;
    [SerializeField] Vector2 retractedPosition;
    [SerializeField] Vector2 extendedPosition;
    [SerializeField] Transform spikeParent;
    bool extended = false;

    IEnumerator ExtendSpikes()
    {
        extended = true;
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
        extended = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(triggerTag == "" || other.CompareTag(triggerTag) && !extended)
        {
            StopAllCoroutines();
            StartCoroutine(ExtendSpikes());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (triggerTag == "" || other.CompareTag(triggerTag))
        {
            StopAllCoroutines();
            StartCoroutine(RetractSpikes());
        }
    }
}

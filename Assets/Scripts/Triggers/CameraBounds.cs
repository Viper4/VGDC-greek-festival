using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBounds : Trigger
{
    Collider2D boundsCollider;

    // Start is called before the first frame update
    void Start()
    {
        InitializeHashSet();
        boundsCollider = GetComponent<Collider2D>();
    }

    public override void TriggerEnter(Collider2D collider)
    {
        base.TriggerEnter(collider);
        CameraControl cameraControl = Camera.main.GetComponent<CameraControl>();
        cameraControl.cameraBoundsMin = boundsCollider.bounds.min;
        cameraControl.cameraBoundsMax = boundsCollider.bounds.max;
    }

    public override void TriggerExit(Collider2D collider)
    {
        base.TriggerExit(collider);
        ExitBounds();
    }

    public void ExitBounds()
    {
        Camera.main.GetComponent<CameraControl>().cameraBoundsMin = Vector2.zero;
        Camera.main.GetComponent<CameraControl>().cameraBoundsMax = Vector2.zero;
    }
}

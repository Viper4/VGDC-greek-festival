using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBounds : Trigger
{
    Collider2D boundsCollider;
    [SerializeField] bool changeOrthoSize = false;
    float originalOrthoSize;
    Vector2 boundsMin;
    Vector2 boundsMax;

    // Start is called before the first frame update
    void Start()
    {
        InitializeHashSet();
        boundsCollider = GetComponent<Collider2D>();
        boundsMin = boundsCollider.bounds.min;
        boundsMax = boundsCollider.bounds.max;
    }

    public override void TriggerEnter(Collider2D collider)
    {
        base.TriggerEnter(collider);

        CameraControl cameraControl = Camera.main.GetComponent<CameraControl>();
        cameraControl.cameraBoundsMin = boundsMin;
        cameraControl.cameraBoundsMax = boundsMax;
        if (changeOrthoSize)
        {
            originalOrthoSize = Camera.main.orthographicSize;
            float boundsHeight = (boundsMax.y - boundsMin.y) * 0.5f;
            if (boundsHeight < Camera.main.orthographicSize)
            {
                Camera.main.orthographicSize = boundsHeight;
            }
        }
    }

    public override void TriggerExit(Collider2D collider)
    {
        base.TriggerExit(collider);
        ExitBounds();
    }

    public void ExitBounds()
    {
        if(changeOrthoSize)
            Camera.main.orthographicSize = originalOrthoSize;
        CameraControl cameraControl = Camera.main.GetComponent<CameraControl>();
        if(cameraControl.cameraBoundsMin == boundsMin && cameraControl.cameraBoundsMax == boundsMax)
        {
            cameraControl.cameraBoundsMin = Vector2.zero;
            cameraControl.cameraBoundsMax = Vector2.zero;
        }
    }
}

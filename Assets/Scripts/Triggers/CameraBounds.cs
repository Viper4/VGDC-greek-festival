using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBounds : Trigger
{
    Collider2D boundsCollider;
    [SerializeField] bool lockCamera = false;
    public Vector2 min;
    public Vector2 max;
    CameraBounds otherBounds;

    // Start is called before the first frame update
    void Start()
    {
        InitializeHashSet();
        if (!TryGetComponent(out boundsCollider) && min == max)
        {
            Debug.LogWarning("No bounds collider found on " + transform.name + " and min = max!");
        }
        else
        {
            min = boundsCollider.bounds.min;
            max = boundsCollider.bounds.max;
        }
    }

    public override void TriggerEnter(Collider2D collider)
    {
        base.TriggerEnter(collider);

        CameraControl cameraControl = Camera.main.GetComponent<CameraControl>();
        otherBounds = cameraControl.bounds;
        if (lockCamera)
        {
            cameraControl.lockedPosition = transform.position;
            cameraControl.locked = true;
        }
        cameraControl.bounds = this;
        float boundsWidth = (max.x - min.x) * 0.5f;
        float boundsHeight = Mathf.Min((max.y - min.y) * 0.5f, boundsWidth / Camera.main.aspect);

        Camera.main.orthographicSize = Mathf.Clamp(boundsHeight, 0, cameraControl.originalHeight);
    }

    public override void TriggerExit(Collider2D collider)
    {
        base.TriggerExit(collider);
        if (Camera.main != null)
            ExitBounds();
    }

    public void ExitBounds()
    {
        CameraControl cameraControl = Camera.main.GetComponent<CameraControl>();
        if (cameraControl.bounds == this)
        {
            cameraControl.locked = false;
            cameraControl.bounds = otherBounds;
            Camera.main.orthographicSize = cameraControl.originalHeight;
        }
        else if(cameraControl.bounds.otherBounds == this)
        {
            cameraControl.bounds.otherBounds = null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    private Camera _camera;
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 1f;
    public CameraBounds bounds;
    public Vector2 lockedPosition;
    public bool locked = false;
    [HideInInspector] public float originalHeight;
    [HideInInspector] public float originalWidth;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        originalHeight = _camera.orthographicSize;
        originalWidth = originalHeight * _camera.aspect;
    }

    private void FixedUpdate()
    {
        Vector3 newPosition;
        if (locked)
        {
            newPosition = lockedPosition;
        }
        else
        {
            newPosition = target.position;
            if(bounds != null)
            {
                float height = _camera.orthographicSize;
                float width = height * _camera.aspect;
                if (bounds.min.y != 0 && bounds.max.y != 0)
                {
                    float minY = bounds.min.y + height;
                    float maxY = bounds.max.y - height;
                    newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
                }
                if (bounds.min.x != 0 && bounds.max.x != 0)
                {
                    float minX = bounds.min.x + width;
                    float maxX = bounds.max.x - width;
                    newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
                }
            }
        }
        newPosition.z = -1; // Keep the camera above everything so we can see
        transform.position = Vector3.Lerp(transform.position, newPosition, followSpeed * Time.deltaTime);
    }
}

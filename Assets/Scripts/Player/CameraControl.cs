using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    Camera _camera;
    [SerializeField] Transform target;
    [SerializeField] float followSpeed = 1f;
    public Vector2 cameraBoundsMin = new Vector2(0, 0);
    public Vector2 cameraBoundsMax = new Vector2(0, 0);

    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    void FixedUpdate()
    {
        float height = _camera.orthographicSize;
        float width = height * _camera.aspect;
        Vector3 newPosition = target.position;
        if (cameraBoundsMin.y != 0 && cameraBoundsMax.y != 0)
        {
            float minY = cameraBoundsMin.y + height;
            float maxY = cameraBoundsMax.y - height;
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        }
        if (cameraBoundsMin.x != 0 && cameraBoundsMax.x != 0)
        {
            float minX = cameraBoundsMin.x + width;
            float maxX = cameraBoundsMax.x - width;
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        }
        newPosition.z = -1; // Keep the camera above everything so we can see
        transform.position = Vector3.Lerp(transform.position, newPosition, followSpeed * Time.deltaTime);
    }
}

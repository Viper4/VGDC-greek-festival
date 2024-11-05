using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    private Camera _camera;
    [SerializeField] private bool preview = false;
    [SerializeField] private float previewSensitivity = 0.75f;
    private Vector2 previewPosition;
    [SerializeField] private Transform target;
    private Vector3 targetPreviousPosition;
    [SerializeField] private float followSpeed = 1f;
    public CameraBounds bounds;
    public Vector2 lockedPosition;
    public bool locked = false;
    [HideInInspector] public float originalHeight;
    [HideInInspector] public float originalWidth;

    private void OnEnable()
    {
        Player.instance.input.Player.CameraPreview.started += ctx => TogglePreview(true);
        Player.instance.input.Player.CameraPreview.canceled += ctx => TogglePreview(false);
    }

    private void OnDisable()
    {
        Player.instance.input.Player.CameraPreview.started -= ctx => TogglePreview(true);
        Player.instance.input.Player.CameraPreview.canceled -= ctx => TogglePreview(false);
    }

    private void Start()
    {
        _camera = GetComponent<Camera>();
        originalHeight = _camera.orthographicSize;
        originalWidth = originalHeight * _camera.aspect;
        targetPreviousPosition = target.position;
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
            if (preview)
            {
                Vector2 mouseDelta = Player.instance.input.Player.Look.ReadValue<Vector2>();
                Vector2 targetDelta = target.position - targetPreviousPosition;
                previewPosition += mouseDelta * previewSensitivity + targetDelta;
                newPosition = ClampWithinBounds(previewPosition);
                previewPosition = newPosition;
            }
            else
            {
                newPosition = ClampWithinBounds(target.position);
            }
        }
        newPosition.z = -1; // Keep the camera above everything so we can see
        transform.position = Vector3.Lerp(transform.position, newPosition, followSpeed * Time.deltaTime);
        targetPreviousPosition = target.position;
    }

    private Vector2 ClampWithinBounds(Vector2 position)
    {
        Vector2 clampedPosition = position;
        if (bounds != null)
        {
            float height = _camera.orthographicSize;
            float width = height * _camera.aspect;

            float minY = bounds.min.y + height;
            float maxY = bounds.max.y - height;
            clampedPosition.y = Mathf.Clamp(position.y, minY, maxY);

            float minX = bounds.min.x + width;
            float maxX = bounds.max.x - width;
            clampedPosition.x = Mathf.Clamp(position.x, minX, maxX);
        }
        return clampedPosition;
    }

    private void TogglePreview(bool value)
    {
        if (value)
        {
            previewPosition = _camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        }
        preview = value;
    }
}

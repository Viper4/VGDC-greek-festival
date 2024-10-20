using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float followSpeed = 1f;

    void FixedUpdate()
    {
        Vector3 newPosition = Vector2.Lerp(transform.position, target.position, followSpeed * Time.deltaTime);
        newPosition.z = -1; // Keep the camera above everything so we can see
        transform.position = newPosition;
    }
}

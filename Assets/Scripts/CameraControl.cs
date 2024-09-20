using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float followSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 position = Vector2.Lerp(transform.position, target.position, followSpeed * Time.deltaTime);
        position.z = -10;
        transform.position = position;
    }
}

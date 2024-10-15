using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    float length;
    float startPosition;
    [SerializeField] float parallaxEffect;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        float temp = Camera.main.transform.position.x * (1 - parallaxEffect);
        float distance = Camera.main.transform.position.x * parallaxEffect;
        transform.position = new Vector3(startPosition + distance, transform.position.y, transform.position.z);

        if(temp > startPosition + length)
        {
            startPosition += length;
        }
        else if(temp < startPosition - length)
        {
            startPosition -= length;
        }
    }
}

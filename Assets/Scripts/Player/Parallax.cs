using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length;
    private float startPosition;
    [SerializeField] private float parallaxEffect;

    // Start is called before the first frame update
    private void Start()
    {
        startPosition = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    private void Update()
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

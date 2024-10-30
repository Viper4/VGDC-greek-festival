using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] Light2D _light;
    [SerializeField] float minIntensity;
    [SerializeField] float maxIntensity;
    [SerializeField] float noiseSpeed;
    float noise = 0;

    // Start is called before the first frame update
    void Start()
    {
        noise = Random.Range(-999f, 999f);
    }

    // Update is called once per frame
    void Update()
    {
        _light.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PerlinNoise1D(noise));
        noise += Time.deltaTime * noiseSpeed;
    }
}

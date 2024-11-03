using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    [SerializeField] private Light2D _light;
    [SerializeField] private float minIntensity;
    [SerializeField] private float maxIntensity;
    [SerializeField] private float noiseSpeed;
    private float noise = 0;

    // Start is called before the first frame update
    private void Start()
    {
        noise = Random.Range(-999f, 999f);
    }

    // Update is called once per frame
    private void Update()
    {
        _light.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PerlinNoise1D(noise));
        noise += Time.deltaTime * noiseSpeed;
    }
}

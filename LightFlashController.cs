using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightFlashController : MonoBehaviour
{
    [SerializeField] private Light2D light2D;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float flashSpeed = 2f;

    private void Start()
    {
        if (light2D == null)
        {
            light2D = GetComponent<Light2D>();
        }
    }

    private void Update()
    {
        float flickerIntensity = Mathf.Lerp(minIntensity, maxIntensity,
            (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f);
        light2D.intensity = flickerIntensity;
    }
}

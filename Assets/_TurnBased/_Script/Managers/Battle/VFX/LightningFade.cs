using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningFade : MonoBehaviour
{
    private Light objectLight;
    private float initialIntensity;
    private float fadeDuration = 1f;
    private bool flag = false;
    [SerializeField] private float minIntensity = 0;

    void Start()
    {
        if (objectLight == null) objectLight = GetComponent<Light>();
        
        if (initialIntensity == 0) initialIntensity = objectLight.intensity;
    }

    void Update()
    {
        float intensityDecreaseRate = initialIntensity / fadeDuration;

        float newIntensity = objectLight.intensity - (intensityDecreaseRate * Time.deltaTime);
        if (flag && newIntensity < minIntensity)
        {
            newIntensity = minIntensity;
            objectLight.intensity = newIntensity;
            return;
        }

        newIntensity = Mathf.Clamp(newIntensity, 0f, initialIntensity);

        objectLight.intensity = newIntensity;

        
        if (newIntensity <= 0f)
        {
            Destroy(this.gameObject);
        }
    }

    public void UpdateIntensityConfig(float newMaxIntensity, float newMinIntensity)
    {
        if (objectLight == null) objectLight = GetComponent<Light>();

        initialIntensity = newMaxIntensity;
        objectLight.intensity = newMaxIntensity; 

        minIntensity = newMinIntensity;
        flag = true;
    }
}
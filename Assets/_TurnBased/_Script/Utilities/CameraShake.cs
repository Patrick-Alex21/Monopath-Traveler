using UnityEngine;


public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.1f;
    private float dampingSpeed = 1.0f;
    private float initialDuration = 0f;

    public void Shake(float duration, float magnitude)
    {
        originalPosition = transform.localPosition;
        shakeDuration = duration;
        initialDuration = duration;
        shakeMagnitude = magnitude;
    }

    private void LateUpdate()
    {
        if (shakeDuration > 0)
        {
            float strength = shakeMagnitude * (shakeDuration / initialDuration);
            transform.localPosition = originalPosition + Random.insideUnitSphere * strength;

            shakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            shakeDuration = 0f;
            transform.localPosition = originalPosition;
        }
    }
}
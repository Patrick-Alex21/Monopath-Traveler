using UnityEngine;
using Fungus;
using System.Collections;
using Fungus.DentedPixel;

namespace Fungus
{
    [CommandInfo("RPG Systems", 
            "Shake Camera", 
            "Shakes the target camera's transform for a specified duration using random offsets.")]
    public class CameraShake : Command
    {
        [Tooltip("The camera to shake. If left empty, it will automatically use Camera.main.")]
        [SerializeField] protected Camera targetCamera;

        [Tooltip("How long the shake lasts in seconds.")]
        [SerializeField] protected float duration = 0.5f;

        [Tooltip("How intense the shake is.")]
        [SerializeField] protected float magnitude = 0.2f;

        [Tooltip("If checked, the Flowchart will wait for the shake to finish before moving to the next command.")]
        [SerializeField] protected bool waitUntilFinished = false;

        public override void OnEnter()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                StartCoroutine(DoShake());
            }
            else
            {
                Continue();
            }

            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        protected virtual IEnumerator DoShake()
        {
            Vector3 originalPosition = targetCamera.transform.localPosition;
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                targetCamera.transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

                elapsed += Time.deltaTime;
                yield return null;
            }

            targetCamera.transform.localPosition = originalPosition;

            if (waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            string camName = targetCamera != null ? targetCamera.name : "Main Camera";
            return $"Shake {camName} for {duration}s";
        }

        public override Color GetButtonColor()
        {
            return new Color32(216, 228, 170, 255); 
        }
    }
}
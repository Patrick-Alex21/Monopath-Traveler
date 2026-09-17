using UnityEngine;
using Fungus.DentedPixel;

namespace Fungus
{

    [CommandInfo("Camera", 
                "Move Camera to Coordinates", 
                "Memindahkan dan memutar Main Camera ke target angka koordinat (X,Y,Z) secara mulus.")]
    public class MoveCameraCommand : Command
    {
        [Header("Target Location")]
        [Tooltip("Position Target (X, Y, Z) ")]
        [SerializeField] private Vector3 targetPosition;
        
        [Tooltip("Rotation Target (X, Y, Z) ")]
        [SerializeField] private Vector3 targetRotation;

        [Header("Animation Settings")]
        [Tooltip("Transform Duration")]
        [SerializeField] private float duration = 2f;

        [Tooltip("Wait until Transform is finished to continue")]
        [SerializeField] private bool waitUntilFinished = true;

        [Header("Hierarchy")]
        [Tooltip("Optional: Make New object as Child of this Transform")]
        [SerializeField] private Transform parentTransform;

        private Vector3 startPos;
        private Quaternion startRot;
        private Quaternion endRot;
        private float timer = 0f;
        private bool isMoving = false;

        public override void OnEnter()
        {
            if (Camera.main == null)
            {
                Continue();
                return;
            }

            startPos = Camera.main.transform.position;
            startRot = Camera.main.transform.rotation;
            
            endRot = Quaternion.Euler(targetRotation);
            
            timer = 0f;

            if (duration <= 0f)
            {
                Camera.main.transform.position = targetPosition;
                Camera.main.transform.rotation = endRot;
                Continue();
            }
            else
            {
                
                isMoving = true;
                if (!waitUntilFinished)
                {
                    Continue();
                }
            }
        }

        private void Update()
        {
            if (!isMoving) return;

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            
            t = t * t * (3f - 2f * t);

            if (Camera.main != null)
            {
                Camera.main.transform.position = Vector3.Lerp(startPos, targetPosition, t);
                Camera.main.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            }

            if (timer >= duration)
            {
                isMoving = false;
                if (waitUntilFinished)
                {
                    Continue(); 
                }
            }
        }

        public override string GetSummary()
        {
            return $"Move to Pos {targetPosition} over {duration}s";
        }

        public override Color GetButtonColor()
        {
            return new Color32(200, 150, 250, 255);
        }
    }
}
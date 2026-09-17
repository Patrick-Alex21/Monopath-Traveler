using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System;
using Fungus.DentedPixel;

namespace Fungus
{
    [CommandInfo("Movement", 
                "Move At Speed", 
                "Move to target with Character Controller")]

    public class MoveAtSpeed : Command
    {
        [Header("References")]
        [SerializeField] private Animator animator;

        [SerializeField] private CharacterController controller;

        [SerializeField] private Transform targetPosition;

        [Header("Settings")]
        [SerializeField] private float speed = 7f;

        [SerializeField] private bool waitUntilFinished = true;

        public override void OnEnter()
        {
            if (targetPosition == null || animator == null)
            {
                Continue(); 
                return;
            }

            StartCoroutine(MoveRoutine());
            
            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        private System.Collections.IEnumerator MoveRoutine()
        {
            Transform objToMove = controller != null ? controller.transform : animator.transform;
            
            Vector3 startPos = objToMove.position;
            Vector3 targetPos = targetPosition.position;
            targetPos.y = startPos.y; 

            Vector3 direction = (targetPos - startPos).normalized;
            float animX = 0f;
            float animY = 0f;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                animX = Mathf.Sign(direction.x);
                SpriteRenderer sr = animator.GetComponent<SpriteRenderer>();
                if (sr != null) sr.flipX = (animX > 0);
            }
            else
            {
                animY = Mathf.Sign(direction.z);
            }

            animator.SetFloat("AnimX", animX);
            animator.SetFloat("AnimY", animY);
            animator.SetBool("isWalking", true);

            
            float distance = Vector3.Distance(objToMove.position, targetPos);

            while (distance > 0.1f)
            {
                Vector3 moveDir = (targetPos - objToMove.position).normalized;
                
                if (controller != null)
                {
                    controller.Move(moveDir * speed * Time.deltaTime);
                }
                else
                {
                    objToMove.position = Vector3.MoveTowards(objToMove.position, targetPos, speed * Time.deltaTime);
                }

                distance = Vector3.Distance(objToMove.position, targetPos);
                yield return null; 
            }

            if (controller == null) objToMove.position = targetPos;

            animator.SetBool("isWalking", false);

            if (waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (targetPosition == null) return "Error: Target belum diisi!";
            return "Move to " + targetPosition.name + " at speed " + speed;
        }

        public override Color GetButtonColor()
        {
            return new Color32(200, 255, 200, 255); 
        }
    }
}
using UnityEngine;
using System.Collections;
using Fungus.DentedPixel; 

public class DamageFeedback : MonoBehaviour
{
    [Header("Character Identity")]
    [SerializeField] private CharacterType characterType = CharacterType.Heroes;

    [Header("Flash Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Pushback Settings")]
    [SerializeField] private float pushDistance = 0.5f;
    [SerializeField] private float pushDuration = 0.1f;

    private Color originalColor;
    private Coroutine flashCoroutine;
    private Vector3 calculatedPushOffset; 
    
    private SnapToGround snapToGround;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        
        snapToGround = GetComponent<SnapToGround>();

        if (characterType == CharacterType.Heroes)
            calculatedPushOffset = new Vector3(pushDistance, 0f, 0f); 
        else if (characterType == CharacterType.Enemies)
            calculatedPushOffset = new Vector3(-pushDistance, 0f, 0f); 
        else
            calculatedPushOffset = Vector3.zero; 
    }

    public void PlayHitReaction()
    {

        if (spriteRenderer != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        LeanTween.cancel(gameObject);
        
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + calculatedPushOffset;

        LeanTween.move(gameObject, targetPos, pushDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float val) => 
            { 
                if (snapToGround != null) snapToGround.Snap(); 
            })
            .setOnComplete(() =>
            {
                LeanTween.move(gameObject, startPos, pushDuration * 1.5f)
                    .setEase(LeanTweenType.easeInOutSine)
                    .setOnUpdate((float val) => 
                    { 
                        if (snapToGround != null) snapToGround.Snap(); 
                    });
            });
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}
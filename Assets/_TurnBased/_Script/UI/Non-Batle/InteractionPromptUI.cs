using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 8f;

    
    private float _targetAlpha = 0f;

    private void Awake()
    {
        
        canvasGroup.alpha = 0f;
        _targetAlpha = 0f;
    }

    private void Update()
    {
        if (Mathf.Approximately(canvasGroup.alpha, _targetAlpha)) return;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, _targetAlpha, fadeSpeed * Time.deltaTime);
    }

    public void ShowPrompt()
    {
        
        _targetAlpha = 1f;
    }

    public void HidePrompt()
    {
        
        _targetAlpha = 0f;
    }
}
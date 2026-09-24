using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    [Header("End Game UI")]
    [Tooltip("Requires a CanvasGroup component on the target UI panel.")]
    [SerializeField] private CanvasGroup endGameCanvasGroup;
    [SerializeField] private Button backToMainMenuButton;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnce = true;

    private bool _hasTriggered;

    private void Awake()
    {
        SetEndGameCanvasVisible(false, 0f); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && _hasTriggered) return;
        GameManager.Instance.ChangeState(GameState.Paused);

        _hasTriggered = true;
        ShowEndGameUI();
    }

    private void ShowEndGameUI()
    {
        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.RemoveAllListeners();
            backToMainMenuButton.onClick.AddListener(OnBackToMainMenuClicked);
        }

        // Start the smooth fade-in
        StartCoroutine(FadeCanvasGroup(endGameCanvasGroup, 0f, 1f, fadeDuration));
    }

    private void OnBackToMainMenuClicked()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    private void SetEndGameCanvasVisible(bool visible, float targetAlpha)
    {
        if (endGameCanvasGroup != null)
        {
            endGameCanvasGroup.alpha = targetAlpha;
            endGameCanvasGroup.interactable = visible;
            endGameCanvasGroup.blocksRaycasts = visible;
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        if (cg == null) yield break;

        cg.interactable = false;
        cg.blocksRaycasts = false;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        cg.alpha = endAlpha;

        if (endAlpha >= 1f)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }
}
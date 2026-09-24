using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SelectAllyUI : MonoBehaviour
{
    public event Action OnCancelRequested;

    [Header("Panel References")]
    [SerializeField] private Image backdropImage;

    [Header("Runtime State")]
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        SetVisible(false);
    }

        public void ShowAllyPanel()
    {
        BattleInputActions.Actions.Battle.Disable();
        SetVisible(true);
        if (backdropImage != null)
        {
            backdropImage.raycastTarget = false;
            StopAllCoroutines();
            StartCoroutine(EnableBackdropRaycastNextFrame());
        }
    }

    public void HideAllyPanel()
    {
        BattleInputActions.Actions.Battle.Enable();
        SetVisible(false);
        StopAllCoroutines();
        if (backdropImage != null) backdropImage.raycastTarget = false;
    }

    private IEnumerator EnableBackdropRaycastNextFrame()
    {
        yield return null;
        backdropImage.raycastTarget = true;
    }

    private void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    public void RequestCancel()
    {
        OnCancelRequested?.Invoke();
    }
    
}
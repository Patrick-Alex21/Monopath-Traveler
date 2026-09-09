using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SelectAllyUI : MonoBehaviour
{
    public event Action OnCancelRequested;
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
    }

    public void HideAllyPanel()
    {
        BattleInputActions.Actions.Battle.Enable();
        SetVisible(false);
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
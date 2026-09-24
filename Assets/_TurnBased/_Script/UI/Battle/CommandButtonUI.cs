using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


[RequireComponent(typeof(CanvasGroup))]
public class CommandButtonUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private CanvasGroup _panelCanvasGroup;
    [SerializeField] private Button _boostAllButton;
    [SerializeField] private Button _attackButton;

    private bool _isMaxBoostActive = false;

    private void Awake()
    {
        if (_panelCanvasGroup == null)
            _panelCanvasGroup = GetComponent<CanvasGroup>();

        SetVisible(false);

        if (_boostAllButton != null)
            _boostAllButton.onClick.AddListener(OnBoostAllClicked);

        if (_attackButton != null)
            _attackButton.onClick.AddListener(OnExecuteClicked);
    }

    private void OnBoostAllClicked()
    {
        _isMaxBoostActive = !_isMaxBoostActive;

        List<HeroCharBase> partyMembers = BattleManager.Instance.GetActiveHeroes();
        if (partyMembers == null) return;

        foreach (HeroCharBase hero in partyMembers)
        {
            int prevBoost = hero.AllocatedBoost;
            int newBoostLevel = _isMaxBoostActive ? Mathf.Min(hero.CurrentBP, 3) : 0;

            hero.ChangeBoostLevel(newBoostLevel);
        }

        if (BattleUIManager.Instance != null)
            BattleUIManager.Instance.RefreshAllBoostVisuals();
    }

    public void RefreshBoostAllButtonState()
    {
        if (_boostAllButton == null) return;

        bool canAnyoneBoost = false;
        List<HeroCharBase> activeHeroes = BattleManager.Instance.GetActiveHeroes();

        foreach (HeroCharBase hero in activeHeroes)
        {
            if (hero.AllocatedBoost < BattleManager.MAX_BOOST && hero.AllocatedBoost < hero.CurrentBP)
            {
                canAnyoneBoost = true;
                break;
            }
        }

        _boostAllButton.interactable = canAnyoneBoost;
    }
    private void OnExecuteClicked()
    {
        BattleManager.Instance.ExecuteAllHeroesActions();
    }

    public void ShowCommands()
    {
        _isMaxBoostActive = false;
        SetVisible(true);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void HideCommands()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (_panelCanvasGroup == null) return;
        _panelCanvasGroup.alpha = visible ? 1f : 0f;
        _panelCanvasGroup.interactable = visible;
        _panelCanvasGroup.blocksRaycasts = visible;
    }
}
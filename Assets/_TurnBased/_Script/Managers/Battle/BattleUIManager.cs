using System.Collections.Generic;
using UnityEngine;

public class BattleUIManager : Singleton<BattleUIManager>
{
    [Header("Menu Panels")]
    [SerializeField] private ActionMenuUI actionMenuPanel; 
    [SerializeField] private CommandButtonUI commandButtonPanel; 
    [SerializeField] private BattleResultUI battleResultUI;
    [SerializeField] private TurnQueueUI turnQueueUI;

    [Header("Hero Stats Panels")]
    [SerializeField] private List<HeroStatUI> heroStatPanels;

    [Header("UI Transition Settings (Fade)")]
    [SerializeField] private CanvasGroup[] battleUIGroups;

    public void SetupPartyUI(List<ScriptableHero> partyData, List<HeroCharBase> physicalUnits)
    {
        for (int i = 0; i < heroStatPanels.Count; i++)
        {
            if (i < partyData.Count && i < physicalUnits.Count)
            {
                heroStatPanels[i].Init(partyData[i], physicalUnits[i]);
                heroStatPanels[i].ShowPanel(); 
            }
            else
            {
                heroStatPanels[i].HidePanel(); 
            }
        }
    }

    public void BuildTurnQueue(List<CharacterBase> currentRound)
    {
        if (turnQueueUI != null) turnQueueUI.BuildQueue(currentRound);
    }

    public void SkipTurnQueue(CharacterBase character)
    {
        if (turnQueueUI != null) turnQueueUI.SkipTurn(character);
    }

    public void RemoveFromTurnQueue(CharacterBase character)
    {
        if (turnQueueUI != null) turnQueueUI.RemoveFromQueue(character);
    }

    public void BeginExecutionQueueVisual()
    {
        if (turnQueueUI != null) turnQueueUI.BeginExecution();
    }

    public void RefreshAllBoostVisuals()
    {
        foreach (HeroStatUI panel in HeroStatUI.ActivePanels)
        {
            if (panel != null && panel.heroChar != null)
                panel.UpdateBoostVisual();
        }
    }

    public void ShowCommandPanel() => commandButtonPanel?.ShowCommands();
    public void HideCommandPanel() => commandButtonPanel?.HideCommands();
    public void CloseActionMenu() => actionMenuPanel?.CloseMenu();

    public void ShowVictoryScreen() 
    {
        HideAllBattleUI();
        if (battleResultUI != null) battleResultUI.ShowVictory();
    }

    public void ShowDefeatScreen() 
    {
        HideAllBattleUI();
        if (battleResultUI != null) battleResultUI.ShowDefeat();
    }

    public void HideAllBattleUI()
    {
        foreach (CanvasGroup cg in battleUIGroups)
        {
            if (cg != null) SetGroupVisible(cg, false);
        }

        foreach (HeroStatUI panel in heroStatPanels)
        {
            if (panel != null) panel.HidePanel();
        }
    }
    public void ShowAllForTransition()
    {
        foreach (CanvasGroup cg in battleUIGroups)
        {
            if (cg != null) SetGroupVisible(cg, true);
        }

        foreach (HeroStatUI panel in heroStatPanels)
        {
            if (panel != null && panel.heroChar != null) panel.ShowPanel();
        }
    }

    public void RefreshAllIntentTexts()
    {
        foreach (HeroStatUI panel in heroStatPanels)
        {
            if (panel == null || panel.heroChar == null) continue;

            ScriptableSkill chosen = panel.heroChar.CurrentIntent.ChosenSkill;
            panel.SetIntentText(chosen != null ? chosen.skillName : "");
        }
    }

    private static void SetGroupVisible(CanvasGroup cg, bool visible)
    {
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }
}
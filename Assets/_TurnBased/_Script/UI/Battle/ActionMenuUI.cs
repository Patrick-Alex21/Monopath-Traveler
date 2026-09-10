using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class ActionMenuUI : MonoBehaviour
{
    public ScriptableHero CurrentHero { get; private set; }
    public HeroCharBase CurrentHeroUnit { get; private set; }
    public static ActionMenuUI Instance;
    private HeroStatUI casterPanel;

    [Header("Menu Setup")]
    [SerializeField] private Transform commandHolder; 
    [SerializeField] private GameObject skillButtonPrefab; 
    [SerializeField] private Image portraitImageUI;
    [SerializeField] private TextMeshProUGUI charNameText;
    [SerializeField] private CanvasGroup commandHolderGroup;

    [Header("Animation Settings")]
    [SerializeField] private Vector2 hiddenPosition = new Vector2(500, 0); 
    [SerializeField] private Vector2 visiblePosition = new Vector2(0, 0); 
    [SerializeField] private float animationDuration = 0.3f;

    [Header("Boost System")]
    private int currentBoost = 0;
    [SerializeField] private TextMeshProUGUI boostMultiplierText;
    [SerializeField] private BoostVFXManager boostVFX;

    [Header("UI Navigation")]
    private GameObject firstSelectedButton;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private PlayerInputAction _actions;

    [SerializeField] private SelectAllyUI selectAllyPanel;

    [Header("Content Animation")]
    [SerializeField] private RectTransform _dynamicContentRect;
    [SerializeField] private CanvasGroup _dynamicContentCanvasGroup; 
    [SerializeField] private float _slideOffset = 30f;
    [SerializeField] private float _swapAnimDuration = 0.15f;

    private float _originalContentX;
    private bool isMenuActive = false;
    private List<Button> _activeSkillButtons = new List<Button>();
    private ScriptableSkill selectedSkill;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (selectAllyPanel != null) selectAllyPanel.OnCancelRequested += CancelAllySelection;

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        
        rectTransform.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (_dynamicContentRect != null)
            _originalContentX = _dynamicContentRect.anchoredPosition.x;
    }

    private void OnEnable() => BattleInputActions.Actions.Battle.Boost.performed += OnBoostPerformed;
    private void OnDisable() => BattleInputActions.Actions.Battle.Boost.performed -= OnBoostPerformed;
 
    public void OpenMenuForHero(ScriptableHero hero, HeroStatUI sourcePanel)
    {
        if (isMenuActive && CurrentHero == hero)
        {
            if (casterPanel != null) casterPanel.SetHighlighted(false);
            CloseMenu();
            return;
        }

        if (isMenuActive && casterPanel != null)
        {
            casterPanel.SetHighlighted(false);
            if (BattleManager.Instance != null)
                BattleManager.Instance.StopTargetingFromMenu();
        }

        sourcePanel.SetHighlighted(true);

        CurrentHero = hero;
        casterPanel = sourcePanel;

        if (charNameText != null) charNameText.text = hero.heroName;
        CurrentHeroUnit = sourcePanel.heroChar;
        SyncBoostStateFromHero();

        ScriptableSkill currentSkill = CurrentHeroUnit.CurrentIntent.ChosenSkill;
        bool needsSingleTarget = currentSkill != null && !IsFriendlyTargetSkill(currentSkill) && currentSkill.targetScope == TargetScope.Single;
        if (needsSingleTarget && BattleManager.Instance != null)
            BattleManager.Instance.StartTargetingForHero(CurrentHeroUnit);

        StopAllCoroutines();
        StartCoroutine(SetupAndShowMenuCoroutine());
    }

    public void OpenMenu()
    {
        BattleInputActions.Actions.Battle.Enable();
        isMenuActive = true;
        SyncBoostStateFromHero();

        if (boostMultiplierText != null)
            boostMultiplierText.text = $"x{currentBoost + 1}";

        if (firstSelectedButton != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);

        StopAllCoroutines();
        StartCoroutine(AnimateMenu(visiblePosition, 1f, true));
    }

    public void CloseMenu()
    {
        BattleInputActions.Actions.Battle.Disable(); 
        isMenuActive = false;

        if (BattleManager.Instance != null)
            BattleManager.Instance.StopTargetingFromMenu();

        if (casterPanel != null)
        {
            casterPanel.SetHighlighted(false);
            casterPanel = null;
        }

        SetAllySelectionMode(false);
        SetCommandHolderInteractable(true);

        BattleUIManager.Instance.ShowCommandPanel();

        StopAllCoroutines();
        StartCoroutine(AnimateMenu(hiddenPosition, 0f, false));
    }

    private IEnumerator SetupAndShowMenuCoroutine()
    {
        SetCommandHolderInteractable(false);

        commandHolder.DestroyChilderns();
        yield return new WaitForEndOfFrame();

        firstSelectedButton = null;
        GameObject buttonToSelect = null;
        ScriptableSkill firstAffordableSkill = null;

        _activeSkillButtons.Clear();

        if (CurrentHero != null)
        {
            if (portraitImageUI != null) portraitImageUI.sprite = CurrentHero.MenuSprite;

            string lastIntent = casterPanel != null ? casterPanel.IntentTextValue : "";

            List<Button> spawnedButtons = new List<Button>();

            if (CurrentHero.skills != null)
            {
                foreach (ScriptableSkill skill in CurrentHero.skills)
                {
                    GameObject newBtn = Instantiate(skillButtonPrefab, commandHolder);
                    
                    SkillButtonUI ui = newBtn.GetComponent<SkillButtonUI>();
                    if (ui != null) ui.Setup(skill, CurrentHeroUnit.currentSp, CurrentHeroUnit.CurrentIntent.Target as EnemyBase);

                    Button btnComp = newBtn.GetComponent<Button>();
                    if (btnComp != null)
                    {
                        if (CurrentHeroUnit.currentSp < skill.spCost)
                        {
                            btnComp.interactable = false; 
                        }
                        else
                        {
                            btnComp.interactable = true;
                            btnComp.onClick.AddListener(() => OnSkillClicked(skill, btnComp)); 
                            _activeSkillButtons.Add(btnComp); 
                            
                            if (firstSelectedButton == null)
                            {
                                firstSelectedButton = newBtn;
                                firstAffordableSkill = skill;
                            }
                            if (skill.skillName == lastIntent) buttonToSelect = newBtn; 
                        }

                        spawnedButtons.Add(btnComp);
                    }
                }
            }

            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                Navigation customNav = new Navigation();
                customNav.mode = Navigation.Mode.Explicit;
                customNav.selectOnUp   = spawnedButtons[i == 0 ? spawnedButtons.Count - 1 : i - 1];
                customNav.selectOnDown = spawnedButtons[i == spawnedButtons.Count - 1 ? 0 : i + 1];
                spawnedButtons[i].navigation = customNav;
            }

            if (_dynamicContentRect != null) 
                LayoutRebuilder.ForceRebuildLayoutImmediate(_dynamicContentRect);
            
            SetCommandHolderInteractable(true);
            
            if (!isMenuActive)
            {
                OpenMenu();
            }
            else
            {
                StartCoroutine(PlaySwapAnimation());
                SyncBoostStateFromHero();
                if (boostMultiplierText != null) boostMultiplierText.text = $" x{currentBoost + 1}";
            }
            
            EventSystem.current.SetSelectedGameObject(null); 
            
            if (buttonToSelect != null)
            {
                EventSystem.current.SetSelectedGameObject(buttonToSelect);
                buttonToSelect.GetComponent<Button>().Select();
            }
            else if (firstSelectedButton != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
                firstSelectedButton.GetComponent<Button>().Select(); 
                
                if (firstAffordableSkill != null)
                {
                    selectedSkill = firstAffordableSkill;
                    CurrentHeroUnit.CurrentIntent.ChosenSkill = firstAffordableSkill;
                    if (casterPanel != null) casterPanel.SetIntentText(firstAffordableSkill.skillName);
                }
            }
        }
    }

    private void OnSkillClicked(ScriptableSkill clickedSkill, Button clickedButton)
    {
        if (selectedSkill == clickedSkill)
        {
            if (selectAllyPanel != null) selectAllyPanel.HideAllyPanel();
            CloseMenu();
            return;
        }

        this.selectedSkill = clickedSkill;

        if (casterPanel != null) casterPanel.SetIntentText(clickedSkill.skillName);
        if (CurrentHeroUnit != null)
        {
            CurrentHeroUnit.CurrentIntent.ChosenSkill = clickedSkill;
            if (clickedSkill.targetScope == TargetScope.Self)
                CurrentHeroUnit.CurrentIntent.AllyTarget = CurrentHeroUnit;
        }

        if (IsFriendlyTargetSkill(clickedSkill))
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.StopTargetingWithoutApplyingTarget();

            if (clickedSkill.targetScope == TargetScope.Single)
            {
                SetAllySelectionMode(true);
                SetCommandHolderInteractable(false);
                BattleUIManager.Instance.HideCommandPanel();
            }
            else
            {
                if (selectAllyPanel != null) selectAllyPanel.HideAllyPanel();
                CloseMenu();
            }
        }
        else if (CurrentHeroUnit != null && BattleManager.Instance != null)
        {
            if (selectAllyPanel != null) selectAllyPanel.HideAllyPanel();
            SetCommandHolderInteractable(true);
            BattleUIManager.Instance.ShowCommandPanel();

            if (clickedSkill.targetScope == TargetScope.All)
            {
                BattleManager.Instance.StopTargetingWithoutApplyingTarget();
                CloseMenu();
            }
            else
            {
                if (BattleManager.Instance.State != BattleState.SelectTarget)
                {
                    BattleManager.Instance.StartTargetingForHero(CurrentHeroUnit);
                }
                CloseMenu();
            }
        }
    }

    public bool TrySelectFriendlyTargetFromPanel(HeroStatUI targetPanel)
    {
        if (!isMenuActive || selectedSkill == null || !IsFriendlyTargetSkill(selectedSkill)) return false;
        if (selectedSkill.targetScope != TargetScope.Single) return false;
        if (CurrentHeroUnit == null || targetPanel == null || targetPanel.heroChar == null) return false;

        CurrentHeroUnit.CurrentIntent.ChosenSkill = selectedSkill;
        CurrentHeroUnit.CurrentIntent.AllyTarget = targetPanel.heroChar;

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.ApplyAllyTargetForHero(CurrentHeroUnit, targetPanel.heroChar);
            BattleManager.Instance.StopTargetingWithoutApplyingTarget();
        }

        if (casterPanel != null && casterPanel != targetPanel)
            casterPanel.SetHighlighted(false);

        targetPanel.SetHighlighted(false);

        SetAllySelectionMode(false);

        CloseMenu();
        casterPanel = null;
        return true;
    }

    private bool IsFriendlyTargetSkill(ScriptableSkill skill)
    {
        return skill != null &&
            (skill.skillCategory == SkillCategory.Recovery ||  skill.skillCategory == SkillCategory.Augment);
    }

    private void CancelAllySelection()
    {
        if (!isMenuActive) return;

        SetAllySelectionMode(false);
        selectedSkill = null;

        if (casterPanel != null) casterPanel.SetHighlighted(false);
    }

    private void OnBoostPerformed(InputAction.CallbackContext ctx)
    {
        if (!isMenuActive) return;

        float boostValue = ctx.ReadValue<float>();
        int prevBoost = currentBoost;

        if (boostValue > 0)
        {
            if (currentBoost < BattleManager.MAX_BOOST && CurrentHeroUnit != null && currentBoost < CurrentHeroUnit.CurrentBP)
            {
                int targetBoost = currentBoost + 1;
                if (CurrentHeroUnit != null)
                    CurrentHeroUnit.ChangeBoostLevel(targetBoost);
                currentBoost = CurrentHeroUnit != null ? CurrentHeroUnit.AllocatedBoost : 0;
                UpdateBoostSystem(prevBoost);
            }
        }
        else if (boostValue < 0)
        {
            if (currentBoost > 0)
            {
                int targetBoost = currentBoost - 1;
                if (CurrentHeroUnit != null)
                    CurrentHeroUnit.ChangeBoostLevel(targetBoost);
                currentBoost = CurrentHeroUnit != null ? CurrentHeroUnit.AllocatedBoost : 0;
                UpdateBoostSystem(prevBoost);
            }
        }
    }

    private void SyncBoostStateFromHero()
    {
        currentBoost = CurrentHeroUnit != null ? CurrentHeroUnit.AllocatedBoost : 0;
        UpdateBoostSystem(currentBoost);
    }

    private void UpdateBoostSystem(int prevBoost)
    {
        if (boostMultiplierText != null) 
        {
            Color boostColor = Color.white; 
            if (currentBoost == 1) boostColor = Color.red; 
            else if (currentBoost == 2) boostColor = Color.yellow; 
            else if (currentBoost == 3) boostColor = Color.cyan; 

            boostMultiplierText.color = boostColor;
            boostMultiplierText.text = currentBoost >= 3 ? "MAX" : $"x{currentBoost + 1}";
        }

        if (casterPanel != null) casterPanel.UpdateBoostVisual();
    }


    private void SetCommandHolderInteractable(bool interactable)
    {
        if (commandHolderGroup == null) return;
        commandHolderGroup.interactable = interactable;
        commandHolderGroup.blocksRaycasts = interactable;
    }

    private IEnumerator PlaySwapAnimation()
    {
        if (_dynamicContentCanvasGroup == null || _dynamicContentRect == null) yield break;

        float elapsed = 0f;
        float startX = _originalContentX + _slideOffset;
        _dynamicContentRect.anchoredPosition = new Vector2(startX, _dynamicContentRect.anchoredPosition.y);
        _dynamicContentCanvasGroup.alpha = 0f;

        while (elapsed < _swapAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _swapAnimDuration;
            float easeOutT = t * (2f - t); 

            float currentX = Mathf.Lerp(startX, _originalContentX, easeOutT);
            _dynamicContentRect.anchoredPosition = new Vector2(currentX, _dynamicContentRect.anchoredPosition.y);
            _dynamicContentCanvasGroup.alpha = Mathf.Lerp(0f, 1f, easeOutT);
            
            yield return null;
        }

        _dynamicContentRect.anchoredPosition = new Vector2(_originalContentX, _dynamicContentRect.anchoredPosition.y);
        _dynamicContentCanvasGroup.alpha = 1f;
    }

    private void SetAllySelectionMode(bool active)
    {
        if (selectAllyPanel != null)
        {
            if (active) selectAllyPanel.ShowAllyPanel();
            else selectAllyPanel.HideAllyPanel();
        }

        if (BattleManager.Instance != null && BattleManager.Instance.TargetingSystem != null)
        {
            BattleManager.Instance.TargetingSystem.blockWorldClick = active;
        }

        SetCommandHolderInteractable(!active);

        if (active) BattleUIManager.Instance.HideCommandPanel();
        else BattleUIManager.Instance.ShowCommandPanel();
    }

    private IEnumerator AnimateMenu(Vector2 targetPos, float targetAlpha, bool interactable)
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / animationDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
        canvasGroup.alpha = targetAlpha;
    }
}
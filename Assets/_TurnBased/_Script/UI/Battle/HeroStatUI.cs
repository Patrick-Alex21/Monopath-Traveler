using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class HeroStatUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI spText;
    [SerializeField] private TextMeshProUGUI intentText;
    [SerializeField] private Image[] boostPoints;
    [SerializeField] private Image uiBG;
    [SerializeField] private Color normalColor  = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("HP / SP Bar")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Image spFillImage;

    [SerializeField] private AudioClip selectSound;

    public static readonly List<HeroStatUI> ActivePanels = new List<HeroStatUI>();

    public string IntentTextValue => intentText != null ? intentText.text : "";
    public HeroCharBase heroChar { get; private set; }

    private ScriptableHero myHero;
    private bool isSelected= false;
    private bool isVisible = false;
    private Button myButton;
    private CanvasGroup canvasGroup;


    private void Awake()
    {
        myButton = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();
        myButton.onClick.AddListener(OnStatPanelClicked);

        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (heroChar == null) return;

        heroChar.OnHealthChanged -= UpdateHPVisuals;
        heroChar.OnSpChanged -= UpdateSPVisuals;
    }


    public void ShowPanel()
    {
        if (isVisible) return;
        isVisible = true;
        SetVisible(true);
        ActivePanels.Add(this);
    }

    public void HidePanel()
    {
        if (!isVisible) return;
        isVisible = false;
        SetVisible(false);
        ActivePanels.Remove(this);
    }

    private void SetVisible(bool visible)
    {
        canvasGroup.alpha  = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    public void Init(ScriptableHero heroData, HeroCharBase heroUnit)
    {
        myHero = heroData;
        heroChar = heroUnit;

        if (heroChar == null) return;

        heroChar.AllocatedBoost = 0;

        nameText.text = heroData.heroName;
        hpText.text = heroData.BaseStats.maxHp.ToString();
        spText.text = heroData.BaseStats.maxSp.ToString();

        heroChar.OnHealthChanged += UpdateHPVisuals;
        heroChar.OnSpChanged += UpdateSPVisuals;

        UpdateHPVisuals(heroChar.currentHp, heroChar.Stats.maxHp);
        UpdateSPVisuals(heroChar.currentSp, heroChar.Stats.maxSp);
        UpdateBoostVisual();
    }

    public void SetIntentText(string newIntent)
    {
        if (intentText != null) intentText.text = newIntent;
    }

    public void UpdateBoostVisual()
    {
        int actualBP = heroChar.CurrentBP; 
        int usedBoost = heroChar.AllocatedBoost;
        int remainingBP = actualBP - usedBoost;

        for (int i = 0; i < boostPoints.Length; i++)
            boostPoints[i].color = (i < remainingBP) ? Color.yellow : Color.gray;
    }

    public void OnHoverEnter()
    {
        if (!isSelected && uiBG != null)
            uiBG.color = selectedColor;
    }

    public void OnHoverExit()
    {
        if (!isSelected && uiBG != null)
            uiBG.color = normalColor;
    }

    public void SetHighlighted(bool locked)
    {
        isSelected = locked;
        if (uiBG != null)
            uiBG.color = isSelected ? selectedColor : normalColor;
    }

    private void OnStatPanelClicked()
    {
        if (myHero == null) return;

        if (BattleManager.Instance != null && BattleManager.Instance.State == BattleState.ExecutingTurn)
            return;

        if (AudioSystem.Instance != null && selectSound != null)
            AudioSystem.Instance.PlayUISound(selectSound);

        if (ActionMenuUI.Instance != null && ActionMenuUI.Instance.TrySelectFriendlyTargetFromPanel(this))
        {
            return;
        }

        if (ActionMenuUI.Instance != null)
        {
            ActionMenuUI.Instance.OpenMenuForHero(myHero, this);
        }

        heroChar.PlayVoice(VoiceType.MyTurn);
    }

    private void UpdateHPVisuals(int currentHp, int maxHp)
    {
        if (hpText != null)
            hpText.text = currentHp.ToString(); 

        if (hpFillImage != null)
        {
            float pct = (float)currentHp / maxHp; 
            hpFillImage.fillAmount = pct; 
            
            if(pct > 0.5f)  hpFillImage.color = Color.green; 
            else if (pct > 0.25f) hpFillImage.color = new Color(0.8f, 0.7f, 0f); 
            else hpFillImage.color = Color.red; 
        }
    }

    private void UpdateSPVisuals(int currentSp, int maxSp)
    {
        if (spText != null) spText.text = currentSp.ToString(); 
        if (spFillImage != null) spFillImage.fillAmount = (float)currentSp / maxSp; 
    }
}
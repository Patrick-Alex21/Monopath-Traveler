using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

//ISelectHandler if use selectable navigation (keyboard/controller) to show description box on select

[RequireComponent(typeof(Button))]
public class SkillButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler, ISelectHandler
{
    [Header("Skill Info")]
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI spText;
    [SerializeField] private Image iconSprite;

    [Header("Description Box")]
    [SerializeField] private CanvasGroup descriptionBoxGroup;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Weakness Match Glow (tanpa sprite)")]
    [Tooltip("Warna icon pas weakness match. Kosongin iconSprite.color balik normal kalau gak match.")]
    [SerializeField] private Color normalIconColor = Color.white;
    [SerializeField] private Color weaknessGlowColor = new Color(1f, 0.85f, 0.3f); 
    [Tooltip("Opsional: komponen Outline bawaan Unity UI (Add Component > UI > Effects > Outline) di object yang sama dengan iconSprite. Kalau diisi, bakal ikut berkilau (pulsing).")]
    [SerializeField] private Outline weaknessOutline;
    [SerializeField] private bool pulseOutline = true;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private Color outlineBaseColor = new Color(1f, 0.85f, 0.3f, 0.4f);
    [SerializeField] private Color outlinePeakColor = new Color(1f, 0.95f, 0.6f, 1f);

    [Header("SP Container")]
    [SerializeField] private CanvasGroup spContainerGroup;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip clickSound;

    [Header("Runtime State")]
    private static readonly List<SkillButtonUI> _instances = new List<SkillButtonUI>();
    private static SkillButtonUI hoveredButton;
    private Button myButton;
    private ScriptableSkill mySkill;
    private Coroutine pulseCoroutine;
    private bool isSelected;

    private void Awake()
    {
        myButton = GetComponent<Button>();
        if (myButton != null)
            myButton.onClick.AddListener(OnButtonClicked);

        SetDescriptionVisible(false);
        _instances.Add(this);
    }

    public void Setup(ScriptableSkill skill, int currentSp, EnemyBase currentTarget = null)
    {
        mySkill = skill;
        SetDescriptionVisible(false);
        if (skill == null) return;

        if (typeText != null) typeText.text= $"{skill.targetScope} / {skill.skillCategory}";
        if (nameText != null) nameText.text = skill.skillName;
        if (descriptionText != null) descriptionText.text = skill.description;

        if (iconSprite != null)
        {
            iconSprite.enabled = skill.skillElement != null;
            if (skill.skillElement != null)
                iconSprite.sprite = skill.skillElement.elementIcon;
        }

        bool hasSPCost = skill.spCost > 0;
        SetGroupVisible(spContainerGroup, hasSPCost);

        bool canAfford = currentSp >= skill.spCost;

        if (hasSPCost && spText != null)
        {
            spText.text  = $"SP {skill.spCost}";
            spText.color = canAfford ? Color.white : Color.red; 
        }

        if (myButton != null)
            myButton.interactable = canAfford;

        bool isKnownWeakness = skill.skillElement != null
            && currentTarget != null
            && currentTarget.DiscoveredWeaknesses.Contains(skill.skillElement);

        SetWeaknessGlow(isKnownWeakness);
    }

    private void SetWeaknessGlow(bool active)
    {
        if (iconSprite != null)
            iconSprite.color = active ? weaknessGlowColor : normalIconColor;

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        if (weaknessOutline == null) return;

        if (active && pulseOutline)
        {
            pulseCoroutine = StartCoroutine(PulseOutline());
        }
        else
        {
            weaknessOutline.effectColor = active ? outlinePeakColor : new Color(0, 0, 0, 0);
        }
    }

    private IEnumerator PulseOutline()
    {
        while (true)
        {
            float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f; 
            weaknessOutline.effectColor = Color.Lerp(outlineBaseColor, outlinePeakColor, t);
            yield return null;
        }
    }

    private void OnButtonClicked()
    {
        if (AudioSystem.Instance != null && clickSound != null)
            AudioSystem.Instance.PlayUISound(clickSound);
    }

    private void OnDestroy()
    {
        if (myButton != null)
            myButton.onClick.RemoveListener(OnButtonClicked);
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);

        _instances.Remove(this);
        isSelected = false;
        if (hoveredButton == this) hoveredButton = null;
        RefreshActiveSkillVisuals();
    }

        public void OnPointerEnter(PointerEventData eventData)
    {
        hoveredButton = this;
        RefreshActiveSkillVisuals();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoveredButton == this) hoveredButton = null;
        RefreshActiveSkillVisuals();
    }

    public void OnSelect(BaseEventData eventData)
    {
        foreach (SkillButtonUI button in _instances)
        {
            button.isSelected = false;
        }

        isSelected = true;
        RefreshActiveSkillVisuals();
    }

    public void OnDeselect(BaseEventData eventData)
    {
            // isSelected = false;
            // RefreshActiveSkillVisuals();
    }

    private bool IsEnemyTargetingAllSkill()
    {
        return mySkill != null
            && mySkill.targetScope == TargetScope.All
            && mySkill.skillCategory != SkillCategory.Recovery
            && mySkill.skillCategory != SkillCategory.Augment;
    }

    private bool IsEnemyTargetingSingleSkill()
    {
        return mySkill != null
            && mySkill.targetScope == TargetScope.Single
            && mySkill.skillCategory != SkillCategory.Recovery
            && mySkill.skillCategory != SkillCategory.Augment;
    }

    private static void RefreshActiveSkillVisuals()
    {
        SkillButtonUI active = hoveredButton;
        if (active == null)
            active = _instances.Find(b => b.isSelected);

        foreach (SkillButtonUI b in _instances)
            b.SetDescriptionVisible(b == active && b.mySkill != null);

        TargetingSystem targetingSystem = BattleManager.Instance != null ? BattleManager.Instance.TargetingSystem : null;
        if (targetingSystem == null) return;

        if (active != null && active.IsEnemyTargetingAllSkill())
            targetingSystem.PreviewAllEnemies();
        else if (active != null && active.IsEnemyTargetingSingleSkill())
            targetingSystem.PreviewSingleTarget();
        else
            targetingSystem.ClearPreview();
    }

    private void SetDescriptionVisible(bool visible)
        => SetGroupVisible(descriptionBoxGroup, visible);

    private static void SetGroupVisible(CanvasGroup cg, bool visible)
    {
        if (cg == null) return;
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }
}
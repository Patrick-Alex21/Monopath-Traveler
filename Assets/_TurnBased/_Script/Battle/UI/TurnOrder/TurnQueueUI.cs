using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Fungus.DentedPixel;


public class TurnQueueUI : MonoBehaviour
{
    [Header("Slot Setup")]
    [SerializeField] private RectTransform slotContainer;
    [SerializeField] private GameObject turnIconPrefab;

    [Header("Layout")]
    [SerializeField] private float slotSpacing = 60f;
    [SerializeField] private float smallScale = 0.6f;
    [SerializeField] private float bigScale = 1f;

    [Header("Next Turn pool (off-screen right)")]
    [SerializeField] private float nextTurnStartX = 500f;
    [SerializeField] private float slideDuration = 0.3f;

    [Header("Runtime State")]
    private readonly List<TurnIconSlot> _queue = new List<TurnIconSlot>();
    private bool _isExecuting = false;

    private class TurnIconSlot
    {
        public CharacterBase character;
        public RectTransform rect;
    }

    private bool _hasBuiltOnce = false;

    public void BuildQueue(List<CharacterBase> currentRound)
    {
        ClearQueue();
        _isExecuting = false;

        if (currentRound == null) return;

        for (int i = 0; i < currentRound.Count; i++)
        {
            if (currentRound[i] == null) continue;

            if (_hasBuiltOnce)
                SpawnIcon(currentRound[i], i);
            else
                SpawnIconInstant(currentRound[i], i);
        }

        _hasBuiltOnce = true;
    }

    private void SpawnIconInstant(CharacterBase character, int slotIndex)
    {
        GameObject obj = Instantiate(turnIconPrefab, slotContainer);
        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(slotIndex * slotSpacing, 0);
        rect.localScale = Vector3.one * ScaleForSlot(slotIndex);

        TurnIconVisual visual = obj.GetComponent<TurnIconVisual>();
        if (visual != null) visual.Setup(character);

        _queue.Add(new TurnIconSlot { character = character, rect = rect });
    }

    private void SpawnIcon(CharacterBase character, int slotIndex)
    {
        GameObject obj = Instantiate(turnIconPrefab, slotContainer);
        RectTransform rect = obj.GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(nextTurnStartX, 0);
        rect.localScale = Vector3.one * smallScale;

        TurnIconVisual visual = obj.GetComponent<TurnIconVisual>();
        if (visual != null) visual.Setup(character);

        _queue.Add(new TurnIconSlot { character = character, rect = rect });

        AnimateToSlot(rect, slotIndex);
    }

    private float ScaleForSlot(int slotIndex)
    {
        return (_isExecuting && slotIndex == 0) ? bigScale : smallScale;
    }

    public void BeginExecution()
    {
        _isExecuting = true;
        RefreshAllSlotScales();
    }

    private void RefreshAllSlotScales()
    {
        for (int i = 0; i < _queue.Count; i++)
        {
            LeanTween.scale(_queue[i].rect, Vector3.one * ScaleForSlot(i), slideDuration);
        }
    }

    public void SkipTurn(CharacterBase character)
    {
        TurnIconSlot slot = _queue.FirstOrDefault(s => s.character == character);
        if (slot == null) return;

        TurnIconVisual visual = slot.rect.GetComponent<TurnIconVisual>();
        if (visual != null) visual.SetDimmed(true);
    }

    public void RemoveFromQueue(CharacterBase character)
    {
        TurnIconSlot slot = _queue.FirstOrDefault(s => s.character == character);
        if (slot == null) return;

        _queue.Remove(slot);

        LeanTween.moveX(slot.rect, -slotSpacing, slideDuration);
        LeanTween.scale(slot.rect, Vector3.zero, slideDuration)
            .setOnComplete(() =>
            {
                if (slot.rect != null)
                    Destroy(slot.rect.gameObject);
            });

        for (int i = 0; i < _queue.Count; i++)
        {
            AnimateToSlot(_queue[i].rect, i);
        }
    }

    private void AnimateToSlot(RectTransform rect, int slotIndex)
    {
        LeanTween.moveX(rect, slotIndex * slotSpacing, slideDuration);
        LeanTween.scale(rect, Vector3.one * ScaleForSlot(slotIndex), slideDuration);
    }

    private void ClearQueue()
    {
        foreach (var slot in _queue)
        {
            if (slot.rect != null)
                Destroy(slot.rect.gameObject);
        }

        _queue.Clear();
    }
}
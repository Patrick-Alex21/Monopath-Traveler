using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mouse = UnityEngine.InputSystem.Mouse;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;

public class TargetingSystem : MonoBehaviour
{
    public event Action<CharacterBase> OnTargetConfirmed;
    public event Action OnTargetCanceled;

    [Header("Visuals (Action Menu Mode)")]
    [SerializeField] private GameObject arrowIndicatorPrefab; 
    private GameObject currentArrow;
    private readonly List<GameObject> _allTargetArrows = new List<GameObject>();
    private readonly Dictionary<CharacterBase, GameObject> _allTargetArrowByEnemy = new Dictionary<CharacterBase, GameObject>();
    private CharacterBase _allTargetMain;
    private bool _showingAllTargetIndicators = false;
    private Renderer[] _arrowRenderers;
    
    [Header("Target Data")]
    [SerializeField] private GameObject currentTarget;  
    
    [Header("Settings")]
    [SerializeField] private float autoHideDelay = 3.0f;

    [Header("Targeting Audio")]
    [SerializeField] private AudioClip confirmTargetSound;
    [SerializeField] private Vector3 arrowFallbackOffset = new Vector3(0, 1.3f, 0);

    [Header("Dynamic Arrow Size")]
    [SerializeField] private float arrowHeightGap = 0f;
    [SerializeField, Min(0.1f)] private float arrowWidthMultiplier = 1.8f;
    [SerializeField, Min(0.01f)] private float arrowMinWidth = 0.6f;
    [SerializeField, Min(0.01f)] private float arrowMaxWidth = 4f;
    [SerializeField] private Color mainTargetColor = new Color(1f, 0.75f, 0.15f, 1f);
    [SerializeField] private Color splashTargetColor = new Color(0.35f, 0.9f, 1f, 1f);

    private int currentTargetIndex = 0;
    private bool isTargeting = false;
    public bool IsTargetingActive => isTargeting || _showingAllTargetIndicators;
    private Camera mainCam;
    private Coroutine hideTimerCoroutine;
    public bool blockWorldClick = false;

    private void Awake()
    {
        mainCam = Camera.main; 
    }

    private void OnEnable()
    {
        BattleInputActions.Actions.Battle.Targeting.performed += OnTargetingPerformed;
        BattleInputActions.Actions.Battle.Submit.performed += OnSubmitPerformed;
        BattleInputActions.Actions.Battle.Cancel.performed += OnCancelPerformed;
    }

    private void OnDisable()
    {
        if (BattleInputActions.Actions != null)
        {
            BattleInputActions.Actions.Battle.Targeting.performed -= OnTargetingPerformed;
            BattleInputActions.Actions.Battle.Submit.performed -= OnSubmitPerformed;
            BattleInputActions.Actions.Battle.Cancel.performed -= OnCancelPerformed;
        }
    }
    
    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
            HandleMouseClick();
    }


    private void SetArrowVisible(bool visible)
    {
        if (currentArrow == null) return;
        foreach (Renderer r in _arrowRenderers)
        {
            if (r != null) r.enabled = visible;
        }
    }

    private void EnsureArrowExists()
    {
        if (currentArrow != null) return;
        if (arrowIndicatorPrefab == null) return;

        currentArrow = Instantiate(arrowIndicatorPrefab);
        _arrowRenderers = currentArrow.GetComponentsInChildren<Renderer>(true);
        SetArrowVisible(false);
    }

    private void HandleMouseClick()
    {
        if (HandleUIClick()) return;
        HandleWorldClick();
    }

    private bool HandleUIClick()
    {
        if (!EventSystem.current.IsPointerOverGameObject()) return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        bool isClickingValidUI = false;

        foreach (RaycastResult result in results)
        {
            GameObject hitObj = result.gameObject;
            if (hitObj.GetComponentInParent<ActionMenuUI>() != null || 
                hitObj.GetComponentInParent<HeroStatUI>() != null ||
                hitObj.GetComponentInParent<UnityEngine.UI.Button>() != null)
            {
                isClickingValidUI = true;
                break; 
            }
        }
        
        if (isTargeting && !isClickingValidUI)
            OnTargetCanceled?.Invoke(); 
        
        return true; 
    }

    private void HandleWorldClick()
    {
        
        if (_showingAllTargetIndicators)
        {
            HandleAllTargetClick();
            return;
        }
        
        if (blockWorldClick) return;
        
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            CharacterBase clickedEnemy = hit.collider.GetComponentInParent<CharacterBase>();

            
            if (clickedEnemy != null && CharacterManager.Instance.ActiveEnemies.Contains(clickedEnemy))
            {
                            
                if (AudioSystem.Instance != null && confirmTargetSound != null)
                    AudioSystem.Instance.PlayUISound(confirmTargetSound);


                if (isTargeting)
                {
                    int clickedIndex = CharacterManager.Instance.ActiveEnemies.IndexOf(clickedEnemy);

                    if (clickedIndex == currentTargetIndex) ConfirmTarget();
                    else
                    {
                        currentTargetIndex = clickedIndex;
                        UpdateHighlight();
                    }
                }
                else
                {
                    if (BattleManager.Instance != null && BattleManager.Instance.State == BattleState.HeroTurn)
                    {
                        SelectTarget(clickedEnemy.gameObject);
                        BattleManager.Instance.ApplyTargetToAllHeroes(clickedEnemy);
                    }
                }
            }
            else
            {
                if (isTargeting) OnTargetCanceled?.Invoke();
            }
        }
        else
        {
            if (isTargeting) OnTargetCanceled?.Invoke();
        }
    }

    public void SelectTarget(GameObject enemy)
    {
        if (AudioSystem.Instance != null && confirmTargetSound != null)
            AudioSystem.Instance.PlayUISound(confirmTargetSound);

        if (currentTarget != null)
        {
            TargetHighlight prevHighlight = currentTarget.GetComponent<TargetHighlight>();
            if (prevHighlight != null) prevHighlight.SetHighlight(false);
        }

        currentTarget = enemy;
        if (BattleManager.Instance != null)
        {
            CharacterBase enemyCharacter = enemy.GetComponent<CharacterBase>();
            if (enemyCharacter != null)
                BattleManager.Instance.ApplyCurrentMainTarget(enemyCharacter);
        }

        TargetHighlight currentHighlight = enemy.GetComponent<TargetHighlight>();
        if (currentHighlight != null) currentHighlight.SetHighlight(true);

        EnsureArrowExists();

        if (currentArrow != null)
        {
            SetArrowVisible(true);
            currentArrow.transform.SetParent(enemy.transform, false);

            UpdateArrowForTarget(enemy);
        }

        if (hideTimerCoroutine != null) StopCoroutine(hideTimerCoroutine);
        hideTimerCoroutine = StartCoroutine(HideCursorRoutine());
    }

    public void StartTargeting(CharacterBase previousTarget = null)
    {
        HideAllTargetIndicators();
        BattleInputActions.Actions.Battle.Enable(); 
        isTargeting = true;

        if (hideTimerCoroutine != null) StopCoroutine(hideTimerCoroutine);

        EnsureArrowExists();

        CharacterBase targetToUse = previousTarget;
        if (targetToUse == null && currentTarget != null)
            targetToUse = currentTarget.GetComponent<CharacterBase>();

        if (targetToUse != null && CharacterManager.Instance.ActiveEnemies.Contains(targetToUse))
            currentTargetIndex = CharacterManager.Instance.ActiveEnemies.IndexOf(targetToUse);
        else
            currentTargetIndex = 0;

        SetArrowVisible(true);
        UpdateHighlight();
    }

    private void HandleAllTargetClick()
    {
        if (AudioSystem.Instance != null && confirmTargetSound != null)
            AudioSystem.Instance.PlayUISound(confirmTargetSound);
        
        if (blockWorldClick || mainCam == null) return;

        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        CharacterBase enemy = hit.collider.GetComponentInParent<CharacterBase>();
        if (enemy != null && CharacterManager.Instance.ActiveEnemies.Contains(enemy))
            SetAllTargetMain(enemy);
    }

    private void SetAllTargetMain(CharacterBase enemy)
    {
        if (enemy == null || !_allTargetArrowByEnemy.ContainsKey(enemy)) return;

        _allTargetMain = enemy;
        currentTarget = enemy.gameObject;
        if (BattleManager.Instance != null)
            BattleManager.Instance.ApplyCurrentMainTarget(enemy);

        foreach (var pair in _allTargetArrowByEnemy)
        {
            SetArrowColor(pair.Value, pair.Key == _allTargetMain
                ? mainTargetColor
                : splashTargetColor);
        }

        TargetHighlight highlight = enemy.GetComponent<TargetHighlight>();
        if (highlight != null) highlight.SetHighlight(true);
    }

    private void SetArrowColor(GameObject arrow, Color color)
    {
        if (arrow == null) return;

        SpriteRenderer sprite = arrow.GetComponentInChildren<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = color;
            return;
        }

        Renderer renderer = arrow.GetComponentInChildren<Renderer>();
        if (renderer != null) renderer.material.color = color;
    }
    public void ShowAllTargetIndicators()
    {
        HideAllTargetIndicators();
        _showingAllTargetIndicators = true;

        if (arrowIndicatorPrefab == null || CharacterManager.Instance == null) return;

                _allTargetArrowByEnemy.Clear();
        _allTargetMain = CharacterManager.Instance.ActiveEnemies.Contains(currentTarget != null ? currentTarget.GetComponent<CharacterBase>() : null) ? currentTarget.GetComponent<CharacterBase>() : null;
foreach (CharacterBase enemy in CharacterManager.Instance.ActiveEnemies)
        {
            if (enemy == null) continue;

            TargetHighlight highlight = enemy.GetComponent<TargetHighlight>();
            if (highlight != null) highlight.SetHighlight(true);

            GameObject arrow = Instantiate(arrowIndicatorPrefab);
            arrow.transform.SetParent(enemy.transform, false);
            
            GameObject previousArrow = currentArrow;
            currentArrow = arrow;
            UpdateArrowForTarget(enemy.gameObject);
            currentArrow = previousArrow;

            _allTargetArrows.Add(arrow);
            _allTargetArrowByEnemy[enemy] = arrow;
        }

        if (_allTargetMain == null && CharacterManager.Instance.ActiveEnemies.Count > 0)
            _allTargetMain = CharacterManager.Instance.ActiveEnemies[0];
        SetAllTargetMain(_allTargetMain);
    }

    public void HideAllTargetIndicators()
    {
        _showingAllTargetIndicators = false;
        if (CharacterManager.Instance != null)
        {
            foreach (CharacterBase enemy in CharacterManager.Instance.ActiveEnemies)
            {
                if (enemy == null) continue;
                TargetHighlight highlight = enemy.GetComponent<TargetHighlight>();
                if (highlight != null) highlight.SetHighlight(false);
            }
        }

        foreach (GameObject arrow in _allTargetArrows)
        {
            if (arrow != null) Destroy(arrow);
        }

        _allTargetArrows.Clear();
        _allTargetArrowByEnemy.Clear();
        _allTargetMain = null;
    }
    public void StopTargeting()
    {
        HideAllTargetIndicators();
        isTargeting = false;

        if (hideTimerCoroutine != null)
        {
            StopCoroutine(hideTimerCoroutine);
            hideTimerCoroutine = null;
        }

        ClearAllHighlights();
        SetArrowVisible(false);
        BattleInputActions.Actions.Battle.Disable();
    }

    public CharacterBase GetCurrentTarget()
    {
        if (CharacterManager.Instance.ActiveEnemies.Count == 0) return null;
        return CharacterManager.Instance.ActiveEnemies[currentTargetIndex];
    }

    private void OnTargetingPerformed(InputAction.CallbackContext ctx)
    {
        if (_showingAllTargetIndicators)
        {
            float allDirection = ctx.ReadValue<float>();
            if (allDirection > 0) ChangeAllTarget(-1);
            else if (allDirection < 0) ChangeAllTarget(1);
            return;
        }
        if (!isTargeting) return;
        float direction = ctx.ReadValue<float>();
        if (direction > 0) ChangeTarget(1); 
        else if (direction < 0) ChangeTarget(-1); 
    }

    private void OnSubmitPerformed(InputAction.CallbackContext ctx)
    {
        if (!isTargeting) return;
        ConfirmTarget();
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        if (!isTargeting) return;
        OnTargetCanceled?.Invoke();
    }

    private void ConfirmTarget()
    {
        var enemies = CharacterManager.Instance.ActiveEnemies;
        if (currentTargetIndex < 0 || currentTargetIndex >= enemies.Count) return;
        OnTargetConfirmed?.Invoke(enemies[currentTargetIndex]);
    }

    private void ChangeAllTarget(int direction)
    {
        if (CharacterManager.Instance == null || CharacterManager.Instance.ActiveEnemies.Count == 0) return;

        int index = CharacterManager.Instance.ActiveEnemies.IndexOf(_allTargetMain);
        if (index < 0) index = 0;

        index = (index + direction) % CharacterManager.Instance.ActiveEnemies.Count;
        if (index < 0) index += CharacterManager.Instance.ActiveEnemies.Count;

        SetAllTargetMain(CharacterManager.Instance.ActiveEnemies[index]);
    }
    private void ChangeTarget(int direction)
    {
        SetEnemyHighlight(currentTargetIndex, false);
        currentTargetIndex += direction;

        int enemyCount = CharacterManager.Instance.ActiveEnemies.Count;
        if (currentTargetIndex >= enemyCount) currentTargetIndex = 0;
        if (currentTargetIndex < 0) currentTargetIndex = enemyCount - 1;

        SetEnemyHighlight(currentTargetIndex, true);
    }

    private void UpdateHighlight()
    {
        ClearAllHighlights();
        SetEnemyHighlight(currentTargetIndex, true);
    }

    private void ClearAllHighlights()
    {
        for (int i = 0; i < CharacterManager.Instance.ActiveEnemies.Count; i++)
            SetEnemyHighlight(i, false);
    }

    private void SetEnemyHighlight(int index, bool isHighlighted)
    {
        if (index < 0 || index >= CharacterManager.Instance.ActiveEnemies.Count) return;

        CharacterBase enemy = CharacterManager.Instance.ActiveEnemies[index];
        
        TargetHighlight highlight = enemy.GetComponent<TargetHighlight>();
        if (highlight != null) highlight.SetHighlight(isHighlighted);

        if (isHighlighted && currentArrow != null)
        {
            currentArrow.transform.SetParent(enemy.transform, false);

            UpdateArrowForTarget(enemy);
        }
    }

    private void UpdateArrowForTarget(CharacterBase target)
    {
        if (target != null) UpdateArrowForTarget(target.gameObject);
    }

    private void UpdateArrowForTarget(GameObject target)
    {
        if (currentArrow == null || target == null) return;

        Renderer targetRenderer = target.GetComponentInChildren<Renderer>();
        Renderer arrowRenderer = currentArrow.GetComponentInChildren<Renderer>();
        if (targetRenderer == null || arrowRenderer == null) return;

        Bounds targetBounds = targetRenderer.bounds;

        // Prefer the collider bounds: sprite animation frames can have
        // different transparent margins and otherwise change the arrow size.
        Collider targetCollider = target.GetComponentInChildren<Collider>();
        if (targetCollider != null)
        {
            targetBounds = targetCollider.bounds;
        }
        else
        {
            Collider2D targetCollider2D = target.GetComponentInChildren<Collider2D>();
            if (targetCollider2D != null)
                targetBounds = targetCollider2D.bounds;
        }
        float desiredWidth = Mathf.Clamp(targetBounds.size.x * arrowWidthMultiplier, arrowMinWidth, arrowMaxWidth);

        // ArrowTargeting uses a 9-sliced SpriteRenderer. Resize its size,
        // instead of scaling the transform, so the corners keep their shape.
        SpriteRenderer slicedArrow = currentArrow.GetComponentInChildren<SpriteRenderer>();
        if (slicedArrow != null && slicedArrow.drawMode != SpriteDrawMode.Simple)
        {
            float parentScaleX = Mathf.Max(0.001f, Mathf.Abs(currentArrow.transform.lossyScale.x));
            Vector2 slicedSize = slicedArrow.size;
            slicedSize.x = desiredWidth / parentScaleX;
            slicedArrow.size = slicedSize;
        }
        else
        {
            float baseWidth = Mathf.Max(0.001f, arrowRenderer.bounds.size.x);
            Vector3 scale = currentArrow.transform.localScale;
            scale.x *= desiredWidth / baseWidth;
            currentArrow.transform.localScale = scale;
        }

        Vector3 pivot = new Vector3(targetBounds.center.x, targetBounds.max.y + arrowHeightGap, targetBounds.center.z);
        currentArrow.transform.localPosition = target.transform.InverseTransformPoint(pivot);
    }
    private IEnumerator HideCursorRoutine()
    {
        yield return new WaitForSeconds(autoHideDelay);
        SetArrowVisible(false);

        if (currentTarget != null)
        {
            TargetHighlight highlight = currentTarget.GetComponent<TargetHighlight>();
            if (highlight != null) highlight.SetHighlight(false);
        }
    }
}
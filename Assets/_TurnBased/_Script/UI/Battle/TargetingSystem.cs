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
    private Renderer[] _arrowRenderers;
    
    [Header("Target Data")]
    public GameObject currentTarget;  
    
    [Header("Settings")]
    public float autoHideDelay = 3.0f;

    [Header("Targeting Audio")]
    [SerializeField] private AudioClip confirmTargetSound;
    [SerializeField] private Vector3 arrowFallbackOffset = new Vector3(0, 1.3f, 0);

    private int currentTargetIndex = 0;
    private bool isTargeting = false;
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
        if (blockWorldClick) return;
        
        Ray ray = mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            CharacterBase clickedEnemy = hit.collider.GetComponentInParent<CharacterBase>();
            
            if (clickedEnemy != null && CharacterManager.Instance.ActiveEnemies.Contains(clickedEnemy))
            {
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

        TargetHighlight currentHighlight = enemy.GetComponent<TargetHighlight>();
        if (currentHighlight != null) currentHighlight.SetHighlight(true);

        EnsureArrowExists();

        if (currentArrow != null)
        {
            SetArrowVisible(true);
            currentArrow.transform.SetParent(enemy.transform, false);

            TargetIndicator indicator = currentArrow.GetComponent<TargetIndicator>();
            if (indicator != null) indicator.SetPivot(arrowFallbackOffset); 
            else currentArrow.transform.localPosition = arrowFallbackOffset;
        }

        if (hideTimerCoroutine != null) StopCoroutine(hideTimerCoroutine);
        hideTimerCoroutine = StartCoroutine(HideCursorRoutine());
    }

    public void StartTargeting(CharacterBase previousTarget = null)
    {
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

    public void StopTargeting()
    {
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

            TargetIndicator indicator = currentArrow.GetComponent<TargetIndicator>();
            if (indicator != null) indicator.SetPivot(new Vector3(0, 1.3f, 0)); 
            else currentArrow.transform.localPosition = new Vector3(0, 1.0f, 0);
        }
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
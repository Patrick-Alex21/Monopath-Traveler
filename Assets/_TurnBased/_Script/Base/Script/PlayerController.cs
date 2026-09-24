using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float groundDist;
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private CharacterController controller;

    [Header("Audio Data")]
    [SerializeField] private AudioClip[] dirtFootsteps;

    [Header("Visuals & Animation")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private Vector2 _moveInput;
    private PlayerInputAction _actions;

    private void Awake()
    {
        _actions = new PlayerInputAction();
    }

    private void OnEnable()
    {
        _actions.Enable();
        _actions.Player.Move.performed += OnMovementPerformed;
        _actions.Player.Move.canceled += OnMovementCanceled;
    }

    private void OnDisable()
    {
        _actions.Player.Move.performed -= OnMovementPerformed;
        _actions.Player.Move.canceled -= OnMovementCanceled;
        _actions.Disable();
    }

    private void OnMovementPerformed(InputAction.CallbackContext ctx) => _moveInput = ctx.ReadValue<Vector2>();
    private void OnMovementCanceled(InputAction.CallbackContext ctx) => _moveInput = Vector2.zero;

    private void Update()
    {   
        if (GameManager.Instance.State != GameState.Exploring) return;
        
        HandleMovement();
    }
    private void LateUpdate()
    {
        SnapToTerrain();
    }

    private void HandleMovement()
    {
        Vector2 strictInput = Vector2.zero;

        if (Mathf.Abs(_moveInput.x) > Mathf.Abs(_moveInput.y))
        {
            strictInput.x = Mathf.Sign(_moveInput.x); 
            
            
            if (strictInput.x > 0) spriteRenderer.flipX = true; 
            else if (strictInput.x < 0) spriteRenderer.flipX = false;
        }
        else if (Mathf.Abs(_moveInput.y) > 0)
        {
            strictInput.y = Mathf.Sign(_moveInput.y); 
        }

        
        bool isMoving = strictInput != Vector2.zero;

        animator.SetBool("isWalking", isMoving);

        if (isMoving)
        {
            animator.SetFloat("AnimX", strictInput.x);
            animator.SetFloat("AnimY", strictInput.y);
        }
        
        Vector3 moveDir = new Vector3(strictInput.x, 0f, strictInput.y);
        controller.Move(moveDir * speed * Time.deltaTime);
    }

    private void SnapToTerrain()
    {
        Vector3 castPos = transform.position;
        castPos.y += 1f;

        if (Physics.Raycast(castPos, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainLayer))
        {
            Vector3 targetPos = transform.position;
            targetPos.y = hit.point.y + groundDist;
            transform.position = targetPos;
        }
    }

    public void StepEvent()
    {
        bool isRestricted = (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.isTransitioning) ||
                        (GameManager.Instance != null && GameManager.Instance.State != GameState.Exploring);

        if (isRestricted)
        {
            return; 
        }

        if (dirtFootsteps == null || dirtFootsteps.Length == 0) return;

        int randomIndex = Random.Range(0, dirtFootsteps.Length);
        AudioClip chosenStep = dirtFootsteps[randomIndex];

        if (AudioSystem.Instance != null && chosenStep != null)
        {
            AudioSystem.Instance.PlaySound(chosenStep, 0.4f); 
        }
    }
}
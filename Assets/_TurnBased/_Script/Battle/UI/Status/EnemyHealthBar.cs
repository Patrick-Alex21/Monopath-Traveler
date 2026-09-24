using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Masukkan komponen Image yang menjadi isi (fill) darah musuh")]
    [SerializeField] private Image hpFillImage;
    
    [Header("Behavior")]
    [Tooltip("Matikan bar jika darah penuh?")]
    [SerializeField] private bool hideWhenZero = false;

    private CharacterBase _character;
    private EnemyWeaknessUI _weaknessIcon;
    private Canvas _canvas;

    private void Awake()
    {
        _character = GetComponentInParent<CharacterBase>();
        _canvas = GetComponent<Canvas>();
        
        _weaknessIcon = GetComponent<EnemyWeaknessUI>();

    }

    private void Start()
    {
        if (_weaknessIcon != null)
        {
            _weaknessIcon.SetupUI();
        }
    }

    private void OnEnable()
    {
        if (_character != null)
        {
            _character.OnHealthChanged += UpdateBar;
            UpdateBar(_character.currentHp, _character.Stats.maxHp);
        }
    }

    private void OnDisable()
    {
        if (_character != null)
        {
            _character.OnHealthChanged -= UpdateBar;
        }
    }

    private void UpdateBar(int currentHp, int maxHp)
    {
        if (hpFillImage == null || maxHp <= 0) return;

        float healthPercentage = (float)currentHp / maxHp;
        hpFillImage.fillAmount = healthPercentage;

        
        if (healthPercentage > 0.5f) 
        {
            hpFillImage.color = Color.green; 
        }
        else if (healthPercentage > 0.25f) 
        {
            
            hpFillImage.color = new Color(0.8f, 0.7f, 0f); 
        }
        else 
        {
            hpFillImage.color = Color.red; 
        }

        
        if (hideWhenZero && _canvas != null)
        {
            _canvas.enabled = healthPercentage < 1.0f;
        }
    }
}
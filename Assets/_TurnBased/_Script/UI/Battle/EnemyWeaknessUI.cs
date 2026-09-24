using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyWeaknessUI : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private Transform iconContainer; 
    [SerializeField] private GameObject iconPrefab;

    [Header("Visuals")]
    [SerializeField] private Sprite unknownWeaknessSprite; 

    private EnemyBase _enemy;
    private List<Image> spawnedIcons = new List<Image>();

    private void Awake()
    {
        _enemy = GetComponentInParent<EnemyBase>();
    }

    private void OnEnable()
    {
        if (_enemy != null)
        {
            
            _enemy.OnWeaknessDiscovered += RefreshIcons;
        }
    }

    private void OnDisable()
    {
        if (_enemy != null)
        {
            _enemy.OnWeaknessDiscovered -= RefreshIcons;
        }
    }

    
    public void SetupUI()
    {
        if (_enemy == null || _enemy.Weaknesses == null) return;

        
        foreach (Transform child in iconContainer) Destroy(child.gameObject);
        spawnedIcons.Clear();

        
        foreach (ScriptableElement weakElement in _enemy.Weaknesses)
        {
            if (weakElement != null)
            {
                GameObject iconObj = Instantiate(iconPrefab, iconContainer);
                WeaknessIcon iconScript = iconObj.GetComponent<WeaknessIcon>();
                spawnedIcons.Add(iconScript.iconImage);
            }
        }

        RefreshIcons(); 
    }

    private void RefreshIcons()
    {
        if (_enemy == null) return;

        
        for (int i = 0; i < _enemy.Weaknesses.Count; i++)
        {
            ScriptableElement currentElement = _enemy.Weaknesses[i];
            Image currentIcon = spawnedIcons[i];

            if (_enemy.DiscoveredWeaknesses.Contains(currentElement))
            {
                currentIcon.sprite = currentElement.elementIcon;
            }
            else
            {
                currentIcon.sprite = unknownWeaknessSprite;
            }
        }
    }
}
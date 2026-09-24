using System;
using UnityEngine;

public class ScriptableBaseCharacter : ScriptableObject
{
    public Sprite DefaultSprite;
    public CharacterType characterType;
    [SerializeField] private Stats _stats;
    public Stats BaseStats => _stats;

    [Header("UI Visuals")]
    public string Description;
    public Sprite MenuSprite;

    [Header("Animation System")]
    public AnimatorOverrideController animatorOverride; 
}
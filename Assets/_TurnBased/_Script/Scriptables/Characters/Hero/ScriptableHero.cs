using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hero", menuName = "Game/Hero")]
public class ScriptableHero : ScriptableBaseCharacter
{
    [Header("Hero Data")]
    public string heroName;
    public HeroType heroType;
    public HeroCharBase Prefab;

    [Header("Hero Skillset")]
    public List<ScriptableSkill> skills; 

    [Header("Combat Settings")]
    public ScriptableSkill heroBasicAttack;

    [Header("Voice Line Settings")]
    public ScriptableHeroVoice voice;
}
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Game/Skill")]
public class ScriptableSkill : ScriptableObject
{
    [Header("General Info")]
    public string skillName;
    public int spCost;
    public int hitCount = 1; 
    
    [TextArea(3, 5)] 
    public string description;

    [Header("Skill Properties")]
    public SkillCategory skillCategory = SkillCategory.None; 
    public TargetScope targetScope;
    public ScriptableElement skillElement;
    public int basePower;

    [Header("Multi-Hit Settings")]
    public bool playAnimationPerHit = true; 
    
    [Tooltip("Delay Between Hit")]
    public float multiHitInterval = 0.2f;

    [Header("Animation & Timing Settings")]
    public SkillAnimTrigger animTrigger = SkillAnimTrigger.Attack;
    public float skillFinishDelay = 0.8f; 

    [Header("Casting Phase (Universal)")]
    public GameObject castVfxPrefab;          
    public float castVfxDuration = 0.6f;     
    public AudioClip castSound;

    [Header("Impact Phase: Spell & Recovery")]
    public GameObject vfxPrefab;            
    public VFXSpawnLocation vfxSpawnLocation = VFXSpawnLocation.ActionCenter;  
    public AudioClip VFXSound;       
    public float vfxImpactDelay = 0.7f;
    public float vfxDuration = 1.5f;

    [Header("Impact Phase: Melee Slash")]
    public GameObject swingVfxPrefab;       
    public GameObject slashVfxPrefab;       
    public GameObject sparkVfxPrefab;
    public AudioClip attackNormal;
    public AudioClip attackWeakness;   
    public float slashImpactDelay = 0.15f;  
    public float swingDelay = 0.15f;  
    
    [Header("Camera Shake")]
    public bool doCameraShake = false;
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.15f;

}
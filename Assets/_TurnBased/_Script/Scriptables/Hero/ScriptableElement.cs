using UnityEngine;

[CreateAssetMenu(fileName = "New Element", menuName = "Game/Element")]
public class ScriptableElement : ScriptableObject
{
    [Header("Element Settings")]
    public SkillElement element; 
    public Sprite elementIcon; 
    
}
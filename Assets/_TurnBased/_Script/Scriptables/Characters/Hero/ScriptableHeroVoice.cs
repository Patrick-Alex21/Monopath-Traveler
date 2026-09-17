using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Hero Voice", menuName = "Game/Hero Voice")]
public class ScriptableHeroVoice : ScriptableObject
{
    [Header("Voice Lines")]
    public List<AudioClip> attack;
    public List<AudioClip> attackWeakness;
    public List<AudioClip> skill;
    public List<AudioClip> special;
    public List<AudioClip> gettingHealed;
    public List<AudioClip> heal;
    public List<AudioClip> hurt;
    public List<AudioClip> die;
    public List<AudioClip> myTurn;
    public List<AudioClip> boost;

    public AudioClip GetRandom(List<AudioClip> list)
    {
        if (list == null || list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
    }
}
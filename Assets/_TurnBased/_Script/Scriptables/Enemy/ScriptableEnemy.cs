using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Game/Enemy")]
public class ScriptableEnemy : ScriptableBaseCharacter
{
    [Header("Enemy Data")]
    public string enemyName;
    public EnemyType enemyType;
    public EnemyBase Prefab; 

    [Header("Elemental Affinities")]
    public List<ScriptableElement> weaknesses;
    public List<ScriptableElement> resistances;
}
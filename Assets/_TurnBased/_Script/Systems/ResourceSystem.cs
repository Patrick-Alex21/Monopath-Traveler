using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ResourceSystem : Singleton<ResourceSystem>
{
    public List<ScriptableHero> Heroes { get; private set; }
    private Dictionary<HeroType, ScriptableHero> _HeroesDict;

    
    public List<ScriptableEnemy> Enemies { get; private set; }
    private Dictionary<EnemyType, ScriptableEnemy> _EnemiesDict;

    protected override void Awake()
    {
        base.Awake();
        AssembleResources();
    }

    private void AssembleResources()
{
    
    Heroes = Resources.LoadAll<ScriptableHero>("Heroes").ToList();
    _HeroesDict = new Dictionary<HeroType, ScriptableHero>();

    foreach (var hero in Heroes)
    {
        if (_HeroesDict.ContainsKey(hero.heroType))
        {
            Debug.LogWarning($"[ResourceSystem] Duplikat ditemukan! HeroType '{hero.heroType}' sudah ada. Mengabaikan aset: {hero.name}");
        }
        else
        {
            _HeroesDict.Add(hero.heroType, hero);
        }
    }

    
    Enemies = Resources.LoadAll<ScriptableEnemy>("Enemies").ToList();
    _EnemiesDict = new Dictionary<EnemyType, ScriptableEnemy>();

    foreach (var enemy in Enemies)
    {
        if (_EnemiesDict.ContainsKey(enemy.enemyType))
        {
            Debug.LogWarning($"[ResourceSystem] Duplikat ditemukan! EnemyType '{enemy.enemyType}' sudah ada. Mengabaikan aset: {enemy.name}");
        }
        else
        {
            _EnemiesDict.Add(enemy.enemyType, enemy);
        }
    }
}           

    
    public ScriptableHero GetHero(HeroType t)
    {
        if (_HeroesDict.TryGetValue(t, out var hero)) return hero;
        Debug.LogError($"[ResourceSystem] No ScriptableHero asset found for HeroType '{t}'. Did you forget to create one in Resources/Heroes?");
        return null;
    }
    public ScriptableHero GetRandomHero() => Heroes[Random.Range(0, Heroes.Count)];

    
    public ScriptableEnemy GetEnemy(EnemyType t)
    {
        if (_EnemiesDict.TryGetValue(t, out var enemy)) return enemy;
        Debug.LogError($"[ResourceSystem] No ScriptableEnemy asset found for EnemyType '{t}'. Did you forget to create one in Resources/Enemies?");
        return null;
    }
    public ScriptableEnemy GetRandomEnemy() => Enemies[Random.Range(0, Enemies.Count)];
}
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : Singleton<CharacterManager>
{
    [Header("Hero Slots")]
    [SerializeField] private Transform heroTopSlot;
    [SerializeField] private Transform heroMidSlot;
    [SerializeField] private Transform heroBottomSlot;

    [Header("Enemy Slots")]
    [SerializeField] private Transform enemyTopSlot;
    [SerializeField] private Transform enemyMidSlot;
    [SerializeField] private Transform enemyBottomSlot;

    public List<CharacterBase> ActiveHeroes { get; private set; } = new List<CharacterBase>();
    public List<CharacterBase> ActiveEnemies { get; private set; } = new List<CharacterBase>();
    public List<HeroCharBase> HeroesPhysics { get; private set; } = new List<HeroCharBase>();

    public void SpawnHeroes()
    {
        ActiveHeroes.Clear();
        HeroesPhysics.Clear(); 

        List<HeroType> party = GameManager.Instance.CurrentParty;
        if (party == null || party.Count == 0)
        {
            Transform targetSlot = GetHeroSlot(1, 0);
            SpawnHeroUnit(HeroType.Alfyn, targetSlot); 
            return;
        }

        int totalHeroes = party.Count;
        for (int i = 0; i < totalHeroes; i++)
        {
            Transform targetSlot = GetHeroSlot(totalHeroes, i);
            SpawnHeroUnit(party[i], targetSlot); 
        }
    }

    private HeroCharBase SpawnHeroUnit(HeroType t, Transform slot)
    {
        var data = ResourceSystem.Instance.GetHero(t);
        var spawned = Instantiate(data.Prefab, slot.position, Quaternion.identity, slot);

        spawned.InitUnitData(data);
        spawned.OnDeath += (c) => BattleUIManager.Instance.RemoveFromTurnQueue(c);

        if (GameManager.Instance.TryGetHeroState(t, out int hp, out int sp))
            spawned.RestoreState(hp, sp);

        ActiveHeroes.Add(spawned);
        HeroesPhysics.Add(spawned);

        return spawned;
    }
    public void SpawnEnemies()
    {
        ActiveEnemies.Clear();
        
        ScriptableEncounter currentEncounter = EncounterManager.Instance.CurrentEncounter;

        if (currentEncounter == null || currentEncounter.enemiesInBattle.Count == 0)
        {
            Transform targetSlot = GetEnemySlot(2, 0);
            Transform targetSlot2 = GetEnemySlot(2, 1);
            SpawnEnemyUnit(EnemyType.Slime, targetSlot);
            SpawnEnemyUnit(EnemyType.GreenSlime, targetSlot2);
            return;
        }

        int totalEnemies = currentEncounter.enemiesInBattle.Count;

        for (int i = 0; i < totalEnemies; i++)
        {
            EnemyType currentEnemyType = currentEncounter.enemiesInBattle[i];
            Transform targetSlot = GetEnemySlot(totalEnemies, i);
            
            SpawnEnemyUnit(currentEnemyType, targetSlot); 
        }
    }

    private void SpawnEnemyUnit(EnemyType t, Transform slot) 
    {
        var data = ResourceSystem.Instance.GetEnemy(t);
        var spawned = Instantiate(data.Prefab, slot.position, Quaternion.identity, slot);
        
        spawned.InitUnitData(data);
        spawned.OnDeath += (c) => BattleUIManager.Instance.RemoveFromTurnQueue(c);
        
        spawned.SetElementalAffinities(data.weaknesses, data.resistances); 
        
        SnapToGround snapper = spawned.GetComponent<SnapToGround>();
        if (snapper != null) snapper.Snap();
        
        ActiveEnemies.Add(spawned);
    }

    public int GetHeroSlotIndex(int totalHeroes, int currentIndex)
    {
        switch (totalHeroes)
        {
            case 1: return 1;
            case 2: return (currentIndex == 0) ? 0 : 2;
            case 3: return currentIndex;
            default: return 1; 
        }
    }

    private Transform GetHeroSlot(int totalHeroes, int currentIndex)
    {
        int index = GetHeroSlotIndex(totalHeroes, currentIndex);
        
        if (index == 0) return heroTopSlot;
        if (index == 2) return heroBottomSlot;
        
        return heroMidSlot;
    }

    private Transform GetEnemySlot(int totalEnemies, int currentIndex)
    {
        switch (totalEnemies)
        {
            case 1: return enemyMidSlot;
            case 2: return (currentIndex == 0) ? enemyTopSlot : enemyBottomSlot;
            case 3: 
                if (currentIndex == 0) return enemyTopSlot;
                if (currentIndex == 1) return enemyMidSlot;
                return enemyBottomSlot;
            default: return enemyMidSlot; 
        }
    }
}
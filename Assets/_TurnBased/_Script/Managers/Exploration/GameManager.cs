using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : PersistentSingleton<GameManager> 
{
    public static event Action<GameState> OnGameStateChanged;
    public GameState State { get; private set; }

    [Header("Player Data (Party)")]
    [SerializeField] private List<HeroType> currentParty = new List<HeroType>();
    public List<HeroType> CurrentParty => currentParty; 

    [Header("Party Health/SP Persistence")]
    private Dictionary<HeroType, (int hp, int sp)> partyState = new Dictionary<HeroType, (int, int)>();
    
    private void Start()
    {
        ChangeState(GameState.Exploring); 
    }
    
    public void ChangeState(GameState newState)
    {
        if (State == newState) return;

        State = newState;
        OnGameStateChanged?.Invoke(newState);
        
        Debug.Log($"Game State Berubah: {newState}");
    }

    public void AddPartyMember(HeroType newHero)
    {
        if (!currentParty.Contains(newHero))
        {
            currentParty.Add(newHero);
        }
    }

    public void SaveHeroState(HeroType type, int hp, int sp)
    {
        partyState[type] = (hp, sp);
    }

    public bool TryGetHeroState(HeroType type, out int hp, out int sp)
    {
        if (partyState.TryGetValue(type, out var s))
        {
            hp = s.hp; sp = s.sp;
            return true;
        }
        hp = sp = 0;
        return false;
    }
}
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
}
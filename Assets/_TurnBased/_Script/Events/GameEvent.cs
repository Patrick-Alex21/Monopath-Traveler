using System;

public static class GameEvents
{
    public static event Action<ScriptableEncounter> OnBattleRequested;

    public static void RequestBattle(ScriptableEncounter encounter)
    {
        OnBattleRequested?.Invoke(encounter);
    }
}
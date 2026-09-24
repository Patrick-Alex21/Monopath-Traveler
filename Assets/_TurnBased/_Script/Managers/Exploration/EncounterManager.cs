using UnityEngine;

public class EncounterManager : PersistentSingleton<EncounterManager>
{
    public ScriptableEncounter CurrentEncounter { get; set; }

    private void OnEnable()
    {
        GameEvents.OnBattleRequested += StartBattleEncounter;
    }

    private void OnDisable()
    {
        GameEvents.OnBattleRequested -= StartBattleEncounter;
    }

    private void StartBattleEncounter(ScriptableEncounter encounter)
    {
        CurrentEncounter = encounter;
        SceneTransitionManager.Instance.TransitionToBattle();
    }
}
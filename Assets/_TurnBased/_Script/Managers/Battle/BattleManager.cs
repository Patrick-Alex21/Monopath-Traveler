using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BattleManager : Singleton<BattleManager>
{
    public static event Action<BattleState> OnPreStateChange;
    public static event Action<BattleState> OnPostStateChange;
    public BattleState State { get; private set; }
    public const int MAX_BOOST = 3;

    [Header("Systems References")]
    [SerializeField] private TargetingSystem targetingSystem;
    public TargetingSystem TargetingSystem => targetingSystem;

    [Header("Stage References")]
    [SerializeField] private Transform actionCenterPoint; 
    public Vector3 ActionCenterPosition => actionCenterPoint.position;
    private Camera _camera;
    public Camera BattleCamera => _camera;
    
    [Header("Audio")]
    [SerializeField] private AudioClip battleBGM;
    [SerializeField] private AudioClip battleVictory;
    [SerializeField] private AudioClip battleDefeat;
    [SerializeField] private AudioClip startSound;


    [Header("Entrance Sequence")]
    [SerializeField] private Transform[] heroSpawnPoints;
    [SerializeField] private Transform[] heroFinalPoints;
    [SerializeField] private float heroEntranceDuration = 1.0f;
    
    private List<CharacterBase> _currentTurnOrder;
    private HeroCharBase _heroBeingPlanned;
    private bool _isFirstTurn = true;

    private void OnEnable()
    {
        if (targetingSystem != null)
        {
            targetingSystem.OnTargetConfirmed += HandleAttackExecution;
            targetingSystem.OnTargetCanceled += HandleCancelTargeting;
        }
    }

    private void OnDisable()
    {
        if (targetingSystem != null)
        {
            targetingSystem.OnTargetConfirmed -= HandleAttackExecution;
            targetingSystem.OnTargetCanceled -= HandleCancelTargeting;
        }
    }

    void Start()
    {
        StartCoroutine(BattleIntroSequence());
    }

    private IEnumerator BattleIntroSequence()
    {
        BattleUIManager.Instance.HideAllBattleUI();
        
        ChangeState(BattleState.SpawningEnemies);

        yield return new WaitForSeconds(1.5f);
    }

    public void ChangeState(BattleState newState)
    {
        if (State == BattleState.Victory || State == BattleState.Defeat) return; 


        if(State == newState) return;
        
        OnPreStateChange?.Invoke(newState);
        State = newState;

        switch (newState)
        {
            case BattleState.SpawningHeroes:
                HandleSpawningHeroes();
                break;
            case BattleState.SpawningEnemies:
                HandleSpawningEnemies();
                break;
            case BattleState.HeroTurn:
                AudioSystem.Instance.PlayUISound(startSound);
                HandleHeroTurn();
                break;
            case BattleState.ExecutingTurn:              
                StartCoroutine(ExecutionSequenceCoroutine()); 
                break;
            case BattleState.Victory:
                StartCoroutine(ShowResultUIDelayed(true)); 
                break;
            case BattleState.Defeat:
                StartCoroutine(ShowResultUIDelayed(false)); 
                break;
        }

        OnPostStateChange?.Invoke(newState);
    }

    private IEnumerator ShowResultUIDelayed(bool isVictory)
    {
        if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.PreloadReturnScene();
        
        if (BoostVFXManager.Instance != null) BoostVFXManager.Instance.StopAllEffects();

        BattleUIManager.Instance.CloseActionMenu();
        BattleUIManager.Instance.HideCommandPanel();
        
        yield return new WaitForSeconds(2.0f);

        if (isVictory) 
        {
            foreach (var hero in CharacterManager.Instance.HeroesPhysics)
            {
                var heroData = hero.CharacterData as ScriptableHero;
                if (heroData != null)
                    GameManager.Instance.SaveHeroState(heroData.heroType, hero.currentHp, hero.currentSp);
            }
            BattleUIManager.Instance.ShowVictoryScreen();
        }
        else 
        {
            BattleUIManager.Instance.ShowDefeatScreen();
            if (battleDefeat != null) AudioSystem.Instance.PlayMusic(battleDefeat);
        }
    }

    private void HandleSpawningHeroes()
    {
        StartCoroutine(HeroEntranceRoutine());
    }

    private IEnumerator HeroEntranceRoutine()
    {
        CharacterManager.Instance.SpawnHeroes(); 

        List<HeroCharBase> activeHeroes = GetActiveHeroes();
        int totalHeroes = activeHeroes.Count;
        int[] cachedSlotIndexes = new int[totalHeroes];
        SnapToGround[] heroSnappers = new SnapToGround[totalHeroes];

        for (int i = 0; i < totalHeroes; i++)
        {
            if (activeHeroes[i] != null)
            {
                cachedSlotIndexes[i] = CharacterManager.Instance.GetHeroSlotIndex(totalHeroes, i);

                activeHeroes[i].transform.position = heroSpawnPoints[cachedSlotIndexes[i]].position;
                
                heroSnappers[i] = activeHeroes[i].GetComponent<SnapToGround>();
                if (heroSnappers[i] != null) heroSnappers[i].Snap();
            }
        }

        List<HeroType> partyTypes = GameManager.Instance.CurrentParty;
        List<ScriptableHero> partyData = new List<ScriptableHero>();

        if (partyTypes == null || partyTypes.Count == 0)
            partyData.Add(ResourceSystem.Instance.GetHero(HeroType.Alfyn));
        else
        {
            foreach (HeroType heroType in partyTypes)
                partyData.Add(ResourceSystem.Instance.GetHero(heroType));
        }



        float elapsed = 0;
        while (elapsed < heroEntranceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / heroEntranceDuration;
            float easeOutCurve = Mathf.Sin(t * Mathf.PI * 0.5f);

            for (int i = 0; i < totalHeroes; i++)
            {
                if (activeHeroes[i] != null)
                {
                    activeHeroes[i].transform.position = Vector3.Lerp(
                        heroSpawnPoints[cachedSlotIndexes[i]].position, 
                        heroFinalPoints[cachedSlotIndexes[i]].position, 
                        easeOutCurve
                    );

                    if (heroSnappers[i] != null) heroSnappers[i].Snap();
                }
            }
            yield return null;
        }

        for (int i = 0; i < totalHeroes; i++)
        {
            if (activeHeroes[i] != null)
            {
                activeHeroes[i].transform.position = heroFinalPoints[cachedSlotIndexes[i]].position;
                if (heroSnappers[i] != null) heroSnappers[i].Snap(); 
            }
        }

        yield return new WaitForSeconds(1f);
        
        BattleUIManager.Instance.SetupPartyUI(partyData, activeHeroes);

        ChangeState(BattleState.HeroTurn);
    }

    private void HandleSpawningEnemies()
    {
        CharacterManager.Instance.SpawnEnemies(); 
        ChangeState(BattleState.SpawningHeroes); 
    }

    private void HandleHeroTurn()
    {
        List<HeroCharBase> activeHeroes = GetActiveHeroes();
        if (activeHeroes == null || activeHeroes.Count == 0) return;

        if (_isFirstTurn) _isFirstTurn = false; 
        else
        {
            foreach (HeroCharBase hero in activeHeroes)
            {
                hero.CurrentBP = Mathf.Min(hero.CurrentBP + 1, 5); 
                if (hero.currentSp < hero.Stats.maxSp) hero.RegenerateSP(5);
            }
        }

        List<CharacterBase> currentRound = new List<CharacterBase>();
        currentRound.AddRange(GetActiveHeroes());
        currentRound.AddRange(CharacterManager.Instance.ActiveEnemies);
        currentRound = currentRound.OrderByDescending(c => c.Stats.speed).ToList();

        _currentTurnOrder = currentRound;

        BattleUIManager.Instance.BuildTurnQueue(currentRound);

        foreach (HeroCharBase hero in activeHeroes)
        {
            hero.InitializeTurnIntent(null);
        }

        BattleUIManager.Instance.RefreshAllIntentTexts();
        BattleUIManager.Instance.ShowAllForTransition();
        BattleUIManager.Instance.RefreshAllBoostVisuals();
        BattleUIManager.Instance.ShowCommandPanel(); 
    }

    public void ExecuteAllHeroesActions()
    {
        if (State != BattleState.HeroTurn) return;

        if (targetingSystem != null)
            targetingSystem.StopTargeting();

        BattleUIManager.Instance.CloseActionMenu();
        BattleUIManager.Instance.HideCommandPanel();

        ChangeState(BattleState.ExecutingTurn);
    }

    private IEnumerator ExecutionSequenceCoroutine()
    {
        List<CharacterBase> turnQueue = _currentTurnOrder ?? new List<CharacterBase>();

        BattleUIManager.Instance.BeginExecutionQueueVisual();
        yield return new WaitForSeconds(0.4f);

        foreach (CharacterBase character in turnQueue)
        {
            if (State == BattleState.Victory || State == BattleState.Defeat)
            yield break;

            if (character == null || character.currentHp <= 0 || !character.gameObject.activeInHierarchy) 
            {
                continue;
            }

            // if (character.IsStunned)
            // {
            //     BattleUIManager.Instance.SkipTurnQueue(character);
            //     yield return new WaitForSeconds(0.5f);
            //     continue;
            // }

            bool isActionDone = false;
            if (character is HeroCharBase hero) hero.ExecuteMove(() => isActionDone = true);
            else if (character is EnemyBase enemy) enemy.ExecuteTurn(() => isActionDone = true);

            yield return new WaitUntil(() => isActionDone);

            yield return new WaitForSeconds(0.5f);
        }

        if (State != BattleState.Victory && State != BattleState.Defeat)
        {
            ChangeState(BattleState.HeroTurn);
        }
    }
        
    private void HandleAttackExecution(CharacterBase targetEnemy)
    {
        if (State != BattleState.SelectTarget) return;

        ApplyTargetToAllHeroes(targetEnemy);

        if (targetingSystem != null)
            targetingSystem.StopTargeting();

        State = BattleState.HeroTurn;

        BattleUIManager.Instance.CloseActionMenu();
        BattleUIManager.Instance.ShowCommandPanel();
    }

    public void StartTargetingForHero(HeroCharBase hero)
    {
        if (State != BattleState.HeroTurn && State != BattleState.SelectTarget) return;

        _heroBeingPlanned = hero;
        if (_heroBeingPlanned.CurrentIntent.ChosenSkill == null)
            _heroBeingPlanned.CurrentIntent.ChosenSkill = _heroBeingPlanned.BasicAttackSkill;

        ChangeState(BattleState.SelectTarget); 

        BattleUIManager.Instance.HideCommandPanel();
        targetingSystem.StartTargeting(_heroBeingPlanned.CurrentIntent.Target);      
    }

    public void StopTargetingFromMenu()
    {
        if (State != BattleState.SelectTarget) return;

        if (_heroBeingPlanned != null && targetingSystem != null)
            ApplyTargetToAllHeroes(targetingSystem.GetCurrentTarget());

        if (targetingSystem != null)
            targetingSystem.StopTargeting();

        State = BattleState.HeroTurn;
        BattleUIManager.Instance.ShowCommandPanel();
    }

    public void StopTargetingWithoutApplyingTarget()
    {
        if (State != BattleState.SelectTarget) return;

        if (targetingSystem != null)
            targetingSystem.StopTargeting();

        State = BattleState.HeroTurn;
        BattleUIManager.Instance.ShowCommandPanel();
    }

    private void HandleCancelTargeting()
    {
        if (State != BattleState.SelectTarget) return;

        if (targetingSystem != null)
            targetingSystem.StopTargeting();

        State = BattleState.HeroTurn;
        BattleUIManager.Instance.CloseActionMenu();
        BattleUIManager.Instance.ShowCommandPanel();
    }

    public void ApplyTargetToAllHeroes(CharacterBase targetEnemy)
    {
        if (targetEnemy == null || CharacterManager.Instance.ActiveEnemies == null || !CharacterManager.Instance.ActiveEnemies.Contains(targetEnemy)) return;

        foreach (HeroCharBase hero in GetActiveHeroes())
        {
            if (hero == null) continue;

            hero.CurrentIntent.Target = targetEnemy;
            if (hero.CurrentIntent.ChosenSkill == null)
                hero.CurrentIntent.ChosenSkill = hero.BasicAttackSkill;
        }
    }

    public void ApplyAllyTargetForHero(HeroCharBase caster, HeroCharBase targetHero)
    {
        if (caster == null || targetHero == null) return;
        if (!GetActiveHeroes().Contains(caster)) return;
        if (!GetActiveHeroes().Contains(targetHero)) return;

        caster.CurrentIntent.AllyTarget = targetHero;
    }

    public void CheckBattleEnd()
    {
        if (CharacterManager.Instance.ActiveEnemies.Count == 0)
        {
            ChangeState(BattleState.Victory); 
            return;
        }
        if (CharacterManager.Instance.ActiveHeroes.Count == 0)
        {
            ChangeState(BattleState.Defeat);
            return;
        }
    }

    public List<HeroCharBase> GetActiveHeroes()
    {
        return CharacterManager.Instance.HeroesPhysics; 
    }
}

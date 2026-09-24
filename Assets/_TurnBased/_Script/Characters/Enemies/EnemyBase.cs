using UnityEngine;
using System.Collections; 
using System.Collections.Generic;   
using System;

public class EnemyBase : CharacterBase
{
    private List<ScriptableElement> _weaknesses = new List<ScriptableElement>();
    private List<ScriptableElement> _resistances = new List<ScriptableElement>();
    public List<ScriptableElement> DiscoveredWeaknesses { get; private set; } = new List<ScriptableElement>();
    public event Action OnWeaknessDiscovered; 
    public List<ScriptableElement> Weaknesses => _weaknesses;
    public List<ScriptableElement> Resistances => _resistances;
    [SerializeField] private GameObject attackSwingVfx;
    [SerializeField] private GameObject attackSlashVfx;
    [SerializeField] private GameObject attackSparkVfx;
    [SerializeField] AudioClip attackNormal;

    private bool isDying = false;

    protected override void Awake() 
    {
        base.Awake(); 
    }

    public void SetElementalAffinities(List<ScriptableElement> weakData, List<ScriptableElement> resistData)
    {
        if (weakData != null) _weaknesses = new List<ScriptableElement>(weakData);
        if (resistData != null) _resistances = new List<ScriptableElement>(resistData);
    }
    public bool IsWeakTo(SkillElement element)
    {
        foreach (var weak in _weaknesses)
        {
            if (weak != null && weak.element == element) return true;
        }
        return false;
    }

    public bool IsResistantTo(SkillElement element)
    {
        foreach (var resist in _resistances)
        {
            if (resist != null && resist.element == element) return true;
        }
        return false;
    }

    public void RevealWeakness(ScriptableElement element)
    {
        if (element != null && !DiscoveredWeaknesses.Contains(element))
        {
            DiscoveredWeaknesses.Add(element);
            OnWeaknessDiscovered?.Invoke(); 
        }
    }

    public void ExecuteTurn(System.Action onComplete)
    {
        StartCoroutine(ExecuteAITurnCoroutine(onComplete));
    }

   public override void EvaluateDeathStatus()
    {
        if (currentHp <= 0 && !isDying)
        {
            isDying = true;

            if (CharacterManager.Instance.ActiveEnemies.Contains(this))
            {
                CharacterManager.Instance.ActiveEnemies.Remove(this);
            }

            if (_animator != null)
            {
                _animator.SetTrigger("Die");
            }

            Destroy(gameObject, 0.5f);

            BattleManager.Instance.CheckBattleEnd();
        }
    }
    
    private IEnumerator ExecuteAITurnCoroutine(System.Action onComplete)
    {
        Debug.Log($"{gameObject.name} sedang memikirkan target...");
        
        List<HeroCharBase> activeHeroes = BattleManager.Instance.GetActiveHeroes();
        if (activeHeroes == null || activeHeroes.Count == 0) 
        {
            onComplete?.Invoke();
            yield break;
        }

        HeroCharBase targetHero = activeHeroes[UnityEngine.Random.Range(0, activeHeroes.Count)];
        Vector3 centerStagePos = BattleManager.Instance.ActionCenterPosition;

        if (_animator != null)
        {
            _animator.SetTrigger("Attack");
        }

        yield return StartCoroutine(MoveToPosition(centerStagePos, 0.2f));

        if (attackSwingVfx != null)
        {
            Vector3 swingPos = transform.position + new Vector3(0f, 0.5f, 0f);
            VFXPool.Instance.Get("swing", swingPos, Quaternion.identity, new Vector3(-1f, 1f, 1f));
        }

        yield return new WaitForSeconds(0.3f); 

        
        if (attackSlashVfx != null)
        {
            Vector3 slashPos = targetHero.transform.position + new Vector3(-0.5f, 0.8f, 0f);
            VFXPool.Instance.Get("slash", slashPos, Quaternion.identity, new Vector3(-1f, 1f, 1f));
        }

        if (attackSparkVfx != null)
        {
            Vector3 slashPos = targetHero.transform.position + new Vector3(-0.5f, 0.8f, 0f);
            VFXPool.Instance.Get("spark", slashPos, Quaternion.identity, new Vector3(-1f, 1f, 1f));
        }

        AudioSystem.Instance.PlaySound(attackNormal);

        int damage = Stats.Attack;
        targetHero.PlayHitAnimation();
        targetHero.TakeDamage(damage);
        targetHero.PlayVoice(VoiceType.Hurt);

        targetHero.EvaluateDeathStatus();

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(MoveToPosition(_originalStandPosition, 0.2f));
        
        onComplete?.Invoke();
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; 

public class HeroCharBase : CharacterBase
{
    [Header("Combat Planning")]
    [SerializeField] private ActionIntent currentIntent = new ActionIntent();
    [SerializeField] private ScriptableSkill _basicAttackSkill;
    public ScriptableSkill BasicAttackSkill => _basicAttackSkill;
    private ScriptableHeroVoice _voice;

    [Header("Boost State")]
    private int currentBP = 1; 
    private int allocatedBoost = 0;
    
    public int CurrentBP 
    { 
        get => currentBP; 
        set => currentBP = value; 
    }
    
    public int AllocatedBoost 
    { 
        get => allocatedBoost;
        set => allocatedBoost = value;
    }

    public ActionIntent CurrentIntent => currentIntent;


    private void Start()
    {
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null && spriteRenderer.material != null)
        {
            spriteRenderer.material.SetColor("_OutlineColor", Color.clear);
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public void InitializeTurnIntent(CharacterBase defaultEnemy)
    {
        List<CharacterBase> activeEnemies = CharacterManager.Instance.ActiveEnemies;
        ResetBoost();
        currentIntent.ResetToDefault(activeEnemies, _basicAttackSkill, currentSp);
    }

    public override void InitUnitData(ScriptableBaseCharacter data)
    {
        base.InitUnitData(data); 

        if (data is ScriptableHero heroData)
        {
            _basicAttackSkill = heroData.heroBasicAttack;
            _voice = heroData.voice;
        }
    }

    public void ChangeBoostLevel(int newLevel)
    {
        int prevBoost = allocatedBoost;
        int clamped = Mathf.Clamp(newLevel, 0, BattleManager.MAX_BOOST);
        allocatedBoost = Mathf.Min(clamped, currentBP);

        if (BoostVFXManager.Instance != null)
        {
            BoostVFXManager.Instance.PlayBoostEffect(this, allocatedBoost, prevBoost);
        }

        if (prevBoost == 0 && allocatedBoost > 0)
        {
            PlayVoice(VoiceType.Boost);
        }
    }

    public void ResetBoost()
    {
        allocatedBoost = 0;
    }

    public virtual void ExecuteMove(Action onComplete)
    {
        StartCoroutine(AttackSequenceCoroutine(onComplete));
    }

    private IEnumerator AttackSequenceCoroutine(Action onComplete)
    {
        ScriptableSkill skill = CurrentIntent.ChosenSkill;
        if (skill == null) skill = _basicAttackSkill;

        if (skill != null && skill.spCost > 0) ConsumeSP(skill.spCost);

        Vector3 centerStagePos = BattleManager.Instance.ActionCenterPosition;
        yield return StartCoroutine(MoveToPosition(centerStagePos, 0.2f));

        if (skill.skillCategory == SkillCategory.Recovery)
            yield return StartCoroutine(ExecuteRecoverySkill(skill));
        else if (skill.skillCategory == SkillCategory.Elem)
            yield return StartCoroutine(ExecuteVFXSpell(skill));
        else
            yield return StartCoroutine(ExecuteOffensiveSkill(skill));

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(MoveToPosition(_originalStandPosition, 0.2f));

        OnAttackFinished();
        onComplete?.Invoke();
    }
    private void TryShakeCamera(ScriptableSkill skill)
    {
        if (!skill.doCameraShake) return;
        if (BattleManager.Instance == null || BattleManager.Instance.BattleCamera == null) return;

        CameraShake shake = BattleManager.Instance.BattleCamera.GetComponent<CameraShake>();
        if (shake != null) shake.Shake(skill.shakeDuration, skill.shakeMagnitude);
    }

    private IEnumerator ExecuteOffensiveSkill(ScriptableSkill skill)
    {
        List<CharacterBase> targets = ResolveEnemyTargets(skill);
        if (targets.Count == 0) yield break;

        int totalHits = skill.hitCount + allocatedBoost; 
        Dictionary<CharacterBase, (int damage, DamageEffectiveness effectiveness)> damageData = new Dictionary<CharacterBase, (int, DamageEffectiveness)>();
        
        foreach (CharacterBase targetEnemy in targets)
        {
            if (targetEnemy == null) continue;
            DamageEffectiveness eff;
            int dmg = CalculateDamagePerHit(targetEnemy, skill, out eff);
            damageData[targetEnemy] = (dmg, eff);
        }

        bool isWeaknessHit = false;
        foreach (var data in damageData.Values)
        {
            if (data.effectiveness == DamageEffectiveness.Weak) { isWeaknessHit = true; break; }
        }
        PlayMoveVoice(skill, isWeaknessHit);

        if (!skill.playAnimationPerHit && _animator != null && skill != null)
        {
            _animator.SetTrigger(skill.animTrigger.ToString());
        }

        for (int i = 0; i < totalHits; i++)
        {
            if (skill.playAnimationPerHit && _animator != null && skill != null) 
            {
                _animator.SetTrigger(skill.animTrigger.ToString());
            }
            yield return new WaitForSeconds(skill.swingDelay);

            if (skill.swingVfxPrefab != null)
            {
                Vector3 swingPos = transform.position + new Vector3(-0.5f, 0.5f, 0f);
                VFXPool.Instance.Get("swing", swingPos);
            }


            float currentDelay = (i == 0) ? skill.slashImpactDelay : 
                (skill.playAnimationPerHit ? skill.slashImpactDelay : skill.multiHitInterval);
            
            yield return new WaitForSeconds(currentDelay);

            foreach (CharacterBase targetEnemy in targets)
            {   
                if (targetEnemy == null) continue;
                if (!damageData.TryGetValue(targetEnemy, out var data)) continue;

                if (skill.slashVfxPrefab != null)
                    VFXPool.Instance.Get("slash", targetEnemy.transform.position + new Vector3(0.5f, 0.8f, 0f));

                if (skill.sparkVfxPrefab != null && data.effectiveness == DamageEffectiveness.Weak)
                    VFXPool.Instance.Get("spark", targetEnemy.transform.position + new Vector3(0f, 0.5f, 0f));

                targetEnemy.PlayHitAnimation();
                targetEnemy.TakeDamage(data.damage, data.effectiveness);

                if (AudioSystem.Instance != null)
                {
                    AudioClip sfx = (data.effectiveness == DamageEffectiveness.Weak && skill.attackWeakness != null) ? skill.attackWeakness : skill.attackNormal;
                    if (sfx != null) AudioSystem.Instance.PlayUISound(sfx);
                }
            }
            if (skill.playAnimationPerHit && i < totalHits - 1) yield return new WaitForSeconds(0.4f); 
        }

        yield return new WaitForSeconds(skill.skillFinishDelay); 
        
        foreach (CharacterBase targetEnemy in targets)
            if (targetEnemy is EnemyBase enemyBase) enemyBase.EvaluateDeathStatus();
    }


    private IEnumerator ExecuteRecoverySkill(ScriptableSkill skill)
    {
        List<HeroCharBase> targets = ResolveHeroTargets(skill);
        int healAmount = CalculateRecoveryAmount(skill);

        if (_animator != null && skill != null) 
            _animator.SetTrigger(skill.animTrigger.ToString());

        if (skill.castSound != null && AudioSystem.Instance != null)
            AudioSystem.Instance.PlayUISound(skill.castSound);

        GameObject castVFX = null;
        if (skill.castVfxPrefab != null)
        {
            castVFX = Instantiate(skill.castVfxPrefab, transform.position - new Vector3(0f, 1.2f, 0f), Quaternion.identity);
            yield return new WaitForSeconds(skill.castVfxDuration);
        }

        PlayVoice(VoiceType.Heal);

        foreach (HeroCharBase targetHero in targets)
        {
            if (targetHero == null) continue;

            if (skill.vfxPrefab != null) 
            {
                GameObject vfxHeal = Instantiate(skill.vfxPrefab, targetHero.transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
                ParticleSystem ps = vfxHeal.GetComponent<ParticleSystem>();
                if(ps != null) {
                    ps.transform.Rotate(-90f,0f,0f);
                    ps.Play();
                }
                if (skill.VFXSound != null && AudioSystem.Instance != null)
                    AudioSystem.Instance.PlayUISound(skill.VFXSound);
            }
            targetHero.Heal(healAmount);

            if (targetHero != this)
            {
                yield return new WaitForSeconds(0.3f);
                targetHero.PlayVoice(VoiceType.GettingHealed); 
            }
        }

        yield return new WaitForSeconds(skill.skillFinishDelay);

        if (castVFX != null) Destroy(castVFX);
    }


    private IEnumerator ExecuteVFXSpell(ScriptableSkill skill)
    {
        List<CharacterBase> targets = ResolveEnemyTargets(skill);
        if (targets.Count == 0) yield break;

        if (_animator != null && skill != null) 
        {
            _animator.SetTrigger(skill.animTrigger.ToString());
            _animator.SetBool("IsCasting", true);
        }

        if (skill.castSound != null && AudioSystem.Instance != null)
            AudioSystem.Instance.PlayUISound(skill.castSound);

        GameObject castVFX = null;
        if (skill.castVfxPrefab != null)
        {
            castVFX = Instantiate(skill.castVfxPrefab, transform.position - new Vector3(0f, 1.2f, 0f), Quaternion.identity);
            yield return new WaitForSeconds(skill.castVfxDuration);
        }

        List<GameObject> spawnedVFX = new List<GameObject>();
        if (skill.vfxSpawnLocation == VFXSpawnLocation.ActionCenter)
        {
            if (skill.vfxPrefab != null) spawnedVFX.Add(Instantiate(skill.vfxPrefab, BattleManager.Instance.ActionCenterPosition, Quaternion.identity));
        }
        else
        {
            foreach (CharacterBase target in targets)
            {
                if (target == null || skill.vfxPrefab == null) continue;
                spawnedVFX.Add(Instantiate(skill.vfxPrefab, target.transform.position - new Vector3(0f, .8f, 0f), Quaternion.identity));
            }
        }

        if (skill.VFXSound != null && AudioSystem.Instance != null)
            AudioSystem.Instance.PlayUISound(skill.VFXSound);

        bool isWeaknessHit = false;
        if (skill.skillElement != null)
        {
            foreach (CharacterBase target in targets)
            {
                if (target is EnemyBase enemyTarget && enemyTarget.IsWeakTo(skill.skillElement.element))
                {
                    isWeaknessHit = true;
                    break;
                }
            }
        }
        PlayMoveVoice(skill, isWeaknessHit);

        yield return new WaitForSeconds(skill.vfxImpactDelay);

        int totalHits = skill.hitCount + allocatedBoost; 
        for (int i = 0; i < totalHits; i++)
        {
            foreach (CharacterBase target in targets)
            {
                if (target == null) continue;
                DamageEffectiveness effectiveness;
                int damage = CalculateDamagePerHit(target, skill, out effectiveness);
                target.TakeDamage(damage, effectiveness);
            }

            if (i < totalHits - 1) yield return new WaitForSeconds(skill.multiHitInterval);
        }

        foreach (GameObject vfx in spawnedVFX)
            if (vfx != null) FadeOutAndDestroy(vfx, 0.5f);

        foreach (CharacterBase target in targets)
            if (target is EnemyBase enemyBase) enemyBase.EvaluateDeathStatus();

        if (castVFX != null) FadeOutAndDestroy(castVFX, 1f);

        if (_animator != null) _animator.SetBool("IsCasting", false);

        yield return new WaitForSeconds(skill.skillFinishDelay);
    }

    private void FadeOutAndDestroy(GameObject vfxObject, float maxWaitTime)
    {
        ParticleSystem[] allSystems = vfxObject.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in allSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        Destroy(vfxObject, maxWaitTime);
    }
                

    private List<CharacterBase> ResolveEnemyTargets(ScriptableSkill skill)
    {
        List<CharacterBase> activeEnemies = CharacterManager.Instance.ActiveEnemies;
        List<CharacterBase> targets = new List<CharacterBase>();

        if (activeEnemies == null || activeEnemies.Count == 0) return targets;

        if (skill != null && skill.targetScope == TargetScope.All)
        {
            targets.AddRange(activeEnemies);
            return targets;
        }

        if (CurrentIntent.Target == null ||
            CurrentIntent.Target.currentHp <= 0 ||
            !CurrentIntent.Target.gameObject.activeInHierarchy ||
            !activeEnemies.Contains(CurrentIntent.Target))
        {
            CurrentIntent.Target = activeEnemies[0];
        }

        targets.Add(CurrentIntent.Target);
        return targets;
    }

    private List<HeroCharBase> ResolveHeroTargets(ScriptableSkill skill)
    {
        List<HeroCharBase> activeHeroes = BattleManager.Instance.GetActiveHeroes();
        List<HeroCharBase> targets = new List<HeroCharBase>();

        if (activeHeroes == null || activeHeroes.Count == 0) return targets;

        if (skill != null && skill.targetScope == TargetScope.All)
        {
            targets.AddRange(activeHeroes);
            return targets;
        }

        if (skill != null && skill.targetScope == TargetScope.Self)
        {
            targets.Add(this);
            return targets;
        }

        if (CurrentIntent.AllyTarget == null ||
            CurrentIntent.AllyTarget.currentHp <= 0 ||
            !CurrentIntent.AllyTarget.gameObject.activeInHierarchy ||
            !activeHeroes.Contains(CurrentIntent.AllyTarget))
        {
            CurrentIntent.AllyTarget = this;
        }

        targets.Add(CurrentIntent.AllyTarget);
        return targets;
    }

    private int CalculateRecoveryAmount(ScriptableSkill skill)
    {
        int skillPower = skill != null ? skill.basePower : 0;
        return Mathf.Max(1, Stats.Attack + skillPower);
    }

    public int CalculateDamagePerHit(CharacterBase targetEnemy, ScriptableSkill chosenSkill, out DamageEffectiveness effectiveness)
    {
        int attackerAtk = this.Stats.Attack; 
        int skillPower = chosenSkill != null ? chosenSkill.basePower : 0;
        int hitCount = (chosenSkill != null && chosenSkill.hitCount > 0) ? chosenSkill.hitCount : 1;
        float baseDamage = attackerAtk + ((float)skillPower / hitCount);
        float multiplier = 1.0f;
        
        effectiveness = DamageEffectiveness.None;

        if (chosenSkill != null && chosenSkill.skillElement != null && targetEnemy is EnemyBase enemy)
        {
            SkillElement skillType = chosenSkill.skillElement.element;
            
            if (enemy.IsWeakTo(skillType)) 
            {
                multiplier = 1.5f; 
                effectiveness = DamageEffectiveness.Weak; 
                
                enemy.RevealWeakness(chosenSkill.skillElement); 
            }
            else if (enemy.IsResistantTo(skillType)) 
            {
                multiplier = 0.5f;
                effectiveness = DamageEffectiveness.Resist;
            }
        }

        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);
        return Mathf.Max(1, finalDamage); 
    }
    public override void EvaluateDeathStatus()
    {
        if (currentHp <= 0)
        {
            if (_animator != null)
            {
                _animator.SetTrigger("Die");
            }

            CharacterManager.Instance.HeroesPhysics.Remove(this);
            CharacterManager.Instance.ActiveHeroes.Remove(this);

            if (CharacterManager.Instance.HeroesPhysics.Count == 0)
            {
                BattleManager.Instance.ChangeState(BattleState.Defeat);
            }
            PlayVoice(VoiceType.Die);  
        }
    }

    private void OnAttackFinished()
    {
        if (allocatedBoost > 0)
        {
            currentBP = Mathf.Max(0, currentBP - allocatedBoost);
        }

        ResetBoost();

        if (BoostVFXManager.Instance != null) BoostVFXManager.Instance.StopHeroEffect(this);

        List<CharacterBase> updatedEnemies = CharacterManager.Instance.ActiveEnemies;
        currentIntent.ResetToDefault(updatedEnemies, _basicAttackSkill, currentSp);
    }

    private void PlayMoveVoice(ScriptableSkill skill, bool isWeaknessHit)
    {
        if (skill == null) return;

        switch (skill.animTrigger)
        {
            case SkillAnimTrigger.Special:
                PlayVoice(VoiceType.Special);
                break;
            case SkillAnimTrigger.Skill:
                PlayVoice(VoiceType.Skill);
                break;
            default:
                PlayVoice(isWeaknessHit ? VoiceType.AttackWeakness : VoiceType.Attack);
                break;
        }
    }

    public void PlayVoice(VoiceType type)
    {
        if (_voice == null) return;

        AudioClip clip = type switch
        {
            VoiceType.Attack => _voice.GetRandom(_voice.attack),
            VoiceType.AttackWeakness => _voice.GetRandom(_voice.attackWeakness),
            VoiceType.Skill => _voice.GetRandom(_voice.skill),
            VoiceType.Special => _voice.GetRandom(_voice.special),
            VoiceType.GettingHealed => _voice.GetRandom(_voice.gettingHealed),
            VoiceType.Heal => _voice.GetRandom(_voice.heal),
            VoiceType.Hurt => _voice.GetRandom(_voice.hurt),
            VoiceType.Die => _voice.GetRandom(_voice.die),
            VoiceType.MyTurn => _voice.GetRandom(_voice.myTurn),
            VoiceType.Boost => _voice.GetRandom(_voice.boost),
            _ => null
        };

        if (clip != null && charAudioSource != null)
        {
            charAudioSource.Stop();
            charAudioSource.clip = clip;
            charAudioSource.Play();
        }
    }
}
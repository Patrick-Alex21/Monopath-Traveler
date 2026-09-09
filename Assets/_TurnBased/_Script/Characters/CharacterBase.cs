using System.Collections;
using UnityEngine;
using System;


public class CharacterBase : MonoBehaviour, IDamageable
{
    public Stats Stats { get; private set; }
    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnSpChanged;
    public event Action<CharacterBase> OnDeath;
    public ScriptableBaseCharacter CharacterData { get; private set; }
    
    [Header("Current Status")]
    public int currentHp { get; protected set; } 
    public int currentSp { get; protected set; }
    protected Animator _animator;
    public AudioSource charAudioSource { get; protected set; }
    public AudioSource voiceAudioSource { get; protected set; }
    protected Vector3 _originalStandPosition;
    protected SnapToGround _snapToGround;
    private DamageFeedback _damageFeedback;
    
    protected virtual void Awake()
    {
        charAudioSource = gameObject.AddComponent<AudioSource>();
        charAudioSource.playOnAwake = false;
        charAudioSource.spatialBlend = 0f;

        voiceAudioSource = gameObject.AddComponent<AudioSource>();
        voiceAudioSource.playOnAwake = false;
        voiceAudioSource.spatialBlend = 0f;

        _originalStandPosition = transform.position;
        _snapToGround = GetComponent<SnapToGround>();
        _damageFeedback = GetComponent<DamageFeedback>();
        
        
        _animator = GetComponent<Animator>();
    }

    public virtual void InitUnitData(ScriptableBaseCharacter data) 
    {
        CharacterData = data;
        
        Stats = data.BaseStats;
        currentHp = Stats.maxHp;
        currentSp = Stats.maxSp;


        if (_animator != null && data.animatorOverride != null)
        {
            _animator.runtimeAnimatorController = data.animatorOverride;
        }

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && data.DefaultSprite != null)
        {
            sr.sprite = data.DefaultSprite;
        }

        StartCoroutine(UpdateColliderSize());

        OnHealthChanged?.Invoke(currentHp, Stats.maxHp);
        OnSpChanged?.Invoke(currentSp, Stats.maxSp);
    }

    public virtual void SetStats(Stats stats)
    {
        Stats = stats;
        currentHp = stats.maxHp;
        currentSp = stats.maxSp;
        OnHealthChanged?.Invoke(currentHp, Stats.maxHp);
        OnSpChanged?.Invoke(currentSp, Stats.maxSp);
    }

    public void RestoreState(int hp, int sp)
    {
        currentHp = Mathf.Clamp(hp, 0, Stats.maxHp);
        currentSp = Mathf.Clamp(sp, 0, Stats.maxSp);
        OnHealthChanged?.Invoke(currentHp, Stats.maxHp);
        OnSpChanged?.Invoke(currentSp, Stats.maxSp);
    }

    private IEnumerator UpdateColliderSize()
    {
        yield return new WaitForEndOfFrame(); 

        Collider col = GetComponent<Collider>(); 
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

        if (col != null && sr != null && sr.sprite != null)
        {
            Vector2 spriteSize = sr.sprite.bounds.size;
            
            if (col is SphereCollider sphereCol)
            {
                sphereCol.radius = Mathf.Max(spriteSize.x, spriteSize.y) * 0.5f;
            }
        }
    }

    public void RegenerateSP(int amount)
    {
        currentSp = Mathf.Min(currentSp + amount, Stats.maxSp);
        
        OnSpChanged?.Invoke(currentSp, Stats.maxSp);
    }

    public virtual void PlayHitAnimation()
    {
        if (_animator != null)
            _animator.SetTrigger("Hit");
    }

    public virtual void Heal(int amount)
    {
        Vector3 basePos = transform.position + new Vector3(0, 0.5f, 0);
        currentHp = Mathf.Min(currentHp + amount, Stats.maxHp);
        OnHealthChanged?.Invoke(currentHp, Stats.maxHp);

        if (DamagePopupPool.Instance != null)
        {
            DamagePopupPool.Instance.Get(basePos + new Vector3(0, 1.5f, 0), amount.ToString(), Color.green, -0.4f, 0f);
        }

    }

    public virtual void TakeDamage(int damage, DamageEffectiveness effectiveness = DamageEffectiveness.None)
    {
        currentHp -= damage;
        if (currentHp < 0) currentHp = 0;
        
        OnHealthChanged?.Invoke(currentHp, Stats.maxHp);

        if (currentHp <= 0)
            OnDeath?.Invoke(this);

        if (_damageFeedback != null)
        {
            _damageFeedback.PlayHitReaction();
        }

        Vector3 basePos = transform.position + new Vector3(0, 1.5f, 0);

        if (DamagePopupPool.Instance != null)
        {
            DamagePopupPool.Instance.Get(basePos, damage.ToString(), Color.white, -0.4f, 0f);
        }

        if (effectiveness != DamageEffectiveness.None && DamagePopupPool.Instance != null)
        {
            if (effectiveness == DamageEffectiveness.Weak)
            {
                DamagePopupPool.Instance.Get(basePos, "WEAK!", Color.red, 0.5f, 0.4f);
            }
            else if (effectiveness == DamageEffectiveness.Resist)
            {
                DamagePopupPool.Instance.Get(basePos, "RESIST", Color.gray, 0.5f, 0.4f);
            }
        }
    }

    public virtual void ConsumeSP(int cost)
    {
        currentSp -= cost;
        if (currentSp < 0) currentSp = 0;
        
        OnSpChanged?.Invoke(currentSp, Stats.maxSp);
    }

    public virtual void EvaluateDeathStatus()
    {
        if (currentHp <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    protected IEnumerator MoveToPosition(Vector3 targetPos, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration); 
            
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            if (_snapToGround != null) _snapToGround.Snap();

            yield return null;
        }

        transform.position = targetPos;
        if (_snapToGround != null) _snapToGround.Snap();
    }
}

using System.Collections.Generic;
using UnityEngine;

public class BoostVFXManager : MonoBehaviour
{
    public static BoostVFXManager Instance { get; private set; }

    [Header("VFX Prefab")]
    [SerializeField] private GameObject boostLightVFXPrefab;

    [Header("UI & System SFX (One-Shot)")]
    [SerializeField] private AudioClip castClip;       
    [SerializeField] private AudioClip countUpClip;    
    [SerializeField] private AudioClip countFullClip;  
    [SerializeField] private AudioClip cancelClip;     

    [Header("Burst SFX (One-Shot)")]
    [SerializeField] private AudioClip[] burstClips;   

    [Header("Aura SFX (Looping)")]
    [SerializeField] private AudioClip[] loopClips;    

    private int maxBoostLevel = 3;

    private Dictionary<HeroCharBase, GameObject> activeVFX = new Dictionary<HeroCharBase, GameObject>();
    private Dictionary<HeroCharBase, AudioSource> activeLoops = new Dictionary<HeroCharBase, AudioSource>();

    private readonly int outlineColorID = Shader.PropertyToID("_OutlineColor");

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayBoostEffect(HeroCharBase hero, int currentLevel, int prevLevel)
    {
        if (hero == null) return;

        SpriteRenderer heroSprite = hero.GetComponentInChildren<SpriteRenderer>();

        if (currentLevel == 0)
        {
            if (AudioSystem.Instance != null) AudioSystem.Instance.PlaySound(cancelClip);
            StopHeroEffect(hero); 
            return;
        }

        if (currentLevel > prevLevel && AudioSystem.Instance != null)
        {
            if (currentLevel == 1) AudioSystem.Instance.PlaySound(castClip);
            else if (currentLevel == maxBoostLevel) AudioSystem.Instance.PlaySound(countFullClip);
            else AudioSystem.Instance.PlaySound(countUpClip);
        }

        GameObject lightVFX;
        
        if (!activeVFX.ContainsKey(hero))
        {
            Vector3 spawnPos = hero.transform.position + new Vector3(0f, 0.4f, 0f); 

            lightVFX = Instantiate(boostLightVFXPrefab, spawnPos, Quaternion.identity);
            lightVFX.transform.SetParent(hero.transform);
            
            activeVFX[hero] = lightVFX;

            AudioSource newLoop = hero.gameObject.AddComponent<AudioSource>();
            newLoop.loop = true;
            newLoop.playOnAwake = false;
            activeLoops[hero] = newLoop;
        }
        else
        {
            lightVFX = activeVFX[hero];
        }
        
        Light vfxLight = lightVFX.GetComponent<Light>();
        LightningFade fadeScript = lightVFX.GetComponent<LightningFade>();
        AudioSource loopSource = activeLoops[hero];

        if (vfxLight != null && fadeScript != null)
        {
            float targetMinInt = 0f;
            float targetMaxInt = 0f; 
            Color baseColor = Color.white;
            float outlineGlowIntensity = 2.5f;

            if (currentLevel == 1)
            {
                targetMinInt = 70f;
                targetMaxInt = 1000f;
                baseColor = new Color(1f, 0.2f, 0.3f);
            }
            else if (currentLevel == 2)
            {   
                
                targetMinInt = 100f; 
                targetMaxInt = 1500f;
                
                baseColor = new Color(1f, 0.8f, 0.2f); 
            }
            else if (currentLevel == 3)
            {
                targetMinInt = 1000f;
                targetMaxInt = 10000f; 
                
                baseColor = new Color(0f, 0.3f, 1f); 
                
                outlineGlowIntensity = 4.0f;
            }
            
            vfxLight.color = baseColor; 

            fadeScript.UpdateIntensityConfig(targetMaxInt, targetMinInt);

            if (heroSprite != null)
            {
                Color hdrOutlineColor = baseColor * outlineGlowIntensity;
                heroSprite.material.SetColor(outlineColorID, hdrOutlineColor);
            }

            if (burstClips.Length >= currentLevel && burstClips[currentLevel - 1] != null && hero.charAudioSource != null)
            {
                hero.charAudioSource.PlayOneShot(burstClips[currentLevel - 1]);
            }

            if (loopClips.Length >= currentLevel && loopClips[currentLevel - 1] != null)
            {
                loopSource.clip = loopClips[currentLevel - 1];
                if (!loopSource.isPlaying) loopSource.Play(); 
            }
        }
    }

    public void StopHeroEffect(HeroCharBase hero)
    {
        if (hero == null) return;

        SpriteRenderer heroSprite = hero.GetComponentInChildren<SpriteRenderer>();

        if (heroSprite != null)
        {
            heroSprite.material.SetColor(outlineColorID, Color.clear);
        }

        if (activeLoops.ContainsKey(hero))
        {
            activeLoops[hero].Stop();
            Destroy(activeLoops[hero]); 
            activeLoops.Remove(hero);
        }

        if (activeVFX.ContainsKey(hero))
        {
            Destroy(activeVFX[hero]); 
            activeVFX.Remove(hero);
        }
    }

    public void StopAllEffects()
    {
        List<HeroCharBase> heroesToStop = new List<HeroCharBase>(activeVFX.Keys);
        foreach (HeroCharBase hero in heroesToStop)
        {
            StopHeroEffect(hero);
        }
    }
}

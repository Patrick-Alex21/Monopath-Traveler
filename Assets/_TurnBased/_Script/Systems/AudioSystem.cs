using UnityEngine;

public class AudioSystem : Singleton<AudioSystem> 
{
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _soundSource;

    private float _lastUiSoundTime;
    private AudioClip _lastUiClip;

    
    public void PlayMusic(AudioClip clip)
    {
        _musicSource.clip = clip;
        _musicSource.Play();
    }

    public void PlayUISound(AudioClip clip)
    {
        if (clip == null) return;

        if (clip == _lastUiClip && Time.time - _lastUiSoundTime < 0.05f) 
            return; 

        _lastUiClip = clip;
        _lastUiSoundTime = Time.time;

        _soundSource.pitch = 1f;
        _soundSource.PlayOneShot(clip, 1f);
    }

    public void PlaySound(AudioClip clip, float vol = 0.6f)
    {
        if (clip == null) return;

        
        if (clip == _lastUiClip && Time.time - _lastUiSoundTime < 0.05f) 
            return;

        _lastUiClip = clip;
        _lastUiSoundTime = Time.time;

        _soundSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        _soundSource.PlayOneShot(clip, vol);
    }

    public void StopMusic()
    {
        _musicSource.Stop();
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioShotManager audioShotManager;
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private bool resetAudioOnAwake, resetAudioOnDestroy;
    
    private AudioSource _audioSource;
    private UnityEvent _onComplete;
    private Coroutine _waitForEndCoroutine, _waitForStartCoroutine;
    private WaitForSeconds _clipLength, _delay;
    private int _shotIndex;

    private void Awake() 
    {
        if (resetAudioOnDestroy) ResetAllAudioShots();
    }

    private void Start()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.outputAudioMixerGroup = mixerGroup;
        
        PlayAwakeAudio();
    }

    private void ConfigureAudioSource(int priority, float volume, float pitch, float spatialBlend,
        float minDistance, float maxDistance)
    {
        StopAudio();
        _audioSource.priority = priority;
        _audioSource.volume = volume;
        _audioSource.pitch = pitch;
        _audioSource.spatialBlend = spatialBlend;
        _audioSource.minDistance = minDistance;
        _audioSource.maxDistance = maxDistance;
    }
    
    private void PlayAwakeAudio()
    {
        foreach (var audioShot in audioShotManager.audioShots)
        {
            if (!audioShot.playOnAwake) continue;
            ConfigureAudioSource(audioShot.priority, audioShot.volume, audioShot.pitch, audioShot.spatialBlend,
                audioShot.minDistance, audioShot.maxDistance);
            _audioSource.PlayOneShot(audioShot.clip);
        }
    }

    public void PlayAudioShot(string id)
    {
        _shotIndex = audioShotManager.audioShots.FindIndex(shot => shot.id == id);
        if (_shotIndex == -1)
        {
            Debug.LogError($"Audio Shot with ID {id} not found in Audio Shot Manager.", this);
            return;
        }
        PlayAudioShot(_shotIndex);
    }
    
    public void PlayAudioShot(int index)
    {
        if (audioShotManager == null || _audioSource == null) return;
        
        _shotIndex = index;
        var audioShot = audioShotManager.audioShots[index];
        if (audioShot.clip == null || (audioShot.hasPlayed && audioShot.playOnlyOncePerGame)) return;
        
        ConfigureAudioSource(audioShot.priority, audioShot.volume, audioShot.pitch, audioShot.spatialBlend,
            audioShot.minDistance, audioShot.maxDistance);
        if (audioShot.delay > 0)
        {
            _delay = new WaitForSeconds(audioShot.delay);
            _waitForStartCoroutine ??= StartCoroutine(WaitForStartDelay(_delay, index));
        }
        else
        {
            audioShotManager.PlayAudio(_audioSource, index);
        }
        if (audioShot.onComplete != null)
        {
            _onComplete = audioShot.onComplete;
            _clipLength = new WaitForSeconds(audioShot.clip.length + audioShot.delay);
            _waitForEndCoroutine ??= StartCoroutine(WaitForClipEnd(_clipLength));
        }
    }

    private IEnumerator WaitForStartDelay(WaitForSeconds delay, int index)
    {
        yield return delay;
        _waitForStartCoroutine = null;
        audioShotManager.PlayAudio(_audioSource, index);
    }

    private IEnumerator WaitForClipEnd(WaitForSeconds clipLength)
    {
        yield return clipLength;
        _onComplete?.Invoke();
        _waitForEndCoroutine = null;
    }
    
    public void StopAudio()
    {
        if (_audioSource != null) _audioSource.Stop();
        if (_waitForStartCoroutine != null)
        {
            StopCoroutine(_waitForStartCoroutine);
            _waitForStartCoroutine = null;
            audioShotManager.audioShots[_shotIndex].hasPlayed = true;
        }

        if (_waitForEndCoroutine == null) return;
        StopCoroutine(_waitForEndCoroutine);
        _onComplete?.Invoke();
        _waitForEndCoroutine = null;
    }
    
    public void PauseAudio()
    {
        if (_audioSource != null) _audioSource.Pause();
    }

    public void ResetAllAudioShots()
    {
        if (audioShotManager != null) audioShotManager.ResetAllAudioShots();
    }

    public void ResetAudioShot(string id)
    {
        if (audioShotManager != null) audioShotManager.ResetAudioShot(id);
    }

    public void ResetAudioShot(int index)
    {
        if (audioShotManager != null) audioShotManager.ResetAudioShot(index);
    }

    public void OnDestroy()
    {
        StopAudio();
        if (resetAudioOnDestroy) ResetAllAudioShots();
    }
}
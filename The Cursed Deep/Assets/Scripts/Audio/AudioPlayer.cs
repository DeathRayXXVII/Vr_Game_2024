using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioShotData audioShotData;
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private bool resetAudioOnAwake, resetAudioOnDestroy;
    
    private AudioSource _audioSource;
    private AudioShotData.AudioShot _currentAudioShot;
    private Coroutine _waitForEndCoroutine, _waitForStartCoroutine;
    private WaitForFixedUpdate _waitFixedUpdate;
    private WaitForSeconds _waitClipLength;
    private WaitForSeconds _waitForDelay;

    private void Awake() 
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.outputAudioMixerGroup = mixerGroup;
        _audioSource.playOnAwake = false;
        if (resetAudioOnAwake) ResetAllAudioShots();
    }

    private void Start() => PlayAwakeAudio();

    private void ConfigureAudioSource(AudioShotData.AudioShot audioShot)
    {
        StopAudio();
        
        _audioSource.priority = audioShot.priority;
        _audioSource.volume = audioShot.volume;
        _audioSource.pitch = audioShot.pitch;
        _audioSource.spatialBlend = audioShot.spatialBlend;
        _audioSource.minDistance = audioShot.minDistance;
        _audioSource.maxDistance = audioShot.maxDistance;
    }
    
    private void PlayAwakeAudio()
    {
        foreach (var audioShot in audioShotData.audioShots.Where(audioShot => audioShot.playOnAwake))
        {
            ConfigureAudioSource(audioShot);
            _audioSource.PlayOneShot(audioShot.clip);
        }
    }

    public void PlayAudioShot(string id) => PlayAudioShot(audioShotData.GetAudioShotIndex(id));
    public void PlayAudioShot(int index)
    {
        audioShotData.ValidateAudioShot(out var audioShot, index:index);
        PlayAudioShot(audioShot, index);
    }

    private void PlayAudioShot(AudioShotData.AudioShot audioShot, int index)
    {
        if (audioShot == null || !audioShotData.IsValidIndex(index))
        {
            Debug.LogError("Audio Shot is invalid. Cannot play audio.", this);
            return;
        }
        if (audioShotData == null || _audioSource == null) return;
        
        var audioClip = audioShot.clip;
        if (audioClip == null || audioShot.locked) return;
        
        ConfigureAudioSource(audioShot);
        
        _currentAudioShot = audioShot;
        
        var startDelay = _currentAudioShot.delay;
        if (startDelay > 0)
        {
            _waitForDelay = new WaitForSeconds(startDelay);
            _waitForStartCoroutine ??= StartCoroutine(WaitForStartDelay(_waitForDelay, index));
        }
        else
        {
            audioShotData.PlayAudio(_audioSource, index);
        }
        
        if (_currentAudioShot.onComplete != null)
        {
            _waitClipLength = new WaitForSeconds(audioClip.length + startDelay);
            _waitForEndCoroutine ??= StartCoroutine(WaitForClipEnd(_waitClipLength));
        }
    }

    private IEnumerator WaitForStartDelay(WaitForSeconds delay, int index)
    {
        yield return delay;
        audioShotData.PlayAudio(_audioSource, index);
        _waitForStartCoroutine = null;
    }

    private IEnumerator WaitForClipEnd(WaitForSeconds clipLength)
    {
        yield return clipLength;
        ProcessClipEnd();
    }
    
    public void StopAudio()
    {
        if (_audioSource != null) _audioSource.Stop();

        if (_waitForEndCoroutine == null) return;
        
        StopCoroutine(_waitForEndCoroutine);
        ProcessClipEnd();
    }
    
    private bool _isProcessingClipEnd;
    private void ProcessClipEnd()
    {
        if (_isProcessingClipEnd) return;
        _isProcessingClipEnd = true;
        
        _waitForEndCoroutine = null;
        _currentAudioShot?.onComplete?.Invoke();
        
        _isProcessingClipEnd = false;
    }
    
    public void PauseAudio()
    {
        if (_audioSource != null) _audioSource.Pause();
    }

    public void ResetAllAudioShots()
    {
        if (audioShotData != null) audioShotData.ResetAllAudioShots();
    }

    public void ResetAudioShot(string id)
    {
        if (audioShotData != null) audioShotData.ResetAudioShot(id);
    }

    public void ResetAudioShot(int index)
    {
        if (audioShotData != null) audioShotData.ResetAudioShot(index);
    }

    public void OnDestroy()
    {
        StopAudio();
        if (resetAudioOnDestroy) ResetAllAudioShots();
    }
}
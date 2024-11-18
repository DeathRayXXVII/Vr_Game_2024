using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using ZPTools.Interface;

[CreateAssetMenu(fileName = "AudioShotManager", menuName = "Audio/Audio Shot Manager", order = 0)]
public class AudioShotData : ScriptableObject, IResetOnNewGame, INeedButton
{
    private const int DefaultPriority = 128;
    private const float DefaultVolume = 1.0f;
    private const float DefaultPitch = 1.0f;
    private const float DefaultSpatialBlend = 0f;
    private const float DefaultMinDistance = 1f;
    private const float DefaultMaxDistance = 500f;
    
    [System.Serializable]
    public class AudioShot
    {
        [Header("Audio Shot")]
        public string id;
        [Header("Audio Clip Settings")]
        public AudioClip clip;
        public bool playOnlyOncePerGame;
        [SerializeField, ReadOnly] private bool _locked;
        public bool playOnAwake;
        [Header("Audio Settings")]
        [Range(0, 256)] public int priority = DefaultPriority;
        [Range(0, 1)] public float volume = DefaultVolume;
        [Range(-3, 3)] public float pitch = DefaultPitch;
        [LabeledRange(0, 1, "2D", "3D")] public float spatialBlend = DefaultSpatialBlend;
        public float minDistance = DefaultMinDistance;
        public float maxDistance = DefaultMaxDistance;
        
        [Header("Runtime Settings")]
        public float delay;
        public UnityEvent onComplete;

        public bool locked
        {
            get => playOnlyOncePerGame && _locked;
            set => _locked = playOnlyOncePerGame && value;
        }
        
        public void SetLocked(bool lockState)
        {
            if (lockState) playOnlyOncePerGame = true;
            _locked = lockState;
        }
        
        public void Activated()
        { 
            // Will only be true if playOnlyOncePerGame is true
            locked = true;
        }
    }

    public List<AudioShot> audioShots;
    
    public bool IsValidIndex(int index) => index >= 0 && index < audioShots.Count;
    
    public void PlayAudio(AudioSource audioSource, string id)
    {
        var index = audioShots.FindIndex(shot => shot.id == id);
        PlayAudio(audioSource, index);
    }
    
    public AudioShot ValidateAudioShot(out AudioShot audioShot, string id = "", int index = -1)
    {
        audioShot = null;
        if (index == -1)
        {
            index = GetAudioShotIndex(id);
        }
        if (IsValidIndex(index))
        {
            audioShot = audioShots[index];
            audioShot.priority = audioShot.priority <= 0 ? DefaultPriority : audioShot.priority;
            audioShot.volume = audioShot.volume <= 0 ? DefaultVolume : audioShot.volume;
            audioShot.pitch = Mathf.Abs(audioShot.pitch) <= 0 ? DefaultPitch : audioShot.pitch;
            audioShot.minDistance = audioShot.minDistance <= 0 ? DefaultMinDistance : audioShot.maxDistance;
            audioShot.maxDistance = audioShot.maxDistance <= audioShot.minDistance ? DefaultMaxDistance : audioShot.maxDistance;
            audioShot.delay = audioShot.delay < 0 ? 0 : audioShot.delay;
        }
        return audioShot;
    }
    
    public int GetAudioShotIndex(string id) => audioShots.FindIndex(shot => shot.id == id);

    public void PlayAudio(AudioSource audioSource, int index)
    {
        if (!IsValidIndex(index) || audioSource == null) return;
        
        var shot = audioShots[index];
        if (shot.locked || shot.clip == null) return;
        
        audioSource.PlayOneShot(shot.clip);
        shot.Activated();
    }

    public void ResetAudioShot(string id) => ProcessLockState(id, false);
    public void ResetAudioShot(int index) => ProcessLockState(index, false);
    public void LockAudioShot(string id) => ProcessLockState(id, true);
    public void LockAudioShot(int index) => ProcessLockState(index, true);
    
    private void ProcessLockState(string id, bool setTo)
    {
        var index = audioShots.FindIndex(shot => shot.id == id);
        ProcessLockState(index, setTo);
    }

    private void ProcessLockState(int index, bool setTo)
    {
        if (!IsValidIndex(index)) return;

        audioShots[index].locked = setTo;
    }

    public void ResetAllAudioShots()
    {
        foreach (var shot in audioShots)
        {
            shot.locked = false;
        }
    }
    
    public void LockAllPlayPlayOnceAudioShots()
    {
        foreach (var shot in audioShots.Where(shot => shot.playOnlyOncePerGame))
        {
            shot.locked = true;
        }
    }

    public void ResetToNewGameValues(int tier = 2)
    {
        if (tier < 2) return;
        ResetAllAudioShots();
    }

    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)> {(ResetAllAudioShots, "Reset All Audio Shots")};
    }
}
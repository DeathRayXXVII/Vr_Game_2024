using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using ZPTools.Interface;

[CreateAssetMenu(fileName = "AudioShotManager", menuName = "Audio/Audio Shot Manager", order = 0)]
public class AudioShotData : ScriptableObject, IResetOnNewGame, INeedButton
{
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
        [Range(0, 256)] public int priority = 128;
        [Range(0, 1)] public float volume = 1f;
        [Range(-3, 3)] public float pitch = 1f;
        [LabeledRange(0, 1, "2D", "3D")] public float spatialBlend;
        public float minDistance = 1f;
        public float maxDistance = 500f;
        
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
            
            audioShot.priority = audioShot.priority <= 0 ? 128 : audioShot.priority;
            audioShot.volume = audioShot.volume <= 0 ? 1f : audioShot.volume;
            audioShot.pitch = Mathf.Approximately(audioShot.pitch, 0) ? 1f : audioShot.pitch;
            audioShot.minDistance = audioShot.minDistance <= 0 ? 1f : audioShot.minDistance;
            audioShot.maxDistance = audioShot.maxDistance == 0 ?
                500f : audioShot.maxDistance <= audioShot.minDistance ?
                    audioShot.minDistance : audioShot.maxDistance;
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
    
    public void LockAllPlayOnlyOnceAudioShots()
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
        return new List<(System.Action, string)> {(ResetAllAudioShots, "Unlock All Audio Shots"),
            (LockAllPlayOnlyOnceAudioShots, "Lock All Play Only Once Audio Shots")};
    }
}
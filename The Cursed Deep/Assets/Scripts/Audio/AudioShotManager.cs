using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using ZPTools.Interface;

[CreateAssetMenu(fileName = "AudioShotManager", menuName = "Audio/Audio Shot Manager", order = 0)]
public class AudioShotManager : ScriptableObject, IResetOnNewGame, INeedButton
{
    [System.Serializable]
    public class AudioShot
    {
        [Header("Audio Shot")]
        public string id;
        [Header("Audio Clip Settings")]
        public AudioClip clip;
        public bool playOnlyOncePerGame;
        public bool hasPlayed;
        public bool playOnAwake;
        [Header("Audio Settings")]
        [Range(0, 256)] public int priority = 128;
        [Range(0, 1)] public float volume = 1.0f;
        [Range(-3, 3)] public float pitch = 1.0f;
        [LabeledRange(0, 1, "2D", "3D")] public float spatialBlend;
        public float minDistance = 1f;
        public float maxDistance = 500f;
        
        [Header("Runtime Settings")]
        public float delay;
        public UnityEvent onComplete;
    }

    public List<AudioShot> audioShots;
    private Coroutine _waitForEndCoroutine;
    
    public void PlayAudio(AudioSource audioSource, string id)
    {
        var index = audioShots.FindIndex(shot => shot.id == id);
        PlayAudio(audioSource, index);
    }

    public void PlayAudio(AudioSource audioSource, int index)
    {
        if (index < 0 || index >= audioShots.Count) return;
        var shot = audioShots[index];
        if ((shot.hasPlayed && shot.playOnlyOncePerGame) || shot.clip == null || audioSource == null) return;
        audioSource.PlayOneShot(shot.clip);
        shot.hasPlayed = true;
    }

    public void ResetAllAudioShots()
    {
        foreach (var shot in audioShots)
        {
            shot.hasPlayed = false;
        }
    }

    public void ResetAudioShot(string id) => ProcessAudioShot(id, false);
    public void ResetAudioShot(int index) => ProcessAudioShot(index, false);
    public void LockAudioShot(string id) => ProcessAudioShot(id, true);
    public void LockAudioShot(int index) => ProcessAudioShot(index, true);
    
    private void ProcessAudioShot(string id, bool setTo)
    {
        var index = audioShots.FindIndex(shot => shot.id == id);
        ProcessAudioShot(index, setTo);
    }

    private void ProcessAudioShot(int index, bool setTo)
    {
        if (index < 0 || index >= audioShots.Count) return;

        audioShots[index].hasPlayed = setTo;
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
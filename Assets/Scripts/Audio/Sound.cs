using System.Collections;
using UnityEngine;

[System.Serializable]
public class SoundData {
    public string name;
    public AudioClip audioClip;
    [Range(0.1f, 1.5f)]
    public float pitchMin = 0.8f;
    [Range(0.1f, 1.5f)]
    public float pitchMax = 1.2f;
    [Range(0f, 1f)]
    public float volume = 1f;
    public float start = 0.3f;
    public float duration = 1f;
}

public class Sound : MonoBehaviour {
    private AudioSource audioSource_;
    private bool playing_ = false;

    private void Awake() {
        audioSource_ = GetComponent<AudioSource>();
    }

    public void Play(SoundData sound) {
        audioSource_.pitch = Random.Range(sound.pitchMin, sound.pitchMin);
        audioSource_.volume = sound.volume;
        audioSource_.clip = sound.audioClip;
        audioSource_.Play();
        playing_ = true;
    }

    public void Stop() {
        audioSource_.Stop();
        audioSource_.pitch = 0;
        audioSource_.volume = 0;
        audioSource_.clip = null;
        playing_ = false;
    }

    public bool playing { get { return playing_; } }
}

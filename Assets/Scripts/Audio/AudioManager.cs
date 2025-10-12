using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

public class AudioManager : Singleton<AudioManager> {
    [Header("Sound.Basic")]
    [SerializeField] private SoundData[] sounds_;
    [SerializeField] private int capacity_ = 10;
    [SerializeField] private int maxSize_ = 100;

    [Header("Sound.Scene")]
    [SerializeField] private SceneSoundData[] sceneSounds_;
    [SerializeField] private AudioMixer audioMixer_ = null;
    [SerializeField] private AudioSource ambientAudio_ = null;
    [SerializeField] private AudioSource musicAudio_ = null;
    [SerializeField] private AudioMixerSnapshot musicSnapshot_ = null;
    [SerializeField] private AudioMixerSnapshot ambientSnapshot_ = null;
    [SerializeField] private float musicPlaySec_ = 20f;
    [SerializeField] private float musicStartSec_ = 10f;
    [SerializeField] private int maxLoop_ = -1;

    private ObjectPool<GameObject> pool_;
    private Dictionary<string, SoundData> soundsMap_;
    private Dictionary<SceneName, SceneSoundData> sceneSoundsMap_;
    private Dictionary<string, Sound> soundObjs_;
    private List<string> soundQueque_;
    private WaitForSeconds musicPlayWait_;
    private Coroutine sceneSoundCoroutine;
    private int count_ = 0;

    protected override void Awake() {
        base.Awake();
        pool_ = new ObjectPool<GameObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, true, capacity_, maxSize_);
        soundsMap_ = new Dictionary<string, SoundData>();
        sceneSoundsMap_ = new Dictionary<SceneName, SceneSoundData>();
        soundObjs_ = new Dictionary<string, Sound>();
        soundQueque_ = new List<string>();
        musicPlayWait_ = new WaitForSeconds(musicPlaySec_);
        foreach (SoundData sound in sounds_) {
            soundsMap_.Add(sound.name, sound);
        }
        foreach (SceneSoundData sound in sceneSounds_) {
            sceneSoundsMap_.Add(sound.scene, sound);
        }
    }

    private void OnEnable() {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable() {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    public void PlaySound(string name) {
        if (soundsMap_.ContainsKey(name) && !soundObjs_.ContainsKey(name)) {
            GameObject prefab = Resources.Load<GameObject>("Prefab/Audio/Sound");
            Assert.AreNotEqual(prefab, null, "Can not find prefab for Audio/Sound");
            soundObjs_[name] = Instantiate(prefab, transform).GetComponent<Sound>();
            soundObjs_[name].name = "Play_" + name;
        }
        if (soundObjs_.ContainsKey(name) && BaseUtils.IsActive(soundObjs_[name]) && !soundObjs_[name].playing) {
            StartCoroutine(PlaySoundRoutine(soundObjs_[name], soundsMap_[name]));
        }
    }

    private void AfterSceneLoad(SceneName scene) {
        if (!sceneSoundsMap_.ContainsKey(scene)) {
            return;
        }
        if (sceneSoundCoroutine != null) {
            StopCoroutine(sceneSoundCoroutine);
        }
        sceneSoundCoroutine = StartCoroutine(PlaySceneSoundRoutine(sceneSoundsMap_[scene]));
    }

    private IEnumerator PlaySceneSoundRoutine(SceneSoundData scene_sound) {
        SoundData music = soundsMap_[scene_sound.music];
        SoundData ambient = soundsMap_[scene_sound.ambient];
        int cnt = 0;
        while (true) {
            PlayAmbient(ambient, scene_sound.ambientTransitionSec);
            yield return new WaitForSeconds(Random.Range(0, musicStartSec_));
            PlayMusic(music, scene_sound.musicTransitionSec);
            yield return musicPlayWait_;
            cnt += 1;
            if (maxLoop_ > 0 && cnt >= maxLoop_) {
                break;
            }
        }
    }

    private void PlayAmbient(SoundData ambient, float transitionSecs) {
        float ambient_volume = ambient.volume * 100f - 80f;
        audioMixer_.SetFloat("AmbientVolume", ambient_volume);
        ambientAudio_.clip = ambient.audioClip;
        ambientAudio_.Play();
        ambientSnapshot_.TransitionTo(transitionSecs);
    }

    private void PlayMusic(SoundData music, float transitionSecs) {
        float music_volume = music.volume * 100f - 80f;
        audioMixer_.SetFloat("MusicVolume", music_volume);
        musicAudio_.clip = music.audioClip;
        musicAudio_.Play();
        musicSnapshot_.TransitionTo(transitionSecs);
    }

    public void TriggerSound(string name) {
        if (soundsMap_.ContainsKey(name)) {
            StartCoroutine(TriggerSoundRoutine(soundsMap_[name]));
        }
    }

    public void ClearSounds() {
        soundQueque_ = new List<string>();
    }

    public void AddSound(string name) {
        soundQueque_.Add(name);
    }

    public void TriggerSounds() {
        foreach (string name in soundQueque_) {
            TriggerSound(name);
        }
    }

    private IEnumerator PlaySoundRoutine(Sound sound, SoundData data) {
        sound.Play(data);
        yield return new WaitForSeconds(data.audioClip.length * data.duration);
        sound.Stop();
    }

    private IEnumerator TriggerSoundRoutine(SoundData data) {
        Sound sound = GetObj(data).GetComponent<Sound>();
        yield return new WaitForSeconds(Random.Range(0.0f, data.start));
        sound.Play(data);
        yield return new WaitForSeconds(data.audioClip.length * data.duration);
        sound.Stop();
        ReleaseObj(sound.gameObject);
    }

    public GameObject GetObj(SoundData data) {
        GameObject obj = pool_.Get();
        obj.name = "Trigger_" + data.name + "_" + count_;
        count_ += 1;
        return obj;
    }

    public void ReleaseObj(GameObject obj) {
        pool_.Release(obj);
    }

    private GameObject CreateFunc() {
        GameObject prefab = Resources.Load<GameObject>("Prefab/Audio/Sound");
        Assert.AreNotEqual(prefab, null, "Can not find prefab for Audio/Sound");
        return Instantiate(prefab, transform);
    }

    private void ActionOnGet(GameObject obj) {
        obj.SetActive(true);
    }

    private void ActionOnRelease(GameObject obj) {
        obj.SetActive(false);
    }

    private void ActionOnDestroy(GameObject obj) {
        Destroy(obj);
    }

}

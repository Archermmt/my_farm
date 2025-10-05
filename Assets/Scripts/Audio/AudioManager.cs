using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Pool;

public class AudioManager : Singleton<AudioManager> {
    [SerializeField] private SoundData[] sounds_;
    [SerializeField] private int capacity_ = 10;
    [SerializeField] private int maxSize_ = 100;
    private ObjectPool<GameObject> pool_;
    private Dictionary<string, SoundData> soundsMap_;
    private Dictionary<string, Sound> soundObjs_;
    private List<string> soundQueque_;
    private int count_ = 0;

    protected override void Awake() {
        base.Awake();
        pool_ = new ObjectPool<GameObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, true, capacity_, maxSize_);
        soundsMap_ = new Dictionary<string, SoundData>();
        soundObjs_ = new Dictionary<string, Sound>();
        soundQueque_ = new List<string>();
        foreach (SoundData sound in sounds_) {
            soundsMap_.Add(sound.name, sound);
        }
    }

    public void PlaySound(string name) {
        if (soundsMap_.ContainsKey(name) && !soundObjs_.ContainsKey(name)) {
            GameObject prefab = Resources.Load<GameObject>("Prefab/Audio/Sound");
            Assert.AreNotEqual(prefab, null, "Can not find prefab for Audio/Sound");
            soundObjs_[name] = Instantiate(prefab, transform).GetComponent<Sound>();
            soundObjs_[name].name = "Play_" + name;
        }
        if (soundObjs_.ContainsKey(name) && !soundObjs_[name].playing) {
            StartCoroutine(PlaySoundRoutine(soundObjs_[name], soundsMap_[name]));
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
            if (soundsMap_.ContainsKey(name)) {
                StartCoroutine(TriggerSoundRoutine(soundsMap_[name]));
            }
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

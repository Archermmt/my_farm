using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EffectTrigger : MonoBehaviour {
    [SerializeField] private GameObject prefab_;
    [SerializeField] EffectType type_;
    [SerializeField] private int capacity_ = 10;
    [SerializeField] private int maxSize_ = 100;
    [SerializeField] private float effectStartSec_ = 0.2f;
    [SerializeField] private float effectEndSec_ = 2f;
    private string sound_ = "";
    private ObjectPool<GameObject> pool_;
    private List<EffectMeta> effectQueque_;
    private WaitForSeconds effectEndWait_;
    private int count_ = 0;

    private void Awake() {
        pool_ = new ObjectPool<GameObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, true, capacity_, maxSize_);
        effectQueque_ = new List<EffectMeta>();
        effectEndWait_ = new WaitForSeconds(effectEndSec_);
        sound_ = prefab_.GetComponent<Effect>().sound;
    }

    public void ClearEffects() {
        effectQueque_ = new List<EffectMeta>();
    }

    public void AddEffect(EffectMeta effect) {
        effectQueque_.Add(effect);
    }

    public void TriggerEffects() {
        int cnt = 0;
        foreach (EffectMeta effect in effectQueque_) {
            StartCoroutine(TriggerRountine(effect));
            cnt += 1;
        }
    }

    private IEnumerator TriggerRountine(EffectMeta effect) {
        GameObject obj = GetObj(effect.position);
        yield return new WaitForSeconds(effect.offset * effectStartSec_ + Random.Range(0.0f, effectStartSec_));
        obj.GetComponent<Effect>().StartEffect(effect);
        yield return effectEndWait_;
        obj.GetComponent<Effect>().EndEffect(effect);
        ReleaseObj(obj);
    }

    public GameObject GetObj(Vector3 position) {
        GameObject obj = pool_.Get();
        obj.transform.position = position;
        obj.name = type_.ToString() + "_" + count_;
        count_ += 1;
        return obj;
    }

    public void ReleaseObj(GameObject obj) {
        pool_.Release(obj);
    }

    private GameObject CreateFunc() {
        return Instantiate(prefab_, transform);
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

    public EffectType type { get { return type_; } }
    public string sound { get { return sound_; } }
}

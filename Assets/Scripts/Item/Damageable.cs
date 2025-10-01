using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DamageData {
    public EffectType effect;
    public int period = 0;
}

public class Damageable : MonoBehaviour {
    [Header("Destroy")]
    [SerializeField] private List<DamageData> DamageDatas_;
    [SerializeField] private float startSeconds_ = 0.2f;
    [SerializeField] private float endSeconds_ = 0.2f;
    private Dictionary<int, DamageData> DamageDatasMap_;
    private WaitForSeconds endWait_;

    private void Awake() {
        endWait_ = new WaitForSeconds(endSeconds_);
        DamageDatasMap_ = new Dictionary<int, DamageData>();
        foreach (DamageData data in DamageDatas_) {
            DamageDatasMap_[data.period] = data;
        }
    }

    public void DamageItem(Item item, int period) {
        if (!DamageDatasMap_.ContainsKey(period)) {
            return;
        }
        StartCoroutine(DamageRountine(item, DamageDatasMap_[period]));
    }

    private IEnumerator DamageRountine(Item item, DamageData data) {
        GameObject obj = EffectManager.Instance.GetTriggerObj(data.effect, item.AlignGrid());
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.0f, startSeconds_));
        obj.SetActive(true);
        yield return endWait_;
        obj.SetActive(false);
        EffectManager.Instance.ReleaseTriggerObj(data.effect, obj);
    }
}

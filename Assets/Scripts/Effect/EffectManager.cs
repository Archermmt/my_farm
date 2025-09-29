using System.Collections.Generic;
using UnityEngine;

public class EffectManager : Singleton<EffectManager> {
    [SerializeField] private EffectTrigger[] triggers_;
    private Dictionary<EffectType, EffectTrigger> triggersMap_;

    protected override void Awake() {
        base.Awake();
        triggersMap_ = new Dictionary<EffectType, EffectTrigger>();
        foreach (EffectTrigger trigger in GetComponentsInChildren<EffectTrigger>()) {
            triggersMap_.Add(trigger.type, trigger);
        }
    }

    public GameObject GetTriggerObj(EffectType type, Vector3 position) {
        return triggersMap_[type].GetObj(position);
    }

    public void ReleaseTriggerObj(EffectType type, GameObject obj) {
        triggersMap_[type].ReleaseObj(obj);
    }
}

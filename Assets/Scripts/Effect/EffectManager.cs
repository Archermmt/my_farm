using System.Collections.Generic;

public class EffectManager : Singleton<EffectManager> {
    private Dictionary<EffectType, EffectTrigger> triggersMap_;

    protected override void Awake() {
        base.Awake();
        triggersMap_ = new Dictionary<EffectType, EffectTrigger>();
        foreach (EffectTrigger trigger in GetComponentsInChildren<EffectTrigger>()) {
            triggersMap_.Add(trigger.type, trigger);
        }
    }

    public void ClearEffects() {
        foreach (EffectTrigger trigger in triggersMap_.Values) {
            trigger.ClearEffects();
        }
    }

    public void AddEffect(EffectMeta effect) {
        EffectTrigger trigger = triggersMap_[effect.type];
        if (trigger.sound.Length > 0) {
            AudioManager.Instance.AddSound(trigger.sound);
        }
        trigger.AddEffect(effect);
    }

    public void TriggerEffects() {
        foreach (EffectTrigger trigger in triggersMap_.Values) {
            trigger.TriggerEffects();
        }
    }
}

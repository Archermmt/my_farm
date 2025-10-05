using UnityEngine;

[SerializeField]
public class EffectMeta {
    public EffectType type;
    public Vector3 position;
    public ItemData item;
    public int amount;
    public int offset = 0;

    public EffectMeta(EffectType type, Vector3 position, ItemData item, int amount, int offset) {
        this.type = type;
        this.position = position;
        this.item = item;
        this.amount = amount;
        this.offset = offset;
    }
}

public class Effect : MonoBehaviour {
    [SerializeField] private string sound_ = "";
    public virtual void StartEffect(EffectMeta data) { }
    public virtual void EndEffect(EffectMeta data) { }

    public string sound { get { return sound_; } }
}

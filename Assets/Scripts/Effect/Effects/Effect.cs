using UnityEngine;

[SerializeField]
public class EffectData {
    public EffectType type;
    public Vector3 position;
    public ItemData item;
    public int amount;
    public int offset = 0;

    public EffectData(EffectType type, Vector3 position, ItemData item, int amount, int offset) {
        this.type = type;
        this.position = position;
        this.item = item;
        this.amount = amount;
        this.offset = offset;
    }
}

public class Effect : MonoBehaviour {
    public virtual void StartEffect(EffectData data) { }

    public virtual void EndEffect(EffectData data) { }
}

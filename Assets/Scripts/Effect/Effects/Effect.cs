using UnityEngine;

[System.Serializable]
public class EffectMeta {
    public EffectType type;
    public Vector3 position;
    public int offset = 0;
}

public class Effect : MonoBehaviour {
    [SerializeField] private string sound_ = "";
    public virtual void Setup(EffectMeta effect) { }
    public virtual void StartEffect(EffectMeta effect) { }
    public virtual void EndEffect(EffectMeta effect) { }

    public string sound { get { return sound_; } }
}

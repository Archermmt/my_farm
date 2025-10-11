using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class DestroyEffectMeta : EffectMeta {
    public Sprite sprite;
    public float rotate = 0f;
    public float scale = 1f;
    public float alpha = 0f;
    public float destroySec = 1f;
}

[RequireComponent(typeof(SpriteRenderer))]
public class DestoryEffect : Effect {
    protected SpriteRenderer render_;
    private Sprite emptySprite_;

    private void Awake() {
        render_ = GetComponent<SpriteRenderer>();
        emptySprite_ = render_.sprite;
    }

    public override void Setup(EffectMeta effect) {
        DestroyEffectMeta meta = (DestroyEffectMeta)effect;
        gameObject.SetActive(true);
        render_.sprite = meta.sprite;
        render_.color = new Color(1f, 1f, 1f, 1f);
        transform.rotation = Quaternion.identity;
    }

    public override void StartEffect(EffectMeta effect) {
        StartCoroutine(DestroyRoutine((DestroyEffectMeta)effect));
    }

    public override void EndEffect(EffectMeta effect) {
        render_.sprite = emptySprite_;
        render_.color = new Color(1f, 1f, 1f, 1f);
        transform.rotation = Quaternion.identity;
        gameObject.SetActive(false);
    }

    protected virtual IEnumerator DestroyRoutine(DestroyEffectMeta meta) {
        float alpha = render_.color.a;
        float distance = meta.alpha - alpha;
        while (Math.Abs(meta.alpha - alpha) > 0.01f) {
            alpha = alpha + distance / meta.destroySec * Time.deltaTime;
            render_.color = new Color(1f, 1f, 1f, alpha);
            if (meta.rotate != 0f) {
                transform.Rotate(0f, 0f, meta.rotate / meta.destroySec * Time.deltaTime);
            }
            yield return null;
        }
        render_.color = new Color(1f, 1f, 1f, meta.alpha);
    }
}

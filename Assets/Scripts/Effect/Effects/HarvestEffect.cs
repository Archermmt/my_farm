using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HarvestEffectMeta : EffectMeta {
    public Sprite sprite;
    public int amount;
}

public class HarvestEffect : Effect {
    [SerializeField] private Image itemImg_;
    [SerializeField] private TextMeshProUGUI amount_;
    private Animator animator_;
    private Sprite emptySprite_;

    private void Awake() {
        animator_ = GetComponent<Animator>();
        emptySprite_ = itemImg_.sprite;
    }

    public override void StartEffect(EffectMeta effect) {
        HarvestEffectMeta meta = (HarvestEffectMeta)effect;
        gameObject.SetActive(true);
        itemImg_.sprite = meta.sprite;
        amount_.text = "X " + meta.amount;
        animator_.SetTrigger("harvest");
    }

    public override void EndEffect(EffectMeta effect) {
        itemImg_.sprite = emptySprite_;
        amount_.text = "";
        gameObject.SetActive(false);
    }
}

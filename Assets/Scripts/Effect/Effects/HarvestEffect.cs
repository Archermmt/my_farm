using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HarvestEffect : Effect {
    [SerializeField] private Image itemImg_;
    [SerializeField] private TextMeshProUGUI amount_;
    private Animator animator_;
    private Sprite emptySprite_;

    private void Awake() {
        animator_ = GetComponent<Animator>();
        emptySprite_ = itemImg_.sprite;
    }

    public override void StartEffect(EffectMeta data) {
        gameObject.SetActive(true);
        itemImg_.sprite = data.item.sprite;
        amount_.text = "X " + data.amount;
        animator_.SetTrigger("harvest");
    }

    public override void EndEffect(EffectMeta data) {
        itemImg_.sprite = emptySprite_;
        amount_.text = "";
        gameObject.SetActive(false);
    }
}

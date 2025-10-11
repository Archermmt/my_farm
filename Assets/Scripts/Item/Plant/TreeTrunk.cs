using System.Collections;
using UnityEngine;

public class TreeTrunk : TreeBase {
    [Header("TreeTrunk")]
    [SerializeField] private Sprite stumpSprite_;
    [SerializeField] private int maturePeriod_;
    private TreeBase stump_;
    private BoxCollider2D collider_;
    private Vector2 matureSize_;
    private Vector2 matureOffset_;

    protected override void Awake() {
        collider_ = GetComponent<BoxCollider2D>();
        triggerable_ = GetComponent<Triggerable>();
        matureSize_ = collider_.size;
        matureOffset_ = collider_.offset;
        base.Awake();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        triggerable_.TriggerItemEnter(collision, this);
        if (stump_ != null) {
            triggerable_.TriggerItemEnter(collision, stump_);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        triggerable_.TriggerItemExit(collision, this);
        if (stump_ != null) {
            triggerable_.TriggerItemExit(collision, stump_);
        }
    }

    public override Vector3 GetEffectPos(EffectType type) {
        if (type == EffectType.TrunkChunk) {
            return AlignGrid();
        }
        return base.GetEffectPos(type);
    }

    protected override void UpdatePeriod() {
        base.UpdatePeriod();
        if (currentPeriod_ >= maturePeriod_) {
            collider_.size = matureSize_;
            collider_.offset = matureOffset_;
            if (stumpSprite_ != null) {
                stump_ = (TreeBase)ItemManager.Instance.CreateItem(stumpSprite_, transform.position, transform, gameObject.name + "_Stump");
                stump_.gameObject.SetActive(true);
                stump_.SetFreeze(true);
                stump_.SetGenerate(false);
                stump_.transform.GetComponent<Renderer>().sortingOrder = 0;
                stump_.Growth(0);
            }
        } else {
            collider_.size = render_.sprite.bounds.size;
            collider_.offset = new Vector2(0, collider_.size.y / 2);
        }
    }

    public override void DestroyItem(FieldGrid grid) {
        DestroyEffectMeta eff_meta = new DestroyEffectMeta();
        eff_meta.type = EffectType.Destroy;
        eff_meta.sprite = GetLifePeriod().sprite;
        eff_meta.position = GetEffectPos(EffectType.Destroy);
        if (currentPeriod_ >= maturePeriod_) {
            AudioManager.Instance.AddSound("TreeFall");
            eff_meta.rotate = direction_ == Direction.Left ? 90 : -90;
            if (stump_ != null) {
                stump_.SetFreeze(false);
                stump_.SetGenerate(true);
                stump_.transform.GetComponent<Renderer>().sortingOrder = render_.sortingOrder;
                stump_.transform.parent = transform.parent;
                grid.AddItem(stump_);
            }
        }
        EffectManager.Instance.AddEffect(eff_meta);
        Destroy(gameObject);
        grid.RemoveItem(this);

    }
}

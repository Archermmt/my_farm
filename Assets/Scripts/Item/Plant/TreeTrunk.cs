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

    protected override void UpdatePeriod() {
        base.UpdatePeriod();
        if (currentPeriod_ >= maturePeriod_) {
            collider_.size = matureSize_;
            collider_.offset = matureOffset_;
            if (stumpSprite_ != null) {
                stump_ = (TreeBase)ItemManager.Instance.CreateItem(stumpSprite_, transform.position, transform.parent, gameObject.name + "_Stump");
                stump_.gameObject.SetActive(true);
                stump_.SetFreeze(true);
                stump_.SetGenerate(true);
                stump_.Growth(0);
            }
        } else {
            collider_.size = render_.sprite.bounds.size;
            collider_.offset = new Vector2(0, collider_.size.y / 2);
        }
    }

    protected override IEnumerator DestroyRoutine(FieldGrid grid) {
        if (currentPeriod_ >= maturePeriod_) {
            AudioManager.Instance.AddSound("TreeFall");
            float angle = direction_ == Direction.Left ? 20 : -20;
            float alpha = render_.color.a;
            while (alpha > 0.01f) {
                alpha = alpha - alpha / destrySec_ * Time.deltaTime;
                transform.Rotate(0f, 0f, angle / destrySec_ * Time.deltaTime);
                render_.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }
            render_.color = new Color(1f, 1f, 1f, 0);
            Destroy(gameObject);
            if (stump_ != null) {
                stump_.SetFreeze(false);
                grid.AddItem(stump_);
            }
        } else {
            yield return base.DestroyRoutine(grid);
        }
    }
}

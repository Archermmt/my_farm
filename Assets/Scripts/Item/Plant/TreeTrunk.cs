using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeTrunk : Plant {
    [Header("TreeTrunk")]
    [SerializeField] private TreeStump stump_;
    [SerializeField] private int maturePeriod_ = -1;
    [SerializeField] private float destroySecs_ = 1.0f;
    private BoxCollider2D collider_;
    private Animator animator_;
    private WaitForSeconds destroyWait_;
    private Vector2 matureSize_;
    private Vector2 matureOffset_;

    protected override void Awake() {
        collider_ = GetComponent<BoxCollider2D>();
        animator_ = GetComponent<Animator>();
        destroyWait_ = new WaitForSeconds(destroySecs_);
        matureSize_ = collider_.size;
        matureOffset_ = collider_.offset;
        base.Awake();
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);
        if (stump_ != null && currentPeriod_ >= maturePeriod_) {
            triggerable_.TriggerItemEnter(collision, stump_);
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        triggerable_.TriggerItemExit(collision, this);
        if (stump_ != null && currentPeriod_ >= maturePeriod_) {
            triggerable_.TriggerItemExit(collision, stump_);
        }
    }

    public override void SetItem(ItemData item_data) {
        base.SetItem(item_data);
        if (maturePeriod_ < 0) {
            maturePeriod_ = totalPeriod_ + maturePeriod_;
        }
        if (stump_ != null) {
            stump_.gameObject.SetActive(false);
        }
    }

    public override void DestroyItem(FieldGrid grid) {
        if (currentPeriod_ >= maturePeriod_) {
            Debug.Log("[TMINFO] should dectroy the tree in direction " + direction_);
            StartCoroutine(DestroyRoutine(grid));
        } else {
            base.DestroyItem(grid);
        }
        if (stump_ != null && currentPeriod_ >= maturePeriod_) {
            grid.AddItem(stump_);
        }
    }

    protected override void UpdatePeriod() {
        base.UpdatePeriod();
        if (currentPeriod_ >= maturePeriod_) {
            collider_.size = matureSize_;
            collider_.offset = matureOffset_;
            if (stump_ != null) {
                stump_.gameObject.SetActive(true);
                stump_.ResetStatus();
                foreach (ItemStatus status in growthPeriods_[currentPeriod_].statusList) {
                    stump_.AddStatus(status);
                }
            }
        } else {
            collider_.size = render_.sprite.bounds.size;
            collider_.offset = new Vector2(0, 0);
            if (stump_ != null) {
                stump_.gameObject.SetActive(false);
            }
        }
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, Tool tool, int hold_level) {
        direction_ = tool.transform.position.x > transform.position.x ? Direction.Left : Direction.Right;
        if (currentPeriod_ >= maturePeriod_) {
            animator_.SetTrigger("wobble");
            animator_.SetInteger("direction", (int)direction_);
        }
        return base.ToolApply(grid, tool, hold_level);
    }

    private IEnumerator DestroyRoutine(FieldGrid grid) {
        animator_.SetTrigger("destroy");
        animator_.SetInteger("direction", (int)direction_);
        yield return destroyWait_;
        base.DestroyItem(grid);
    }
}

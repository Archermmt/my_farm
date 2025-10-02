using UnityEngine;

public class TreeTrunk : Plant {
    [Header("Tree")]
    [SerializeField] private Plant stump_;
    //[SerializeField] private int seedPeriod_ = 0;
    [SerializeField] private int maturePeriod_ = -1;

    public override void SetItem(ItemData item_data) {
        base.SetItem(item_data);
        if (maturePeriod_ < 0) {
            maturePeriod_ = totalPeriod_ + maturePeriod_;
        }
        stump_.gameObject.SetActive(false);
    }


    protected override void UpdatePeriod() {
        base.UpdatePeriod();
        if (currentPeriod_ >= maturePeriod_) {
            stump_.gameObject.SetActive(true);
        }
        /*
        if (seedPeriod_ >= maturePeriod_) {
            RemoveStatus(ItemStatus.Nudgable);
            AddStatus(ItemStatus.Fadable);
            stump_.gameObject.SetActive(true);
        } else if (currentPeriod_ > seedPeriod_) {
            AddStatus(ItemStatus.Nudgable);
        }
        */
    }

    /*
    public override bool ToolUsable(FieldGrid grid, ToolType tool_type, int hold_level) {
        if (currentPeriod_ >= maturePeriod_) {
            return tool_type == ToolType.Axe;
        }
        if (seedPeriod_ >= maturePeriod_) {
            return tool_type == ToolType.Scythe;
        }
        return false;
    }
    */
}

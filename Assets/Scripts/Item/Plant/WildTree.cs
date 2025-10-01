using UnityEngine;

public class WildTree : Plant {
    [Header("Tree")]
    [SerializeField] private int seedPeriod_ = 0;
    [SerializeField] private int maturePeriod_ = -1;

    public override void SetItem(ItemData item_data) {
        if (maturePeriod_ < 0) {
            maturePeriod_ = totalPeriod_ + maturePeriod_;
        }
    }


    protected override void UpdatePeriod() {
        base.UpdatePeriod();
        if (seedPeriod_ >= maturePeriod_) {
            RemoveStatus(ItemStatus.Nudgable);
            AddStatus(ItemStatus.Fadable);
        } else if (currentPeriod_ > seedPeriod_) {
            AddStatus(ItemStatus.Nudgable);
        }
    }

    public override bool ToolUsable(FieldGrid grid, ToolType tool_type, int hold_level) {
        if (currentPeriod_ >= maturePeriod_) {
            return tool_type == ToolType.Axe;
        }
        if (seedPeriod_ >= maturePeriod_) {
            return tool_type == ToolType.Scythe;
        }
        return false;
    }
}

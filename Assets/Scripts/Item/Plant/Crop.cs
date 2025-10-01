using UnityEngine;

public class Crop : Plant {
  [Header("Crop")]
  [SerializeField] private int seedPeriod_ = 0;
  [SerializeField] private int harvestPeriod_ = -1;

  public override void SetItem(ItemData item_data) {
    base.SetItem(item_data);
    if (harvestPeriod_ < 0) {
      harvestPeriod_ = totalPeriod_ + harvestPeriod_;
    }
  }

  protected override void UpdatePeriod() {
    base.UpdatePeriod();
    if (currentPeriod_ > seedPeriod_) {
      AddStatus(ItemStatus.Nudgable);
    }
  }

  public override bool ToolUsable(FieldGrid grid, ToolType tool_type, int hold_level) {
    if (currentPeriod_ <= seedPeriod_) {
      return false;
    }
    if (tool_type == ToolType.Scythe) {
      return currentPeriod_ < harvestPeriod_;
    }
    if (tool_type == ToolType.Basket) {
      return currentPeriod_ >= harvestPeriod_;
    }
    return false;
  }

  public override void UpdateTime(TimeType time_type, TimeData time, int delta, FieldGrid grid) {
    if (time_type == TimeType.Day && grid.HasTag(FieldTag.Watered)) {
      Growth(delta);
    }
  }
}

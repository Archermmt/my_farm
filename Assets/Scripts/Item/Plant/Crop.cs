using UnityEngine;

public class Crop : Plant {
  /*
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
  */

  public override void UpdateTime(TimeType time_type, TimeData time, int delta, FieldGrid grid) {
    if (time_type == TimeType.Day && grid.HasTag(FieldTag.Watered)) {
      Growth(delta);
    }
  }
}

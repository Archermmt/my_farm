using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crop : Plant {
  [Header("Crop.Basic")]
  [SerializeField] private int seedPeriod_ = 0;
  [SerializeField] private int harvestPeriod_ = -1;

  public override void SetItem(ItemData item_data) {
    base.SetItem(item_data);
    if (harvestPeriod_ < 0) {
      harvestPeriod_ = totalPeriod_ + harvestPeriod_;
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

  protected override bool Nudgable(Collider2D collision) {
    return currentPeriod_ > seedPeriod_ && base.Nudgable(collision);
  }
}

using UnityEngine;

public class Crop : Plant {
  [Header("Crop")]
  [SerializeField] private int seedPeriod_ = 0;
  [SerializeField] private int reapPeriod_ = -1;

  public override bool ToolUsable(ToolType tool_type, int hold_level) {
    if (currentPeriod <= seedPeriod_) {
      return false;
    }
    if (tool_type == ToolType.Scythe) {
      return reapPeriod_ == -1 || currentPeriod <= reapPeriod_;
    }
    if (tool_type == ToolType.Basket) {
      return currentPeriod == totalPeriod_ - 1;
    }
    return false;
  }

  public override ItemStatus ToolApply(ToolType tool_type, int hold_level) {
    if (tool_type == ToolType.Scythe) {
      return ItemStatus.Destroyable;
    }
    return base.ToolApply(tool_type, hold_level);
  }

  protected override bool Nudgable(Collider2D collision) {
    return currentPeriod > seedPeriod_ && base.Nudgable(collision);
  }
}

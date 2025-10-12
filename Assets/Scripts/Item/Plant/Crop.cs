using System.Collections.Generic;

public class Crop : Plant {
  private int srcSorting_;

  protected override void Awake() {
    base.Awake();
    srcSorting_ = render_.sortingOrder;
  }

  protected override void UpdatePeriod() {
    base.UpdatePeriod();
    render_.sortingOrder = HasStatus(ItemStatus.Nudgable) ? srcSorting_ : 0;
  }

  public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, Tool tool, int hold_level) {
    Dictionary<ItemData, int> items = base.ToolApply(grid, tool, hold_level);
    if (items.Count > 0) {
      AudioManager.Instance.AddSound("Pluck");
    }
    return items;
  }

  public override void UpdateTime(TimeType time_type, TimeData time, int delta, FieldGrid grid) {
    if (time_type == TimeType.Day && grid.HasTag(FieldTag.Watered)) {
      Growth(delta);
    }
  }
}

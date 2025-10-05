using System.Collections.Generic;

public class Crop : Plant {
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

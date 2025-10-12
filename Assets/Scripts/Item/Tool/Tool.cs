using System;
using System.Collections.Generic;
using UnityEngine;

public class Tool : Item {
  protected List<Vector2Int> scopeRanges_;

  public override int ConsumeEnergy(List<Cursor> cursors) {
    int hold_level = GetHoldLevel();
    int energy = Math.Max(hold_level, 1);
    int count = Math.Max(cursors.Count / EnergyUnit(hold_level), 1);
    return energy * count;
  }

  protected override bool Dropable(FieldGrid grid) {
    return false;
  }

  protected virtual int GetUseCount() {
    return 1;
  }

  protected virtual int EnergyUnit(int hold_level) {
    return 5;
  }

  protected override int holdLevelMax { get { return scopeRanges_.Count; } }

  public virtual ToolType toolType { get { return ToolType.Tool; } }
}

using System.Collections.Generic;
using UnityEngine;

public class Tool : Item {
  protected List<Vector2Int> scopeRanges_;

  protected override bool Dropable(FieldGrid grid) {
    return false;
  }

  protected virtual int GetUseCount() {
    return 1;
  }

  protected override int holdLevelMax { get { return scopeRanges_.Count; } }

  public virtual ToolType toolType { get { return ToolType.Tool; } }
}

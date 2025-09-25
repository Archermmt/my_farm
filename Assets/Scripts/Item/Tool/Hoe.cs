using UnityEngine;
using System.Collections.Generic;

public class Hoe : Tool {
  [SerializeField] private RuleTile dugTile_;

  protected override bool GridUsable(FieldGrid grid) {
    return grid.HasTag(FieldTag.Diggable) && !grid.HasTag(FieldTag.Dug) && grid.items.Count == 0;
  }

  public override int Apply(List<Cursor> cursors, int amount) {
    FieldLayer layer = FieldManager.Instance.GetLayer(FieldTag.Dug);
    foreach (Cursor cursor in cursors) {
      layer.AddGrid(cursor.grid, dugTile_);
    }
    return 0;
  }

  public override AnimationTag animationTag { get { return AnimationTag.Hoe; } }

  protected override ToolType toolType { get { return ToolType.Hoe; } }
}

using UnityEngine;
using System.Collections.Generic;

public class Hoe : Tool {
  [SerializeField] private RuleTile dugTile_;

  protected override void Awake() {
    base.Awake();
    animationTags_ = new List<AnimationTag> { AnimationTag.Wave, AnimationTag.Hoe };
  }

  protected override bool GridUsable(FieldGrid grid) {
    return grid.HasTag(FieldTag.Diggable) && !grid.HasTag(FieldTag.Dug) && grid.items.Count == 0;
  }

  public override Dictionary<ItemData, int> Apply(List<Cursor> cursors, int amount) {
    FieldLayer layer = FieldManager.Instance.GetLayer(FieldTag.Dug);
    foreach (Cursor cursor in cursors) {
      layer.AddGrid(cursor.grid, dugTile_);
    }
    return new Dictionary<ItemData, int>();
  }

  protected override ToolType toolType { get { return ToolType.Hoe; } }
}

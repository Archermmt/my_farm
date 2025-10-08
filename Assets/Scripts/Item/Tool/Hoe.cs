using System.Collections.Generic;

public class Hoe : GridTool {
  protected override void Awake() {
    base.Awake();
    animationTags_ = new List<AnimationTag> { AnimationTag.Wave, AnimationTag.Hoe };
  }

  protected override bool GridUsable(FieldGrid grid) {
    return grid.HasTag(FieldTag.Diggable) && !grid.HasTag(FieldTag.Dug) && grid.items.Count == 0;
  }

  public override ToolType toolType { get { return ToolType.Hoe; } }
  protected override FieldTag fieldTag { get { return FieldTag.Dug; } }
}

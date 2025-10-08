using System.Collections.Generic;

public class WaterCan : GridTool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Lift, AnimationTag.WaterCan };
    }

    protected override bool GridUsable(FieldGrid grid) {
        return grid.HasTag(FieldTag.Dug);
    }

    public override ToolType toolType { get { return ToolType.WaterCan; } }
    protected override FieldTag fieldTag { get { return FieldTag.Watered; } }
}

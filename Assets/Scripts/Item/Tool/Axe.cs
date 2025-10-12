using System;
using System.Collections.Generic;

public class Axe : ItemTool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Wave, AnimationTag.Axe };
    }

    protected override int EnergyUnit(int hold_level) {
        return 2 * Math.Max(hold_level, 1);
    }

    public override ToolType toolType { get { return ToolType.Axe; } }
}

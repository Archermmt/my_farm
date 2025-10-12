using System;
using System.Collections.Generic;

public class Pickaxe : ItemTool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Wave, AnimationTag.Pickaxe };
    }

    protected override int EnergyUnit(int hold_level) {
        return 3 * Math.Max(hold_level, 1);
    }

    public override ToolType toolType { get { return ToolType.Pickaxe; } }
}

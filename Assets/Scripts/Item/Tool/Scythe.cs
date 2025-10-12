using System;
using System.Collections.Generic;
using UnityEngine;

public class Scythe : ItemTool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Swing, AnimationTag.Scythe };
    }

    protected override int EnergyUnit(int hold_level) {
        return 5 * Math.Max(hold_level, 1);
    }

    public override ToolType toolType { get { return ToolType.Scythe; } }
}

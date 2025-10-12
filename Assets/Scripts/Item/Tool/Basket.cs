using System;
using System.Collections.Generic;
using UnityEngine;

public class Basket : ItemTool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Pick };
    }

    protected override int EnergyUnit(int hold_level) {
        return 9 * Math.Max(hold_level, 1);
    }

    protected override int GetUseCount() {
        if (GetHoldLevel() <= 0) { return 1; }
        Vector2Int range = GetScopeRange();
        return range.x * range.y;
    }

    public override ToolType toolType { get { return ToolType.Basket; } }
}

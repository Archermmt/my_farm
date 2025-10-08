using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class Basket : ItemTool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Pick };
    }

    protected override int GetUseCount() {
        if (GetHoldLevel() <= 0) { return 1; }
        Vector2Int range = GetScopeRange();
        return range.x * range.y;
    }

    public override ToolType toolType { get { return ToolType.Basket; } }
}

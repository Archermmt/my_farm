using System.Collections.Generic;
using UnityEngine;

public class Basket : Tool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Pick };
        scopeRanges_[0] = new Vector2Int(3, 1);
    }

    protected override bool GridUsable(FieldGrid grid) {
        return false;
    }

    protected override Vector2Int GetScopeRange() {
        int level = GetHoldLevel();
        if (level <= 0) { return scopeRanges_[0]; }
        if (level == 1) { return scopeRanges_[1]; }
        if (level == 2) { return scopeRanges_[2]; }
        return scopeRanges_[3];
    }

    protected override int holdLevelMax { get { return 3; } }

    public override ToolType toolType { get { return ToolType.Basket; } }
}

using System.Collections.Generic;
using UnityEngine;

public class Scythe : Tool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Swing, AnimationTag.Scythe };
        scopeRanges_ = new List<Vector2Int> {
          new Vector2Int(3, 2),
          new Vector2Int(3, 3),
          new Vector2Int(9,3),
          new Vector2Int(9,9),
        };
    }

    protected override bool GridUsable(FieldGrid grid) {
        return false;
    }

    protected override Vector2Int GetScopeRange() {
        int level = GetHoldLevel();
        if (level < 0) { return scopeRanges_[0]; }
        if (level == 0) { return scopeRanges_[1]; }
        if (level == 1) { return scopeRanges_[2]; }
        return scopeRanges_[3];
    }

    protected override int GetUseCount() {
        int hold_level = GetHoldLevel();
        if (hold_level <= 0) { return 1; }
        return hold_level * 5;
    }

    protected override int holdLevelMax { get { return 3; } }

    public override ToolType toolType { get { return ToolType.Scythe; } }
}

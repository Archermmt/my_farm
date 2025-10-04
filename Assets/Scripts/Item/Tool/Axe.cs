using System.Collections.Generic;
using UnityEngine;

public class Axe : Tool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Wave, AnimationTag.Axe };
        scopeRanges_ = new List<Vector2Int> {
          new Vector2Int(3, 2),
        };
    }

    protected override bool GridUsable(FieldGrid grid) {
        return false;
    }

    protected override Vector2Int GetScopeRange() {
        return scopeRanges_[0];
    }

    protected override int GetUseCount() { return 1; }

    public override ToolType toolType { get { return ToolType.Axe; } }
}

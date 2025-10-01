using System.Collections.Generic;
using UnityEngine;

public class Basket : Tool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Pick };
    }

    protected override bool GridUsable(FieldGrid grid) {
        return false;
    }

    protected override Vector2Int GetScopeRange() {
        int level = GetHoldLevel();
        if (level <= 0) { return new Vector2Int(3, 1); }
        if (level == 1) { return new Vector2Int(3, 3); }
        if (level == 2) { return new Vector2Int(9, 3); }
        return new Vector2Int(9, 9);
    }

    protected override int holdLevelMax { get { return 3; } }

    protected override ToolType toolType { get { return ToolType.Basket; } }
}

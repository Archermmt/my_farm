using UnityEngine;

public class Basket : Tool {
    protected override bool GridUsable(FieldGrid grid) {
        return false;
    }

    protected override Vector2Int GetScopeRange() {
        int level = GetHoldLevel();
        if (level == -1) { return new Vector2Int(3, 3); }
        if (level == 0) { return new Vector2Int(3, 1); }
        if (level == 1) { return new Vector2Int(3, 3); }
        if (level == 2) { return new Vector2Int(9, 3); }
        if (level == 3) { return new Vector2Int(9, 9); }
        return Vector2Int.one;
    }

    protected override int holdLevelMax { get { return 3; } }

    public override AnimationTag animationTag { get { return AnimationTag.Basket; } }

    protected override ToolType toolType { get { return ToolType.Basket; } }
}

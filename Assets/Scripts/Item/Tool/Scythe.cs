using UnityEngine;

public class Scythe : Tool {
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

    public override int GetUseCount() {
        int hold_level = GetHoldLevel();
        if (hold_level <= 0) { return 1; }
        return hold_level * 5;
    }

    protected override int holdLevelMax { get { return 3; } }

    public override AnimationTag animationTag { get { return AnimationTag.Scythe; } }

    protected override ToolType toolType { get { return ToolType.Scythe; } }
}

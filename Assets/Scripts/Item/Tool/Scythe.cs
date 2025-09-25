using System.Collections.Generic;
using UnityEngine;

public class Scythe : Tool {
    protected override bool GridUsable(FieldGrid grid) {
        return false;
    }

    public override int Apply(List<Cursor> cursors, int amount) {
        int level = GetHoldLevel();
        foreach (Cursor cursor in cursors) {
            ItemStatus status = cursor.item.ToolApply(toolType, level);
            if (status == ItemStatus.Destroyable) {
                cursor.grid.RemoveItem(cursor.item);
                Destroy(cursor.item.gameObject);
            }
        }
        return 0;
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

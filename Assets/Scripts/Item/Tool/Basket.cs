using System.Collections.Generic;
using UnityEngine;

public class Basket : Tool {
    protected override bool GridUsable(FieldGrid grid) {
        return false;
    }

    public override Dictionary<string, int> Apply(List<Cursor> cursors, int amount) {
        int level = GetHoldLevel();
        Dictionary<string, int> crops = new Dictionary<string, int>();
        foreach (Cursor cursor in cursors) {
            cursor.item.ToolApply(cursor.grid, toolType, level);
            Crop crop = (Crop)cursor.item;
            if (!crops.ContainsKey(crop.matureName)) {
                crops.Add(crop.matureName, 0);
            }
            crops[crop.matureName] += 1;
        }
        return crops;
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

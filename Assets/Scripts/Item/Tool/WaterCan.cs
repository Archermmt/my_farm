using UnityEngine;
using System.Collections.Generic;

public class WaterCan : Tool {
    [SerializeField] private RuleTile wateredTile_;

    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Lift, AnimationTag.WaterCan };
        scopeRanges_[0] = new Vector2Int(3, 1);
    }

    protected override bool GridUsable(FieldGrid grid) {
        return grid.HasTag(FieldTag.Dug);
    }

    protected override void ApplyToGrid(List<Cursor> cursors, int amount) {
        AudioManager.Instance.PlaySound(toolType.ToString());
        FieldLayer layer = FieldManager.Instance.GetLayer(FieldTag.Watered);
        foreach (Cursor cursor in cursors) {
            layer.AddGrid(cursor.grid, wateredTile_);
        }
    }

    public override ToolType toolType { get { return ToolType.WaterCan; } }
}

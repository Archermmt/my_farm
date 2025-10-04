using UnityEngine;
using System.Collections.Generic;

public class WaterCan : Tool {
    [SerializeField] private RuleTile wateredTile_;

    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Lift, AnimationTag.WaterCan };
    }

    protected override bool GridUsable(FieldGrid grid) {
        return grid.HasTag(FieldTag.Dug);
    }

    public override Dictionary<ItemData, int> Apply(List<Cursor> cursors, int amount) {
        FieldLayer layer = FieldManager.Instance.GetLayer(FieldTag.Watered);
        foreach (Cursor cursor in cursors) {
            layer.AddGrid(cursor.grid, wateredTile_);
        }
        return new Dictionary<ItemData, int>();
    }

    public override ToolType toolType { get { return ToolType.WaterCan; } }
}

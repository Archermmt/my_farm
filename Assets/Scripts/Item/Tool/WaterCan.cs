using UnityEngine;
using System.Collections.Generic;

public class WaterCan : Tool {
    [SerializeField] private RuleTile wateredTile_;

    protected override bool GridUsable(FieldGrid grid) {
        return grid.HasTag(FieldTag.Dug);
    }

    public override int Apply(List<Cursor> cursors, int amount) {
        FieldLayer layer = FieldManager.Instance.GetLayer(FieldTag.Watered);
        foreach (Cursor cursor in cursors) {
            layer.AddGrid(cursor.grid, wateredTile_);
        }
        return 0;
    }

    public override AnimationTag animationTag { get { return AnimationTag.WaterCan; } }
}

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Harvestable))]
public class Obstacle : Item {
    private Harvestable harvestable_;

    public override void SetItem(ItemData item_data) {
        base.SetItem(item_data);
        harvestable_ = GetComponent<Harvestable>();
    }

    protected override bool Pickable(FieldGrid grid) {
        return false;
    }

    protected override bool Dropable(FieldGrid grid) {
        return false;
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, ToolType tool_type, int hold_level) {
        return harvestable_.HarvestItems(grid, this, 0);
    }
}

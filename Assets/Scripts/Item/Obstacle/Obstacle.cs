using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Harvestable))]
public class Obstacle : Item {
    [SerializeField] private int health_;
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

    protected virtual int GetDamage(ToolType tool_type, int hold_level) {
        return hold_level;
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, ToolType tool_type, int hold_level) {
        if (health_ <= 0) {
            return harvestable_.HarvestItems(grid, this, 0);
        }
        health_ -= GetDamage(tool_type, hold_level);
        return new Dictionary<ItemData, int>();
    }
}

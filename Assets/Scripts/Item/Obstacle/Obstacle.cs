using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Harvestable))]
public class Obstacle : Item {
    [Header("Obstacle")]
    [SerializeField] private int health_ = 1;
    private Harvestable harvestable_;

    protected override void Awake() {
        base.Awake();
        harvestable_ = GetComponent<Harvestable>();
    }

    protected override bool Pickable(FieldGrid grid) {
        return false;
    }

    protected override bool Dropable(FieldGrid grid) {
        return false;
    }

    protected virtual int GetDamage(ToolType tool_type, int hold_level) {
        return Math.Max(hold_level + 1, 1);
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, ToolType tool_type, int hold_level) {
        health_ -= GetDamage(tool_type, hold_level);
        return harvestable_.HarvestItem(grid, this, 0, health_);
    }
}

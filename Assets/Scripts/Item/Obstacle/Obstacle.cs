using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Harvestable), typeof(Damageable))]
public class Obstacle : Item {
    [SerializeField] private int health_;
    private Harvestable harvestable_;
    private Damageable damageable_;

    protected override void Awake() {
        base.Awake();
        harvestable_ = GetComponent<Harvestable>();
        damageable_ = GetComponent<Damageable>();
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
        damageable_.DamageItem(this, 0);
        if (health_ <= 0) {
            return harvestable_.HarvestItems(grid, this, 0);
        }
        return new Dictionary<ItemData, int>();
    }
}

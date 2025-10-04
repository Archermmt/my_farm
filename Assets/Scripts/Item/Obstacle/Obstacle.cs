using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Harvestable))]
public class Obstacle : Item {
    [Header("Obstacle")]
    [SerializeField] private List<ToolType> harvestTools;
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

    protected virtual int GetDamage(Tool tool, int hold_level) {
        return Math.Max(hold_level + 1, 1);
    }

    public override bool ToolUsable(FieldGrid grid, Tool tool, int hold_level) {
        return harvestTools.Contains(tool.toolType);
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, Tool tool, int hold_level) {
        health_ -= GetDamage(tool, hold_level);
        return harvestable_.HarvestItem(grid, this, 0, health_);
    }
}

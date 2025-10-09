using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Triggerable))]
public class TreeBase : Harvestable {
    protected Triggerable triggerable_;

    protected override void Awake() {
        triggerable_ = GetComponent<Triggerable>();
        base.Awake();
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, Tool tool, int hold_level) {
        direction_ = tool.transform.position.x > transform.position.x ? Direction.Left : Direction.Right;
        triggerable_.Rotate(this, direction_ == Direction.Right, 0.5f);
        return base.ToolApply(grid, tool, hold_level);
    }
}

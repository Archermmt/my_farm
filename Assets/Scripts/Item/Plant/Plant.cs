using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Harvestable))]
public class Plant : Item {
    [Header("Plant.Growth")]
    [SerializeField] private List<int> growthPeriods_;
    [SerializeField] private List<Sprite> growthSprites_;
    [SerializeField] private int growthDay_ = 0;
    protected int currentPeriod_;
    protected int totalPeriod_;
    protected Harvestable harvestable_;

    public override void SetItem(ItemData item_data) {
        base.SetItem(item_data);
        UpdatePeriod();
        totalPeriod_ = growthPeriods_.Count;
        harvestable_ = GetComponent<Harvestable>();
        AddStatus(ItemStatus.Nudgable);
    }

    protected override bool Pickable(FieldGrid grid) {
        return false;
    }

    protected override bool Dropable(FieldGrid grid) {
        return false;
    }

    public virtual void Growth(int days = 1) {
        growthDay_ += days;
        UpdatePeriod();
    }

    protected virtual void UpdatePeriod() {
        currentPeriod_ = growthPeriods_.Count - 1;
        for (int i = 0; i < growthPeriods_.Count; i++) {
            if (growthPeriods_[i] >= growthDay_) {
                currentPeriod_ = i;
                break;
            }
        }
        ChangeSprite(growthSprites_[currentPeriod_]);
    }

    public override bool ToolUsable(FieldGrid grid, ToolType tool_type, int hold_level) {
        return tool_type == ToolType.Scythe;
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, ToolType tool_type, int hold_level) {
        return harvestable_.HarvestItems(grid, this, currentPeriod_);
    }
}

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Triggerable), typeof(Harvestable), typeof(Damageable))]
public class Plant : Item {
    [Header("Plant.Growth")]
    [SerializeField] private List<int> growthPeriods_;
    [SerializeField] private List<Sprite> growthSprites_;
    [SerializeField] private int growthDay_ = 0;
    protected int currentPeriod_;
    protected int totalPeriod_;
    private Triggerable triggerable_;
    private Harvestable harvestable_;
    private Damageable damageable_;

    protected override void Awake() {
        base.Awake();
        currentPeriod_ = 0;
        totalPeriod_ = growthPeriods_ == null ? 0 : growthPeriods_.Count;
        triggerable_ = GetComponent<Triggerable>();
        harvestable_ = GetComponent<Harvestable>();
        damageable_ = GetComponent<Damageable>();
        AddStatus(ItemStatus.Nudgable);
        UpdatePeriod();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        triggerable_.TriggerItemEnter(collision, this);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        triggerable_.TriggerItemExit(collision, this);
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
        if (totalPeriod_ == 0) {
            return;
        }
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
        damageable_.DamageItem(this, currentPeriod_);
        return harvestable_.HarvestItems(grid, this, currentPeriod_);
    }
}

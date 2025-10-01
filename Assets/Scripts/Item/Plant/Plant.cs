using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Triggerable), typeof(Harvestable), typeof(Damageable))]
public class Plant : Item {
    [Header("Plant")]
    [SerializeField] private int health_ = 1;
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
        triggerable_ = GetComponent<Triggerable>();
        harvestable_ = GetComponent<Harvestable>();
        damageable_ = GetComponent<Damageable>();
    }

    public override void SetItem(ItemData item_data) {
        base.SetItem(item_data);
        currentPeriod_ = 0;
        totalPeriod_ = growthPeriods_ == null ? 0 : growthPeriods_.Count;
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

    protected virtual int GetDamage(ToolType tool_type, int hold_level) {
        return Math.Max(hold_level + 1, 1);
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, ToolType tool_type, int hold_level) {
        health_ -= GetDamage(tool_type, hold_level);
        damageable_.DamageItem(this, currentPeriod_);
        if (health_ <= 0) {
            return harvestable_.HarvestItems(grid, this, currentPeriod_);
        }
        return new Dictionary<ItemData, int>();
    }

    public override void UpdateTime(TimeType time_type, TimeData time, int delta, FieldGrid grid) {
        if (time_type == TimeType.Day) {
            Growth(delta);
        }
    }
}

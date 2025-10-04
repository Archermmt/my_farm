using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GrowthPeriod {
    public Sprite sprite;
    public int day = 0;
    public int health = 1;
    public List<ToolType> harvestTools;
    public List<ItemStatus> statusList;

    public GrowthPeriod(Sprite sprite, int day = 0, int health = 1, List<ToolType> harvestTools = null, List<ItemStatus> statusList = null) {
        this.sprite = sprite;
        this.day = day;
        this.health = health;
        this.harvestTools = harvestTools == null ? new List<ToolType> { ToolType.Scythe } : harvestTools;
        this.statusList = statusList == null ? new List<ItemStatus> { ItemStatus.Nudgable } : statusList;
    }
}

[RequireComponent(typeof(Triggerable), typeof(Harvestable))]
public class Plant : Item {
    [Header("Plant")]
    [SerializeField] protected List<GrowthPeriod> growthPeriods_;
    [SerializeField] private int growthDay_ = 0;
    protected int currentPeriod_;
    protected int totalPeriod_;
    private int health_ = 1;
    protected Triggerable triggerable_;
    private Harvestable harvestable_;

    protected override void Awake() {
        base.Awake();
        triggerable_ = GetComponent<Triggerable>();
        harvestable_ = GetComponent<Harvestable>();
    }

    public override void SetItem(ItemData item_data) {
        base.SetItem(item_data);
        currentPeriod_ = 0;
        if (growthPeriods_ == null || growthPeriods_.Count == 0) {
            growthPeriods_ = new List<GrowthPeriod> { new GrowthPeriod(item_data.sprite) };
        }
        totalPeriod_ = growthPeriods_.Count;
        UpdatePeriod();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        triggerable_.TriggerItemEnter(collision, this);
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
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
        currentPeriod_ = growthPeriods_.Count - 1;
        for (int i = 0; i < growthPeriods_.Count; i++) {
            if (growthPeriods_[i].day >= growthDay_) {
                currentPeriod_ = i;
                break;
            }
        }
        ChangeSprite(growthPeriods_[currentPeriod_].sprite);
        health_ = growthPeriods_[currentPeriod_].health;
        ResetStatus();
        foreach (ItemStatus status in growthPeriods_[currentPeriod_].statusList) {
            AddStatus(status);
        }
    }

    protected virtual int GetDamage(Tool tool, int hold_level) {
        return Math.Max(hold_level + 1, 1);
    }

    public override bool ToolUsable(FieldGrid grid, Tool tool, int hold_level) {
        return growthPeriods_[currentPeriod_].harvestTools.Contains(tool.toolType);
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, Tool tool, int hold_level) {
        health_ -= GetDamage(tool, hold_level);
        return harvestable_.HarvestItem(grid, this, currentPeriod_, health_);
    }

    public override void UpdateTime(TimeType time_type, TimeData time, int delta, FieldGrid grid) {
        if (time_type == TimeType.Day) {
            Growth(delta);
        }
    }
}

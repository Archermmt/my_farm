using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HarvestData {
    public Sprite sprite;
    public ToolType tool;
    public EffectType effect = EffectType.None;
    public int min;
    public int max;
}

[Serializable]
public class LifePeriod {
    public Sprite sprite;
    public int day = 0;
    public int health = 1;
    public List<ItemStatus> statusList;
    public List<HarvestData> harvestDatas;
}

public class Harvestable : Item {
    [Header("Harvest")]
    [SerializeField] private List<LifePeriod> lifePeriods_;
    private List<ToolType> harvestTools_;
    protected int currentPeriod_ = 0;
    protected int totalPeriod_;

    protected override void Awake() {
        totalPeriod_ = lifePeriods_.Count;
        base.Awake();
    }

    public override void SetItem(ItemData item_data) {
        base.SetItem(item_data);
        UpdatePeriod();
    }

    protected override bool Dropable(FieldGrid grid) {
        return false;
    }

    public override void Growth(int days = 1) {
        base.Growth(days);
        UpdatePeriod();
    }

    protected virtual void UpdatePeriod() {
        currentPeriod_ = 0;
        if (lifePeriods_.Count > 1) {
            for (int i = 0; i < lifePeriods_.Count; i++) {
                if (days_ >= lifePeriods_[i].day && (i == lifePeriods_.Count - 1 || days_ < lifePeriods_[i + 1].day)) {
                    currentPeriod_ = i;
                    break;
                }
            }
        }
        LifePeriod period = lifePeriods_[currentPeriod_];
        ChangeSprite(period.sprite);
        health_ = period.health;
        harvestTools_ = new List<ToolType>();
        foreach (HarvestData data in period.harvestDatas) {
            harvestTools_.Add(data.tool);
        }
        ResetStatus();
        foreach (ItemStatus status in period.statusList) {
            AddStatus(status);
        }
    }

    protected LifePeriod GetLifePeriod() {
        return lifePeriods_[currentPeriod_];
    }

    protected virtual int GetDamage(Tool tool, int hold_level) {
        return Math.Max(hold_level + 1, 1);
    }

    public override bool ToolUsable(FieldGrid grid, Tool tool, int hold_level) {
        return harvestTools_.Contains(tool.toolType);
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, Tool tool, int hold_level) {
        health_ -= GetDamage(tool, hold_level);
        LifePeriod period = lifePeriods_[currentPeriod_];
        Vector3 effect_pos = GetEffectPos();
        foreach (HarvestData data in period.harvestDatas) {
            if (data.effect == EffectType.None || data.tool != tool.toolType) {
                continue;
            }
            EffectManager.Instance.AddEffect(new EffectMeta(data.effect, effect_pos, meta, 0, 0));
        }
        if (health_ > 0) {
            return new Dictionary<ItemData, int>();
        }
        Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();
        foreach (HarvestData data in period.harvestDatas) {
            if (data.max == 0 || data.tool != tool.toolType) {
                continue;
            }
            ItemData item_data = ItemManager.Instance.FindItem(data.sprite);
            int amount = UnityEngine.Random.Range(data.min, data.max);
            EffectManager.Instance.AddEffect(new EffectMeta(EffectType.Harvest, transform.position, item_data, amount, items.Count));
            items.Add(item_data, amount);
        }
        grid.RemoveItem(this);
        DestroyItem(grid);
        return items;
    }

    public override void UpdateTime(TimeType time_type, TimeData time, int delta, FieldGrid grid) {
        if (time_type == TimeType.Day) {
            Growth(delta);
        }
    }
}

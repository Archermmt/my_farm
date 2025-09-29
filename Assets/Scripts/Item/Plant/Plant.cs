using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HarvestData {
    public string name;
    public int period;
    public int min;
    public int max;
    private ItemData item_;
    public void SetItem(ItemData item) {
        item_ = item;
    }
    public ItemData item { get { return item_; } }
}

public class Plant : Triggerable {
    [Header("Plant.Growth")]
    [SerializeField] private List<int> growthPeriods_;
    [SerializeField] private List<Sprite> growthSprites_;
    [SerializeField] private int growthDay_ = 0;

    [Header("Plant.Harvest")]
    [SerializeField] private List<HarvestData> harvestDatas_;
    [SerializeField] private float harvestStart_ = 0.5f;
    [SerializeField] private float harvestEnd_ = 1f;

    private List<List<HarvestData>> harvestPeriods_;
    protected int currentPeriod_;
    protected int totalPeriod_;

    public override void SetItem(ItemData item_data) {
        base.SetItem(item_data);
        UpdatePeriod();
        totalPeriod_ = growthPeriods_.Count;
        harvestPeriods_ = new List<List<HarvestData>>();
        for (int i = 0; i < growthSprites_.Count; i++) {
            harvestPeriods_.Add(new List<HarvestData>());
        }
        foreach (HarvestData data in harvestDatas_) {
            data.SetItem(ItemManager.Instance.FindItem(data.name));
            harvestPeriods_[data.period].Add(data);
        }
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

    protected List<HarvestData> GetHarvestDatas() {
        return harvestPeriods_[currentPeriod_];
    }

    protected override bool Pickable(FieldGrid grid) {
        return false;
    }

    protected override bool Dropable(FieldGrid grid) {
        return false;
    }

    public override bool ToolUsable(FieldGrid grid, ToolType tool_type, int hold_level) {
        return tool_type == ToolType.Scythe;
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, ToolType tool_type, int hold_level) {
        Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();
        foreach (HarvestData data in GetHarvestDatas()) {
            items.Add(data.item, UnityEngine.Random.Range(data.min, data.max));
        }
        renderer_.sprite = null;
        grid.RemoveItem(this);
        int cnt = 0;
        foreach (KeyValuePair<ItemData, int> pair in items) {
            StartCoroutine(Harvest(pair.Key, pair.Value, harvestStart_ * cnt, cnt == items.Count - 1));
            cnt += 1;
        }
        return items;
    }

    private IEnumerator Harvest(ItemData item, int amount, float start = 0, bool destroy = false) {
        yield return new WaitForSeconds(start + UnityEngine.Random.Range(0.0f, harvestStart_));
        GameObject obj = EffectManager.Instance.GetTriggerObj(EffectType.Harvest, transform.position);
        obj.GetComponent<Harvest>().Trigger(transform, item, amount);
        yield return new WaitForSeconds(harvestEnd_);
        EffectManager.Instance.ReleaseTriggerObj(EffectType.Harvest, obj);
        if (destroy) {
            Destroy(gameObject);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HarvestData {
    public Sprite sprite;
    public int period;
    public int min;
    public int max;
    public EffectType effect = EffectType.None;
}

public class Harvestable : MonoBehaviour {
    [Header("Harvest")]
    [SerializeField] private List<HarvestData> harvestDatas_;
    private Dictionary<int, List<HarvestData>> harvestDatasMap_;

    private void Awake() {
        harvestDatasMap_ = new Dictionary<int, List<HarvestData>>();
        foreach (HarvestData data in harvestDatas_) {
            if (!harvestDatasMap_.ContainsKey(data.period)) {
                harvestDatasMap_[data.period] = new List<HarvestData>();
            }
            harvestDatasMap_[data.period].Add(data);
        }
    }

    public Dictionary<ItemData, int> HarvestItem(FieldGrid grid, Item item, int period, int health) {
        if (!harvestDatasMap_.ContainsKey(period)) {
            return new Dictionary<ItemData, int>();
        }
        Vector3 pos = item.GetEffectPos();
        foreach (HarvestData data in harvestDatasMap_[period]) {
            if (data.effect == EffectType.None) {
                continue;
            }
            EffectManager.Instance.AddEffect(new EffectMeta(data.effect, pos, item.meta, 0, 0));
        }
        if (health > 0) {
            return new Dictionary<ItemData, int>();
        }
        Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();
        foreach (HarvestData data in harvestDatasMap_[period]) {
            if (data.max == 0) {
                continue;
            }
            ItemData item_data = ItemManager.Instance.FindItem(data.sprite);
            int amount = UnityEngine.Random.Range(data.min, data.max);
            EffectManager.Instance.AddEffect(new EffectMeta(EffectType.Harvest, pos, item_data, amount, items.Count));
            items.Add(item_data, amount);
        }
        grid.RemoveItem(item);
        item.DestroyItem(grid);
        return items;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HarvestData {
    public Sprite sprite;
    public int period;
    public int min;
    public int max;
}

public class Harvestable : MonoBehaviour {
    [Header("Harvest")]
    [SerializeField] private List<HarvestData> harvestDatas_;

    public Dictionary<ItemData, int> HarvestItems(FieldGrid grid, Item item, int period) {
        Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();
        foreach (HarvestData data in harvestDatas_) {
            if (data.period != period) {
                continue;
            }
            items.Add(ItemManager.Instance.FindItem(data.sprite), UnityEngine.Random.Range(data.min, data.max));
        }
        item.ChangeSprite(null);
        int cnt = 0;
        foreach (KeyValuePair<ItemData, int> pair in items) {
            StartCoroutine(HarvestRountine(item, pair.Key, pair.Value, cnt, cnt == items.Count - 1));
            cnt += 1;
        }
        grid.RemoveItem(item);
        return items;
    }

    private IEnumerator HarvestRountine(Item item, ItemData harvest, int amount, int offset = 0, bool destroy = false) {
        GameObject obj = EffectManager.Instance.GetTriggerObj(EffectType.Harvest, transform.position);
        yield return obj.GetComponent<Harvest>().Trigger(item.AlignGrid(), harvest, amount, offset);
        EffectManager.Instance.ReleaseTriggerObj(EffectType.Harvest, obj);
        if (destroy) {
            Destroy(gameObject);
        }
    }
}

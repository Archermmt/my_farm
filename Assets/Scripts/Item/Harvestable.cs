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
    public List<EffectType> effects;
}

public class Harvestable : MonoBehaviour {
    [Header("Harvest")]
    [SerializeField] private List<HarvestData> harvestDatas_;
    [SerializeField] private float harvestStartSec_ = 0.3f;
    [SerializeField] private float harvetEndSec_ = 1f;
    [SerializeField] private float effectStartSec_ = 0.2f;
    [SerializeField] private float effectEndSec_ = 0.2f;
    private Dictionary<int, List<HarvestData>> harvestDatasMap_;
    private WaitForSeconds harvestEndWait_;
    private WaitForSeconds effectEndWait_;

    private void Awake() {
        harvestEndWait_ = new WaitForSeconds(harvetEndSec_);
        effectEndWait_ = new WaitForSeconds(effectEndSec_);
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
        foreach (HarvestData data in harvestDatasMap_[period]) {
            foreach (EffectType effect in data.effects) {
                StartCoroutine(EffectRountine(item, effect));
            }
        }
        if (health > 0) {
            return new Dictionary<ItemData, int>();
        }
        Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();
        foreach (HarvestData data in harvestDatasMap_[period]) {
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

    private IEnumerator EffectRountine(Item item, EffectType effect) {
        GameObject obj = EffectManager.Instance.GetTriggerObj(effect, item.AlignGrid());
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.0f, effectStartSec_));
        obj.SetActive(true);
        yield return effectEndWait_;
        obj.SetActive(false);
        EffectManager.Instance.ReleaseTriggerObj(effect, obj);
    }

    private IEnumerator HarvestRountine(Item item, ItemData harvest, int amount, int offset = 0, bool destroy = false) {
        GameObject obj = EffectManager.Instance.GetTriggerObj(EffectType.Harvest, transform.position);
        yield return new WaitForSeconds(offset * harvestStartSec_ + UnityEngine.Random.Range(0.0f, harvestStartSec_));
        obj.GetComponent<Harvest>().Trigger(item.AlignGrid(), harvest, amount, offset);
        yield return harvestEndWait_;
        EffectManager.Instance.ReleaseTriggerObj(EffectType.Harvest, obj);
        if (destroy) {
            Destroy(gameObject);
        }
    }
}

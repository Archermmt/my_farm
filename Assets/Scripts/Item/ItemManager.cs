using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Pool;

public class ItemManager : Singleton<ItemManager> {
    [SerializeField] private ItemData[] items_;
    private Dictionary<string, ItemData> itemsMap_;
    private Dictionary<string, Transform> holdersMap_;

    protected override void Awake() {
        base.Awake();
        itemsMap_ = new Dictionary<string, ItemData>();
        foreach (ItemData item in items_) {
            itemsMap_.Add(item.name, item);
        }
        holdersMap_ = ParseHolders();
    }

    public ItemData FindItem(string name) {
        return itemsMap_[name];
    }

    public ItemData RandomItem(List<ItemType> types = null) {
        System.Random rand = new System.Random();
        List<ItemType> v_types = types == null ? new List<ItemType> { ItemType.Seed, ItemType.Food, ItemType.Object } : types;
        List<ItemData> allowed_items = new List<ItemData>();
        foreach (ItemData item in itemsMap_.Values) {
            if (v_types.Contains(item.type)) {
                allowed_items.Add(item);
            }
        }
        return allowed_items[rand.Next(allowed_items.Count)];
    }

    public Item CreateItem(ItemData item_data, Vector3 world_pos, Transform holder = null) {
        GameObject prefab = Resources.Load<GameObject>("Prefab/Item/" + item_data.type.ToString() + "/" + item_data.name);
        if (!prefab) {
            prefab = Resources.Load<GameObject>("Prefab/Item/" + item_data.type.ToString() + "/" + item_data.type.ToString());
        }
        if (!prefab) {
            prefab = Resources.Load<GameObject>("Prefab/Item/Item");
        }
        Assert.AreNotEqual(prefab, null, "Can not find prefab for " + item_data.name + "(" + item_data.type.ToString() + ")");
        Transform item_holder = holder;
        if (item_holder == null) {
            if (holdersMap_.ContainsKey(item_data.name)) {
                item_holder = holdersMap_[item_data.name];
            } else if (holdersMap_.ContainsKey(item_data.type.ToString())) {
                item_holder = holdersMap_[item_data.type.ToString()];
            } else {
                item_holder = holdersMap_["Item"];
            }
        }
        GameObject item_obj = Instantiate(prefab, new Vector3(world_pos.x, world_pos.y - Settings.gridCellSize / 2f, world_pos.z), Quaternion.identity, item_holder);
        item_obj.GetComponent<Item>().SetItem(item_data);
        int cnt = 1;
        while (item_holder.Find("Gen_" + item_data.name + "_" + cnt) != null) {
            cnt++;
        }
        item_obj.name = "Gen_" + item_data.name + "_" + cnt;
        return item_obj.GetComponent<Item>();
    }

    public Item CreateItem(string item_name, Vector3 world_pos, Transform holder = null) {
        return CreateItem(FindItem(item_name), world_pos, holder);
    }

    private Dictionary<string, Transform> ParseHolders() {
        Transform parent = GameObject.FindGameObjectWithTag("Items").transform;
        Dictionary<string, Transform> item_holders = new Dictionary<string, Transform>();
        foreach (Transform child in parent) {
            item_holders[child.name] = child;
        }
        return item_holders;
    }
}

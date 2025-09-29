using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ItemManager : Singleton<ItemManager> {
    [SerializeField] private ItemData[] items_;
    private Dictionary<string, ItemData> itemsMap_;
    private Dictionary<SceneName, Dictionary<string, Transform>> itemHolders_;
    private SceneName currentScene_ = SceneName.StartScene;

    protected override void Awake() {
        base.Awake();
        itemsMap_ = new Dictionary<string, ItemData>();
        foreach (ItemData item in items_) {
            itemsMap_.Add(item.name, item);
        }
        itemHolders_ = new Dictionary<SceneName, Dictionary<string, Transform>>();
    }

    private void OnEnable() {
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnload;
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable() {
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnload;
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
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
            Dictionary<string, Transform> holders = itemHolders_[currentScene_];
            if (holders.ContainsKey(item_data.name)) {
                item_holder = holders[item_data.name];
            } else if (holders.ContainsKey(item_data.type.ToString())) {
                item_holder = holders[item_data.type.ToString()];
            } else {
                item_holder = holders["Item"];
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

    private void BeforeSceneUnload(SceneName scene_name) {
    }

    private void AfterSceneLoad(SceneName scene_name) {
        currentScene_ = scene_name;
        if (!itemHolders_.ContainsKey(scene_name)) {
            ParseHolders(scene_name);
        }
    }

    private void ParseHolders(SceneName scene_name) {
        Transform parent = GameObject.FindGameObjectWithTag("Items").transform;
        Dictionary<string, Transform> item_holders = new Dictionary<string, Transform>();
        foreach (Transform child in parent) {
            item_holders[child.name] = child;
        }
        itemHolders_[scene_name] = item_holders;
    }
}

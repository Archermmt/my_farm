using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;


[System.Serializable]
public class ItemSave {
    public string name;
    public string item_name;
    public string holder;
    public int days;
    public Vector3Save position;

    public ItemSave(Item item, string holder) {
        position = new Vector3Save(item.transform.position);
        name = item.gameObject.name;
        item_name = item.meta.name;
        days = item.days;
        this.holder = holder;
    }
}

public class ItemManager : Singleton<ItemManager> {
    [SerializeField] private ItemData[] items_;
    private Dictionary<string, ItemData> itemsMap_;
    private Dictionary<Sprite, ItemData> itemSpritesMap_;
    private Dictionary<SceneName, Dictionary<string, Transform>> itemHolders_;
    private Dictionary<SceneName, List<Pickable>> pickableItems_;
    private Dictionary<SceneName, List<ItemSave>> itemSaves_;
    private SceneName currentScene_ = SceneName.StartScene;
    private bool freezed_ = false;

    protected override void Awake() {
        base.Awake();
        itemsMap_ = new Dictionary<string, ItemData>();
        itemSpritesMap_ = new Dictionary<Sprite, ItemData>();
        foreach (ItemData item in items_) {
            itemsMap_.Add(item.name, item);
            if (item.sprite != null) {
                itemSpritesMap_.Add(item.sprite, item);
            }
        }
        itemHolders_ = new Dictionary<SceneName, Dictionary<string, Transform>>();
        pickableItems_ = new Dictionary<SceneName, List<Pickable>>();
        itemSaves_ = new Dictionary<SceneName, List<ItemSave>>();
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

    public ItemData FindItem(Sprite sprite) {
        return itemSpritesMap_[sprite];
    }

    public void AddPickable(Pickable item, bool freeze = false) {
        if (freeze) {
            item.SetFreeze(true);
        }
        if (!pickableItems_[currentScene_].Contains(item)) {
            pickableItems_[currentScene_].Add(item);
        }
    }

    public void RemovePickable(Pickable item) {
        if (pickableItems_[currentScene_].Contains(item)) {
            pickableItems_[currentScene_].Remove(item);
        }
        Destroy(item.gameObject);
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

    public Item CreateItem(ItemData item_data, Vector3 world_pos, Transform holder = null, string name = "") {
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
        GameObject item_obj = Instantiate(prefab, new Vector3(world_pos.x, world_pos.y, world_pos.z), Quaternion.identity, item_holder);
        Item item = item_obj.GetComponent<Item>();
        Assert.AreNotEqual(item, null, "Can not find item for " + item_data.name + "(" + item_data.type.ToString() + ") from " + prefab);
        item.SetGenerate(true);
        if (item.meta == null) {
            item.SetItem(item_data);
        }
        if (name.Length > 0) {
            item_obj.name = name;
        } else {
            int cnt = 1;
            while (item_holder.Find("Gen_" + item_data.name + "_" + cnt) != null) {
                cnt++;
            }
            item_obj.name = "Gen_" + item_data.name + "_" + cnt;
        }
        return item;
    }

    public Item CreateItem(string item_name, Vector3 world_pos, Transform holder = null, string name = "") {
        return CreateItem(FindItem(item_name), world_pos, holder, name);
    }

    public Item CreateItem(Sprite item_sprite, Vector3 world_pos, Transform holder = null, string name = "") {
        return CreateItem(FindItem(item_sprite), world_pos, holder, name);
    }

    private void BeforeSceneUnload(SceneName scene_name) {
        itemSaves_[scene_name] = new List<ItemSave>();
        Transform parent = GameObject.FindGameObjectWithTag("Items").transform;
        foreach (Transform child in parent) {
            foreach (Item item in child.GetComponentsInChildren<Item>()) {
                if (!item.generated) {
                    continue;
                }
                itemSaves_[scene_name].Add(new ItemSave(item, child.name));
            }
        }
    }

    private void AfterSceneLoad(SceneName scene_name) {
        currentScene_ = scene_name;
        ParseHolders(scene_name);
        if (!itemSaves_.ContainsKey(scene_name)) {
            GenerateItems(scene_name);
        } else {
            foreach (ItemSave saved in itemSaves_[scene_name]) {
                Item item = CreateItem(saved.item_name, saved.position.ToVector3(), itemHolders_[scene_name][saved.holder], saved.name);
                item.Growth(saved.days);
            }
        }
    }

    private void ParseHolders(SceneName scene_name) {
        Transform parent = GameObject.FindGameObjectWithTag("Items").transform;
        Dictionary<string, Transform> item_holders = new Dictionary<string, Transform>();
        foreach (Transform child in parent) {
            if (child.name == "Generator") {
                continue;
            }
            item_holders[child.name] = child;
        }
        itemHolders_[scene_name] = item_holders;
        pickableItems_[scene_name] = new List<Pickable>();
    }

    private void GenerateItems(SceneName scene_name) {
        Transform holder = GameObject.FindGameObjectWithTag("Items").transform.Find("Generator");
        if (holder != null && holder.gameObject.activeSelf) {
            foreach (Generator generator in holder.GetComponentsInChildren<Generator>()) {
                generator.Generate();
            }
        }
    }

    public void SetFreeze(bool freeze) {
        freezed_ = freeze;
    }

    public bool freezed { get { return freezed_; } }
}

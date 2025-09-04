using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ItemManager : Singleton<ItemManager>
{
    public ItemData[] items;
    private Dictionary<string, ItemData> itemsMap_;
    private Camera camera_;
    private GameObject textbox_;
    private Dictionary<string, Transform> holdersMap_;

    protected override void Awake()
    {
        base.Awake();
        camera_ = Camera.main;
        itemsMap_ = new Dictionary<string, ItemData>();
        foreach (ItemData item in items)
        {
            itemsMap_.Add(item.name, item);
        }
        holdersMap_ = ParseHolders();
    }

    public ItemData FindItem(string name)
    {
        return itemsMap_[name];
    }

    public ItemData RandomItem()
    {
        System.Random rand = new System.Random();
        List<ItemData> allowed_items = new List<ItemData>();
        foreach (ItemData item in itemsMap_.Values)
        {
            if (item.type == ItemType.Seed || item.type == ItemType.Food || item.type == ItemType.Object)
            {
                allowed_items.Add(item);
            }
        }
        return allowed_items[rand.Next(allowed_items.Count)];
    }

    public Item CreateItemInWorld(ItemData item_data, Vector3 world_pos, Transform holder = null)
    {
        GameObject prefab;
        prefab = Resources.Load<GameObject>("Prefab/Item/" + item_data.type.ToString() + "/" + item_data.name);
        if (!prefab)
        {
            prefab = Resources.Load<GameObject>("Prefab/Item/" + item_data.type.ToString() + "/Common");
        }
        if (!prefab)
        {
            prefab = Resources.Load<GameObject>("Prefab/Item/Common");
        }
        Assert.AreNotEqual(prefab, null, "Can not find prefab for " + item_data.name + "(" + item_data.type.ToString() + ")");
        Transform item_holder = holder;
        if (item_holder == null)
        {
            if (holdersMap_.ContainsKey(item_data.name))
            {
                item_holder = holdersMap_[item_data.name];
            }
            else if (holdersMap_.ContainsKey(item_data.type.ToString()))
            {
                item_holder = holdersMap_[item_data.type.ToString()];
            }
            else
            {
                item_holder = holdersMap_["Common"];
            }
        }
        GameObject item_obj = Instantiate(prefab, new Vector3(world_pos.x, world_pos.y - Settings.gridCellSize / 2f, world_pos.z), Quaternion.identity, item_holder);
        item_obj.GetComponent<Item>().SetItem(item_data);
        int cnt = 1;
        while (item_holder.Find("Gen_" + item_data.name + "_" + cnt) != null)
        {
            cnt++;
        }
        item_obj.name = "Gen_" + item_data.name + "_" + cnt;
        return item_obj.GetComponent<Item>();
    }


    public Item CreateItemAtMouse(ItemData item_data, Transform holder = null)
    {
        return CreateItemInWorld(item_data, CameraUtils.MouseToWorld(camera_), holder);
    }

    public void CommentItem(ItemData item_data, Transform item_transform)
    {
        if (textbox_ == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefab/Item/Textbox");
            textbox_ = Instantiate(prefab, item_transform.position, Quaternion.identity, item_transform);
            textbox_.GetComponent<ItemDescriber>().DescribeItem(item_data);
        }
        Vector3 player_pos = Player.Instance.GetViewportPosition();
        Vector3 item_pos = item_transform.position;
        if (player_pos.y > 0.3f)
        {
            textbox_.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);
            textbox_.transform.position = new Vector3(item_pos.x, item_pos.y + 50f, item_pos.z);
        }
        else if (player_pos.y <= 0.3f)
        {
            textbox_.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            textbox_.transform.position = new Vector3(item_pos.x, item_pos.y - 50f, item_pos.z);
        }
    }

    public void UnCommentItem()
    {
        if (textbox_ != null)
        {
            Destroy(textbox_);
        }
    }

    private Dictionary<string, Transform> ParseHolders()
    {
        Transform parent = GameObject.FindGameObjectWithTag("Items").transform;
        Dictionary<string, Transform> item_holders = new Dictionary<string, Transform>();
        foreach (Transform child in parent)
        {
            item_holders[child.name] = child;
        }
        return item_holders;
    }
}

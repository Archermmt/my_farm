using UnityEngine;

[System.Serializable]
public class ItemData
{
    public ItemType type;
    public string name;
    public string description;
    public Sprite sprite;
    public bool pickable = true;
    public bool dropable = true;
    public int useRadius = 1;
    public int price = 0;
    public int value = 0;
    // Food
    public int energy = 0;

    public override string ToString()
    {
        string str = name + "(" + type.ToString() + "),";
        str += "R " + useRadius.ToString() + ",";
        str += "P " + price.ToString() + "/" + value.ToString();
        return str;
    }
}

public class Item : MonoBehaviour
{
    public string item_name;
    private ItemData itemData_;
    private SpriteRenderer renderer_;
    private BoxCollider2D collider_;

    public void SetItem(ItemData item)
    {
        item_name = item.name;
        itemData_ = item;
        renderer_.sprite = item.sprite;
    }

    private void Awake()
    {
        renderer_ = GetComponent<SpriteRenderer>();
        collider_ = GetComponent<BoxCollider2D>();
        if (item_name.Length > 0)
        {
            SetItem(ItemManager.Instance.FindItem(item_name));
        }
    }

    public override string ToString()
    {
        return transform.name + " : " + itemData_.ToString();
    }

    public ItemData ItemData
    {
        get { return itemData_; }
    }

    public virtual bool Dropable
    {
        get { return itemData_.dropable; }
    }

    public virtual bool Usable
    {
        get { return false; }
    }
}
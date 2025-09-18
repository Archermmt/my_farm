using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public ItemType type;
    public string name;
    public string description;
    public Sprite sprite;
    public int price = 0;
    public int value = 0;

    public override string ToString()
    {
        string str = name + "(" + type.ToString() + "),";
        str += "P " + price.ToString() + "/" + value.ToString();
        return str;
    }

    public string Unique()
    {
        return type.ToString() + "::" + name;
    }
}

public class Item : MonoBehaviour
{
    [SerializeField] private string item_name_;
    protected Direction direction_;
    private ItemData meta_;
    private SpriteRenderer renderer_;
    private BoxCollider2D collider_;
    private HashSet<ItemStatus> statusSet_;

    public void SetItem(ItemData item_data)
    {
        item_name_ = item_data.name;
        meta_ = item_data;
        renderer_.sprite = item_data.sprite;
    }

    private void Awake()
    {
        renderer_ = GetComponent<SpriteRenderer>();
        collider_ = GetComponent<BoxCollider2D>();
        statusSet_ = new HashSet<ItemStatus>();
        direction_ = Direction.Around;
        if (item_name_.Length > 0)
        {
            SetItem(ItemManager.Instance.FindItem(item_name_));
        }
    }

    public virtual (Vector3, Vector3) GetScope(FieldGrid center, Vector3 grid_min, Vector3 grid_max)
    {
        Vector3 c_pos = center.position;
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        Vector2Int range = GetScopeRange();
        int near = (range.x - 1) / 2;
        int far = direction_ == Direction.Around ? (range.y - 1) / 2 : range.y;
        if (direction_ == Direction.Around)
        {
            min = new Vector3(Mathf.Max(c_pos.x - near, grid_min.x), Mathf.Max(c_pos.y - far, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + near, grid_max.x), Mathf.Min(c_pos.y + far, grid_max.y), c_pos.z);
        }
        else if (direction_ == Direction.Up)
        {
            min = new Vector3(Mathf.Max(c_pos.x - near, grid_min.x), Mathf.Max(c_pos.y + 1, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + near, grid_max.x), Mathf.Min(c_pos.y + far, grid_max.y), c_pos.z);
        }
        else if (direction_ == Direction.Down)
        {
            min = new Vector3(Mathf.Max(c_pos.x - near, grid_min.x), Mathf.Max(c_pos.y - far, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + near, grid_max.x), Mathf.Min(c_pos.y - 1, grid_max.y), c_pos.z);
        }
        else if (direction_ == Direction.Left)
        {
            min = new Vector3(Mathf.Max(c_pos.x - far, grid_min.x), Mathf.Max(c_pos.y - near, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x - 1, grid_max.x), Mathf.Min(c_pos.y + near, grid_max.y), c_pos.z);
        }
        else if (direction_ == Direction.Right)
        {
            min = new Vector3(Mathf.Max(c_pos.x + 1, grid_min.x), Mathf.Max(c_pos.y - near, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + far, grid_max.x), Mathf.Min(c_pos.y + near, grid_max.y), c_pos.z);
        }
        return (min, max);
    }

    public virtual List<Vector3> EffectField(List<FieldGrid> grids, FieldGrid start, Vector3 pos, Vector3 min, Vector3 max)
    {
        ResetStatus();
        bool dropable = Dropable(start) && grids.Contains(start);
        AddStatus(dropable ? ItemStatus.Dropable : ItemStatus.ItemUnusable);
        return new List<Vector3> { pos };
    }

    public virtual int Apply(List<Cursor> cursors)
    {
        return 0;
    }

    protected void ResetStatus()
    {
        statusSet_ = new HashSet<ItemStatus>();
    }

    public bool HasStatus(ItemStatus status)
    {
        return statusSet_ != null && statusSet_.Contains(status);
    }

    protected void AddStatus(ItemStatus status)
    {
        statusSet_.Add(status);
    }

    protected void RemoveStatus(ItemStatus status)
    {
        if (statusSet_.Contains(status))
        {
            statusSet_.Remove(status);
        }
    }

    protected virtual Vector2Int GetScopeRange()
    {
        return new Vector2Int(5, 5);
    }

    protected virtual bool Dropable(FieldGrid grid)
    {
        return grid.HasTag(FieldTag.Dropable);
    }

    public override string ToString()
    {
        string str = transform.name + " : " + meta_.ToString();
        if (statusSet_.Count > 0)
        {
            str += "<" + statusSet_.Count.ToString() + " Status>:";
            foreach (ItemStatus status in statusSet_)
            {
                str += status.ToString() + ",";
            }
        }
        return str;
    }

    public virtual AnimationTag animationTag { get { return AnimationTag.Carry; } }

    public ItemData meta { get { return meta_; } }

    public virtual bool pickable { get { return true; } }
}
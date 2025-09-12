using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public ItemType type;
    public string name;
    public string description;
    public Sprite sprite;
    public int dropRadius = 2;
    public int useRadius = -1;
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
    private ItemData meta_;
    private SpriteRenderer renderer_;
    private BoxCollider2D collider_;
    private List<ItemStatus> statusList_;

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
        if (item_name_.Length > 0)
        {
            SetItem(ItemManager.Instance.FindItem(item_name_));
        }
    }

    public virtual (Vector3, Vector3) GetScope(FieldGrid center, Vector3 pos, Vector3 grid_min, Vector3 grid_max)
    {
        FieldGrid start = FieldManager.Instance.GetGrid(pos);
        if (start == null)
        {
            return (Vector3.zero, Vector3.zero);
        }
        if (dropRadius == 0 || (useRadius > 0 && GridUsable(start)))
        {
            return GetScopeByRadius(center, pos, grid_min, grid_max, useRadius);
        }
        return GetScopeByRadius(center, pos, grid_min, grid_max, dropRadius);
    }

    protected (Vector3, Vector3) GetScopeByRadius(FieldGrid center, Vector3 pos, Vector3 grid_min, Vector3 grid_max, float radius, Direction direct = Direction.Around)
    {
        Vector3 c_pos = center.position;
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        if (direct == Direction.Around)
        {
            min = new Vector3(Mathf.Max(c_pos.x - radius, grid_min.x), Mathf.Max(c_pos.y - radius, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + radius, grid_max.x), Mathf.Min(c_pos.y + radius, grid_max.y), c_pos.z);
        }
        else if (direct == Direction.Up)
        {
            min = new Vector3(Mathf.Max(c_pos.x - radius, grid_min.x), Mathf.Max(c_pos.y + 1, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + radius, grid_max.x), Mathf.Min(c_pos.y + radius, grid_max.y), c_pos.z);
        }
        else if (direct == Direction.Down)
        {
            min = new Vector3(Mathf.Max(c_pos.x - radius, grid_min.x), Mathf.Max(c_pos.y - radius, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + radius, grid_max.x), Mathf.Min(c_pos.y - 1, grid_max.y), c_pos.z);
        }
        else if (direct == Direction.Left)
        {
            min = new Vector3(Mathf.Max(c_pos.x - radius, grid_min.x), Mathf.Max(c_pos.y - radius, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x - 1, grid_max.x), Mathf.Min(c_pos.y + radius, grid_max.y), c_pos.z);
        }
        else if (direct == Direction.Right)
        {
            min = new Vector3(Mathf.Max(c_pos.x + 1, grid_min.x), Mathf.Max(c_pos.y - radius, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + radius, grid_max.x), Mathf.Min(c_pos.y + radius, grid_max.y), c_pos.z);
        }
        return (min, max);
    }

    public virtual List<Vector3> EffectField(List<FieldGrid> grids, Vector3 pos, Vector3 min, Vector3 max)
    {
        if (!HasStatus(ItemStatus.Holding))
        {
            statusList_ = new List<ItemStatus>();
        }
        List<Vector3> positions = new List<Vector3>();
        FieldGrid start = FieldManager.Instance.GetGrid(pos);
        if (useRadius > 0 && GridUsable(start))
        {
            positions.Add(start.GetCenter());
            AddStatus(grids.Contains(start) ? ItemStatus.GridUsable : ItemStatus.GridUnusable);

        }
        else if (dropRadius > 0 && GridDropable(start))
        {
            positions.Add(pos);
            float s_size = Settings.gridCellSize;
            bool dropable = pos.x > min.x && pos.x < max.x + s_size && pos.y > min.y && pos.y < max.y + s_size;
            AddStatus(dropable ? ItemStatus.Dropable : ItemStatus.PosUnusable);
        }
        else
        {
            positions.Add(pos);
            AddStatus(ItemStatus.PosUnusable);
        }
        return positions;
    }

    public virtual int Apply(List<Cursor> cursors)
    {
        return 0;
    }

    public void AddStatus(ItemStatus status)
    {
        statusList_.Add(status);
    }

    public void RemoveStatus(ItemStatus status)
    {
        if (statusList_.Contains(status))
        {
            statusList_.Remove(status);
        }
    }

    public bool HasStatus(ItemStatus status)
    {
        return statusList_ != null && statusList_.Contains(status);
    }

    protected virtual bool GridDropable(FieldGrid grid)
    {
        return grid.HasTag(FieldTag.Dropable);
    }

    protected virtual bool GridUsable(FieldGrid grid)
    {
        return true;
    }

    protected virtual bool ItemUsable(Item other)
    {
        return true;
    }

    public override string ToString()
    {
        string str = transform.name + " : " + meta_.ToString();
        if (statusList_.Count > 0)
        {
            str += "<" + statusList_.Count.ToString() + " Status>:";
            foreach (ItemStatus status in statusList_)
            {
                str += status.ToString() + ",";
            }
        }
        return str;
    }

    public ItemData meta { get { return meta_; } }

    public List<ItemStatus> statusList { get { return statusList_; } }

    public virtual int pickRadius { get { return 0; } }

    public virtual int dropRadius { get { return meta_.dropRadius; } }

    public virtual int useRadius { get { return meta_.useRadius; } }

    public virtual int effectNum { get { return 1; } }
}
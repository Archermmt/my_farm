using System.Collections.Generic;
using System.Text;
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
}

public class Item : MonoBehaviour
{
    [SerializeField] private string item_name_;
    private ItemData meta_;
    private SpriteRenderer renderer_;
    private BoxCollider2D collider_;

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

    public virtual (Vector3, Vector3) GetScope(EnvGrid center, EnvGrid start, Vector3 pos, Vector3 grid_min, Vector3 grid_max)
    {
        if ((UseRadius > 0 && GridUsable(start)) || DropRadius == 0)
        {
            return GetScopeByRadius(center, pos, grid_min, grid_max, UseRadius);
        }
        return GetScopeByRadius(center, pos, grid_min, grid_max, DropRadius);
    }

    protected (Vector3, Vector3) GetScopeByRadius(EnvGrid center, Vector3 pos, Vector3 grid_min, Vector3 grid_max, float radius, bool around = true)
    {
        Vector3 center_pos = center.Position;
        Vector3 min, max;
        float diff_width = pos.x - center_pos.x;
        float diff_height = pos.y - center_pos.y;
        if (around)
        {
            min = new Vector3(Mathf.Max(center_pos.x - radius, grid_min.x), Mathf.Max(center_pos.y - radius, grid_min.y), center_pos.z);
            max = new Vector3(Mathf.Min(center_pos.x + radius, grid_max.x), Mathf.Min(center_pos.y + radius, grid_max.y), center_pos.z);
        }
        else if (Mathf.Abs(diff_height) >= Mathf.Abs(diff_width))
        {
            if (diff_height < 0)
            {
                min = new Vector3(Mathf.Max(center_pos.x - radius, grid_min.x), Mathf.Max(center_pos.y - radius, grid_min.y), center_pos.z);
                max = new Vector3(Mathf.Min(center_pos.x + radius, grid_max.x), Mathf.Min(center_pos.y - 1, grid_max.y), center_pos.z);
            }
            else
            {
                min = new Vector3(Mathf.Max(center_pos.x - radius, grid_min.x), Mathf.Max(center_pos.y + 1, grid_min.y), center_pos.z);
                max = new Vector3(Mathf.Min(center_pos.x + radius, grid_max.x), Mathf.Min(center_pos.y + radius, grid_max.y), center_pos.z);
            }
        }
        else
        {
            if (diff_width < 0)
            {
                min = new Vector3(Mathf.Max(center_pos.x - radius, grid_min.x), Mathf.Max(center_pos.y - radius, grid_min.y), center_pos.z);
                max = new Vector3(Mathf.Min(center_pos.x - 1, grid_max.x), Mathf.Min(center_pos.y + radius, grid_max.y), center_pos.z);
            }
            else
            {
                min = new Vector3(Mathf.Max(center_pos.x + 1, grid_min.x), Mathf.Max(center_pos.y - radius, grid_min.y), center_pos.z);
                max = new Vector3(Mathf.Min(center_pos.x + radius, grid_max.x), Mathf.Min(center_pos.y + radius, grid_max.y), center_pos.z);
            }
        }
        return (min, max);
    }

    public virtual (List<Vector3>, List<ItemStatus>) EffectEnv(List<EnvGrid> grids, EnvGrid start, Vector3 pos, Vector3 min, Vector3 max)
    {
        List<Vector3> positions = new List<Vector3>();
        List<ItemStatus> status_list = new List<ItemStatus>();
        if (UseRadius > 0 && GridUsable(start))
        {
            positions.Add(start.Position);
            status_list.Add(grids.Contains(start) ? ItemStatus.GridUsable : ItemStatus.GridUnusable);
        }
        else if (DropRadius > 0 && GridDropable(start))
        {
            positions.Add(pos);
            bool dropable = pos.x > min.x && pos.x < max.x && pos.y > min.y && pos.y < max.y;
            status_list.Add(dropable ? ItemStatus.Dropable : ItemStatus.PosUnusable);
        }
        else
        {
            positions.Add(pos);
            status_list.Add(ItemStatus.PosUnusable);
        }
        return (positions, status_list);
    }

    protected virtual bool GridDropable(EnvGrid grid)
    {
        return grid.HasTag(AreaTag.Dropable);
    }

    protected virtual bool GridUsable(EnvGrid grid)
    {
        return true;
    }

    public override string ToString()
    {
        return transform.name + " : " + meta_.ToString();
    }

    public ItemData Meta
    {
        get { return meta_; }
    }

    public virtual int PickRadius
    {
        get { return 0; }
    }

    public virtual int DropRadius
    {
        get { return meta_.dropRadius; }
    }

    public virtual int UseRadius
    {
        get { return meta_.useRadius; }
    }

    public virtual int EffectNum
    {
        get { return 1; }
    }
}
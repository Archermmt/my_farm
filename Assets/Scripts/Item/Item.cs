using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemData {
    public ItemType type;
    public string name;
    public string description;
    public Sprite sprite;
    public int price = 0;
    public int value = 0;

    public override string ToString() {
        string str = name + "(" + type.ToString() + "),";
        str += "P " + price.ToString() + "/" + value.ToString();
        return str;
    }

    public string Unique() {
        return type.ToString() + "::" + name;
    }
}

[RequireComponent(typeof(SpriteRenderer))]
public class Item : MonoBehaviour {
    [Header("Basic")]
    [SerializeField] private string item_name_;
    protected Direction direction_;
    private ItemData meta_;
    protected SpriteRenderer renderer_;
    private HashSet<ItemStatus> statusSet_;
    private DateTime holdStart_;

    public virtual void SetItem(ItemData item_data) {
        item_name_ = item_data.name;
        meta_ = item_data;
        renderer_.sprite = item_data.sprite;
    }

    protected virtual void Awake() {
        renderer_ = GetComponent<SpriteRenderer>();
        statusSet_ = new HashSet<ItemStatus>();
        direction_ = Direction.Around;
        if (item_name_.Length > 0) {
            SetItem(ItemManager.Instance.FindItem(item_name_));
        }
    }

    public virtual (Vector3, Vector3) GetScope(FieldGrid center, Vector3 grid_min, Vector3 grid_max) {
        Vector3 c_pos = center.position;
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        Vector2Int range = GetScopeRange();
        int near = (range.x - 1) / 2;
        int far = direction_ == Direction.Around ? (range.y - 1) / 2 : range.y;
        if (direction_ == Direction.Around) {
            min = new Vector3(Mathf.Max(c_pos.x - near, grid_min.x), Mathf.Max(c_pos.y - far, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + near, grid_max.x), Mathf.Min(c_pos.y + far, grid_max.y), c_pos.z);
        } else if (direction_ == Direction.Up) {
            min = new Vector3(Mathf.Max(c_pos.x - near, grid_min.x), Mathf.Max(c_pos.y + 1, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + near, grid_max.x), Mathf.Min(c_pos.y + far, grid_max.y), c_pos.z);
        } else if (direction_ == Direction.Down) {
            min = new Vector3(Mathf.Max(c_pos.x - near, grid_min.x), Mathf.Max(c_pos.y - far, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + near, grid_max.x), Mathf.Min(c_pos.y - 1, grid_max.y), c_pos.z);
        } else if (direction_ == Direction.Left) {
            min = new Vector3(Mathf.Max(c_pos.x - far, grid_min.x), Mathf.Max(c_pos.y - near, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x - 1, grid_max.x), Mathf.Min(c_pos.y + near, grid_max.y), c_pos.z);
        } else if (direction_ == Direction.Right) {
            min = new Vector3(Mathf.Max(c_pos.x + 1, grid_min.x), Mathf.Max(c_pos.y - near, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + far, grid_max.x), Mathf.Min(c_pos.y + near, grid_max.y), c_pos.z);
        }
        return (min, max);
    }

    public virtual List<CursorMeta> GetCursorMetas(List<FieldGrid> grids, FieldGrid start, Vector3 pos) {
        ResetStatus();
        bool dropable = Dropable(start) && grids.Contains(start);
        AddStatus(dropable ? ItemStatus.Dropable : ItemStatus.Unusable);
        return new List<CursorMeta> { new CursorMeta(pos, start, null, dropable ? CursorMode.ValidPos : CursorMode.Invalid) };
    }

    public virtual Dictionary<ItemData, int> Apply(List<Cursor> cursors, int amount) {
        return new Dictionary<ItemData, int>();
    }

    public virtual void Hold(Direction direction) {
        AddStatus(ItemStatus.Holding);
        direction_ = direction;
        holdStart_ = DateTime.Now;
    }

    public virtual void Unhold() {
        RemoveStatus(ItemStatus.Holding);
        direction_ = Direction.Around;
    }

    protected void ChangeSprite(Sprite sprite) {
        renderer_.sprite = sprite;
    }

    protected int GetHoldLevel() {
        if (!HasStatus(ItemStatus.Holding)) { return -1; }
        TimeSpan diff = DateTime.Now - holdStart_;
        return Math.Min(Mathf.FloorToInt((float)diff.TotalSeconds), holdLevelMax);
    }

    protected void ResetStatus() {
        if (!HasStatus(ItemStatus.Holding)) {
            statusSet_ = new HashSet<ItemStatus>();
        }
    }

    public bool HasStatus(ItemStatus status) {
        return statusSet_ != null && statusSet_.Contains(status);
    }

    protected void AddStatus(ItemStatus status) {
        statusSet_.Add(status);
    }

    protected void RemoveStatus(ItemStatus status) {
        if (statusSet_.Contains(status)) {
            statusSet_.Remove(status);
        }
    }

    protected virtual Vector2Int GetScopeRange() {
        return new Vector2Int(5, 5);
    }

    protected virtual bool Pickable(FieldGrid grid) {
        return true;
    }

    protected virtual bool Dropable(FieldGrid grid) {
        return grid.HasTag(FieldTag.Dropable);
    }

    public virtual bool ToolUsable(FieldGrid grid, ToolType tool_type, int hold_level) {
        return false;
    }

    public virtual Dictionary<ItemData, int> ToolApply(FieldGrid grid, ToolType tool_type, int hold_level) {
        return new Dictionary<ItemData, int>();
    }

    public override string ToString() {
        string str = transform.name + " : " + meta_.ToString();
        if (statusSet_ != null && statusSet_.Count > 0) {
            str += "<" + statusSet_.Count.ToString() + " Status>:";
            foreach (ItemStatus status in statusSet_) {
                str += status.ToString() + ",";
            }
        }
        return str;
    }

    public virtual AnimationTag animationTag { get { return AnimationTag.Carry; } }

    public ItemData meta { get { return meta_; } }

    protected virtual int holdLevelMax { get { return 1; } }
}
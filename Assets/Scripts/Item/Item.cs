using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemData {
    public string name;
    public ItemType type;
    public string description;
    public Sprite sprite;
    public int health = 1;
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
    [SerializeField] protected int days_ = 0;
    protected int health_;
    protected Direction direction_;
    protected SpriteRenderer render_;
    protected List<AnimationTag> animationTags_;
    private ItemData meta_;
    private HashSet<ItemStatus> statusSet_;
    private DateTime holdStart_;

    protected virtual void Awake() {
        render_ = GetComponent<SpriteRenderer>();
        statusSet_ = new HashSet<ItemStatus>();
        direction_ = Direction.Around;
        animationTags_ = new List<AnimationTag> { AnimationTag.Carry };
        if (item_name_.Length > 0) {
            SetItem(ItemManager.Instance.FindItem(item_name_));
        }
    }

    public virtual void SetItem(ItemData item_data) {
        item_name_ = item_data.name;
        meta_ = item_data;
        render_.sprite = item_data.sprite;
        health_ = item_data.health;
        days_ = 0;
    }

    public virtual void DestroyItem(FieldGrid grid) {
        Destroy(gameObject);
    }

    public virtual void Growth(int days = 1) {
        days_ += days;
    }

    public Vector3 AlignGrid() {
        Vector3 pos = transform.position;
        return new Vector3(pos.x, pos.y + Settings.gridCellSize / 2, pos.z);
    }

    public virtual Vector3 GetEffectPos() {
        Vector3 pos = transform.position;
        return new Vector3(pos.x, pos.y + render_.sprite.bounds.size.y / 2, pos.z);
    }

    public virtual (Vector3, Vector3) GetScope(FieldGrid center, Vector3 grid_min, Vector3 grid_max, Direction direct) {
        return GetScopeByDirect(center, grid_min, grid_max, direct);
    }

    protected (Vector3, Vector3) GetScopeByDirect(FieldGrid center, Vector3 grid_min, Vector3 grid_max, Direction direct) {
        Vector3 c_pos = center.position;
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        Vector2Int range = GetScopeRange();
        int near = (range.x - 1) / 2;
        int far = direct == Direction.Around ? (range.y - 1) / 2 : range.y;
        if (direct == Direction.Around) {
            min = new Vector3(Mathf.Max(c_pos.x - near, grid_min.x), Mathf.Max(c_pos.y - far, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + near, grid_max.x), Mathf.Min(c_pos.y + far, grid_max.y), c_pos.z);
        } else if (direct == Direction.Up) {
            min = new Vector3(Mathf.Max(c_pos.x - near, grid_min.x), Mathf.Max(c_pos.y + 1, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + near, grid_max.x), Mathf.Min(c_pos.y + far, grid_max.y), c_pos.z);
        } else if (direct == Direction.Down) {
            min = new Vector3(Mathf.Max(c_pos.x - near, grid_min.x), Mathf.Max(c_pos.y - far, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x + near, grid_max.x), Mathf.Min(c_pos.y - 1, grid_max.y), c_pos.z);
        } else if (direct == Direction.Left) {
            min = new Vector3(Mathf.Max(c_pos.x - far, grid_min.x), Mathf.Max(c_pos.y - near, grid_min.y), c_pos.z);
            max = new Vector3(Mathf.Min(c_pos.x - 1, grid_max.x), Mathf.Min(c_pos.y + near, grid_max.y), c_pos.z);
        } else if (direct == Direction.Right) {
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

    public void ChangeSprite(Sprite sprite) {
        render_.sprite = sprite;
    }

    protected int GetHoldLevel() {
        if (!HasStatus(ItemStatus.Holding)) { return -1; }
        TimeSpan diff = DateTime.Now - holdStart_;
        return Math.Min(Mathf.FloorToInt((float)diff.TotalSeconds), holdLevelMax);
    }

    public void ResetStatus() {
        if (!HasStatus(ItemStatus.Holding)) {
            statusSet_ = new HashSet<ItemStatus>();
        }
    }

    public bool HasStatus(ItemStatus status) {
        return statusSet_ != null && statusSet_.Contains(status);
    }

    public void AddStatus(ItemStatus status) {
        statusSet_.Add(status);
    }

    protected void RemoveStatus(ItemStatus status) {
        if (statusSet_.Contains(status)) {
            statusSet_.Remove(status);
        }
    }

    protected virtual Vector2Int GetScopeRange() {
        return new Vector2Int(5, 2);
    }

    protected virtual bool Pickable(FieldGrid grid) {
        return true;
    }

    protected virtual bool Dropable(FieldGrid grid) {
        return grid.HasTag(FieldTag.Dropable);
    }

    public virtual bool ToolUsable(FieldGrid grid, Tool tool, int hold_level) {
        return false;
    }

    public virtual Dictionary<ItemData, int> ToolApply(FieldGrid grid, Tool tool, int hold_level) {
        return new Dictionary<ItemData, int>();
    }

    public virtual void UpdateTime(TimeType time_type, TimeData time, int delta, FieldGrid grid) {
        if (time_type == TimeType.Day) {
            Growth(delta);
        }
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

    public ItemData meta { get { return meta_; } }
    public Direction direction { get { return direction_; } }
    public SpriteRenderer render { get { return render_; } }
    public List<AnimationTag> animationTags { get { return animationTags_; } }
    protected virtual int holdLevelMax { get { return 1; } }
}
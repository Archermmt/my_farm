using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class FieldGrid {
    private Vector3 position_;
    private Vector2Int coord_;
    private HashSet<FieldTag> fieldTags_;
    private List<Item> items_;

    public FieldGrid(Vector3 position, Vector2Int coord) {
        position_ = position;
        coord_ = coord;
        fieldTags_ = new HashSet<FieldTag>();
        items_ = new List<Item>();
    }

    public Vector3 GetCenter() {
        return new Vector3(position_.x + Settings.gridCellSize / 2, position_.y + Settings.gridCellSize / 2, position_.z);
    }

    public Vector3 GetItemPos() {
        return new Vector3(position_.x + Settings.gridCellSize / 2, position_.y, position_.z);
    }

    public void AddTag(FieldTag tag) {
        fieldTags_.Add(tag);
    }

    public bool HasTag(FieldTag tag) {
        return fieldTags_.Contains(tag);
    }

    public void RemoveTag(FieldTag tag) {
        if (fieldTags_.Contains(tag)) {
            fieldTags_.Remove(tag);
        }
    }

    public void AddItem(Item item) {
        if (!items_.Contains(item)) {
            items_.Add(item);
        }
    }

    public void RemoveItem(Item item) {
        if (items_.Contains(item)) {
            items_.Remove(item);
        }
    }

    public bool HasItemType(ItemType type) {
        foreach (Item item in items_) {
            if (item.meta.type == type) {
                return true;
            }
        }
        return false;
    }

    public Vector3 position {
        get { return position_; }
    }

    public Vector2Int coord {
        get { return coord_; }
    }

    public override string ToString() {
        string str = "Coord[" + coord_ + "] @ " + position_ + ":";
        if (fieldTags_ != null && fieldTags_.Count > 0) {
            str += " <" + fieldTags_.Count + " Tags>:";
            foreach (FieldTag tag in fieldTags_) {
                str += tag.ToString() + ",";
            }
        }
        if (items_ != null && items_.Count > 0) {
            str += " <" + items_.Count + " Items>:";
            foreach (Item item in items_) {
                str += item.ToString() + " @ " + item.transform.position + ",";
            }
        }
        return str;
    }

    public HashSet<FieldTag> fieldTags { get { return fieldTags_; } }
    public List<Item> items { get { return items_; } }
}

[RequireComponent(typeof(Tilemap))]
public class FieldLayer : MonoBehaviour {
    [SerializeField] private FieldTag fieldTag_;
    [SerializeField] private RuleTile tile_;
    protected List<FieldGrid> grids_;
    private Tilemap tilemap_;

    private void Awake() {
        tilemap_ = GetComponent<Tilemap>();
        grids_ = new List<FieldGrid>();
    }

    public void SetTag(FieldTag tag) {
        fieldTag_ = tag;
    }

    public void AddGrid(FieldGrid grid) {
        grid.AddTag(fieldTag_);
        if (tile_ != null) {
            tilemap_.SetTile(Vector3Int.FloorToInt(grid.GetCenter()), tile_);
        }
        if (!grids_.Contains(grid)) {
            grids_.Add(grid);
        }
    }

    public void RemoveGrid(FieldGrid grid) {
        grid.RemoveTag(fieldTag_);
        tilemap_.SetTile(Vector3Int.FloorToInt(grid.GetCenter()), null);
        if (grids_.Contains(grid)) {
            grids_.Remove(grid);
        }
    }

    public virtual void UpdateTime(TimeType time_type, TimeData time, int delta) { }

    public FieldTag fieldTag { get { return fieldTag_; } }
    public List<FieldGrid> grids { get { return grids_; } }
}

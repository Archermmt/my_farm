using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class FieldGrid {
    private Vector3 position_;
    private Vector2Int coord_;
    private Dictionary<FieldTag, int> fieldTags_;
    private List<Item> items_;

    public FieldGrid(Vector3 position, Vector2Int coord) {
        position_ = position;
        coord_ = coord;
        fieldTags_ = new Dictionary<FieldTag, int>();
        items_ = new List<Item>();
    }

    public Vector3 GetCenter() {
        return new Vector3(position_.x + Settings.gridCellSize / 2, position_.y + Settings.gridCellSize / 2, position_.z);
    }

    public void AddTag(FieldTag tag, int duration = 0) {
        if (!fieldTags_.ContainsKey(tag)) {
            fieldTags_[tag] = duration;
        }
    }

    public void IncreaseTag(FieldTag tag, int inc = 1) {
        if (fieldTags_.ContainsKey(tag)) {
            fieldTags_[tag] += inc;
        }
    }

    public bool HasTag(FieldTag tag) {
        return fieldTags_.ContainsKey(tag);
    }

    public void RemoveTag(FieldTag tag) {
        if (fieldTags_.ContainsKey(tag)) {
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

    public Vector3 position {
        get { return position_; }
    }

    public Vector2Int coord {
        get { return coord_; }
    }

    public Dictionary<FieldTag, int> fieldTags {
        get { return fieldTags_; }
    }

    public List<Item> items {
        get { return items_; }
    }

    public override string ToString() {
        string str = "Coord[" + coord_ + "] @ " + position_ + ":";
        if (fieldTags_ != null && fieldTags_.Count > 0) {
            str += " <" + fieldTags_.Count + " Tags>:";
            foreach (KeyValuePair<FieldTag, int> pair in fieldTags_) {
                str += pair.Key.ToString() + "|" + pair.Value.ToString() + ",";
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
}

[RequireComponent(typeof(Tilemap))]
public class FieldLayer : MonoBehaviour {
    [SerializeField] private FieldTag fieldTag_;
    protected List<FieldGrid> grids_;
    private Tilemap tilemap_;

    private void Awake() {
        tilemap_ = GetComponent<Tilemap>();
        grids_ = new List<FieldGrid>();
    }

    public void SetTag(FieldTag tag) {
        fieldTag_ = tag;
    }

    public void AddGrid(FieldGrid grid, RuleTile tile = null) {
        grid.AddTag(fieldTag_);
        if (tile != null) {
            tilemap_.SetTile(Vector3Int.FloorToInt(grid.GetCenter()), tile);
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
}

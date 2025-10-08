using System.Collections.Generic;
using UnityEngine;

public class ItemTool : Tool {
    protected override void Awake() {
        base.Awake();
        scopeRanges_ = new List<Vector2Int> {
          new Vector2Int(3, 3),
          new Vector2Int(9, 3),
          new Vector2Int(9, 9)
        };
    }

    public override List<CursorMeta> GetCursorMetas(List<FieldGrid> grids, FieldGrid start, Vector3 pos, int amount) {
        ResetStatus();
        int hold_level = GetHoldLevel();
        if (hold_level == -1) {
            foreach (Item item in start.items) {
                if (!item.freezed && item.ToolUsable(start, this, hold_level)) {
                    AddStatus(ItemStatus.Usable);
                    return new List<CursorMeta> { new CursorMeta(item.AlignGrid(), start, item, CursorMode.ValidPos) };
                }
            }
            return new List<CursorMeta> { new CursorMeta(pos, start, null, CursorMode.Invalid) };
        }
        List<CursorMeta> metas = new List<CursorMeta>();
        List<FieldGrid> v_grids = hold_level == 0 ? new List<FieldGrid> { start } : grids;
        int use_count = GetUseCount();
        foreach (FieldGrid grid in v_grids) {
            foreach (Item item in grid.items) {
                if (!item.freezed && item.ToolUsable(grid, this, hold_level)) {
                    metas.Add(new CursorMeta(item.AlignGrid(), grid, item, CursorMode.ValidPos));
                }
                if (metas.Count >= use_count) {
                    break;
                }
            }
            if (metas.Count >= use_count) {
                break;
            }
        }
        return metas;
    }

    public override Dictionary<ItemData, int> Apply(List<Cursor> cursors, int amount) {
        int level = GetHoldLevel();
        AudioManager.Instance.PlaySound(toolType.ToString());
        Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();
        foreach (Cursor cursor in cursors) {
            Dictionary<ItemData, int> c_items = cursor.item.ToolApply(cursor.grid, this, level);
            foreach (KeyValuePair<ItemData, int> pair in c_items) {
                if (!items.ContainsKey(pair.Key)) {
                    items[pair.Key] = pair.Value;
                } else {
                    items[pair.Key] += pair.Value;
                }
            }
        }
        return items;
    }

    protected override Vector2Int GetScopeRange() {
        int level = GetHoldLevel();
        if (level <= 0) { return scopeRanges_[0]; }
        if (level == 1) { return scopeRanges_[1]; }
        return scopeRanges_[2];
    }

    protected override int GetUseCount() {
        int hold_level = GetHoldLevel();
        if (hold_level <= 0) { return 1; }
        return hold_level * 5;
    }

    protected override int holdLevelMax { get { return scopeRanges_.Count - 1; } }

}

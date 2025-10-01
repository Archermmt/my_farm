using System.Collections.Generic;
using UnityEngine;

public class Tool : Item {
  public override (Vector3, Vector3) GetScope(FieldGrid center, Vector3 grid_min, Vector3 grid_max, Direction direct) {
    return GetScopeByDirect(center, grid_min, grid_max, direct);
  }

  public override List<CursorMeta> GetCursorMetas(List<FieldGrid> grids, FieldGrid start, Vector3 pos) {
    ResetStatus();
    int hold_level = GetHoldLevel();
    if (hold_level == -1) {
      if (!grids.Contains(start)) {
        AddStatus(ItemStatus.Unusable);
        return new List<CursorMeta> { new CursorMeta(pos, start, null, CursorMode.Invalid) };
      }
      if (GridUsable(start)) {
        AddStatus(ItemStatus.GridUsable);
        return new List<CursorMeta> { new CursorMeta(start.GetCenter(), start, null, CursorMode.ValidGrid) };
      }
      foreach (Item item in start.items) {
        if (item.ToolUsable(start, toolType, hold_level)) {
          AddStatus(ItemStatus.ItemUsable);
          return new List<CursorMeta> { new CursorMeta(item.AlignGrid(), start, item, CursorMode.ValidPos) };
        }
      }
      AddStatus(ItemStatus.Unusable);
      return new List<CursorMeta> { new CursorMeta(pos, start, null, CursorMode.Invalid) };
    }
    List<CursorMeta> metas = new List<CursorMeta>();
    List<FieldGrid> v_grids = hold_level == 0 ? new List<FieldGrid> { start } : grids;
    int use_count = GetUseCount();
    foreach (FieldGrid grid in v_grids) {
      if (HasStatus(ItemStatus.GridUsable)) {
        if (GridUsable(grid)) {
          metas.Add(new CursorMeta(grid.GetCenter(), grid, null, CursorMode.ValidGrid));
        }
      } else if (HasStatus(ItemStatus.ItemUsable)) {
        foreach (Item item in grid.items) {
          if (item.ToolUsable(grid, toolType, hold_level)) {
            metas.Add(new CursorMeta(item.AlignGrid(), grid, item, CursorMode.ValidPos));
          }
          if (metas.Count >= use_count) {
            break;
          }
        }
      }
      if (metas.Count >= use_count) {
        break;
      }
    }
    return metas;
  }

  public override Dictionary<ItemData, int> Apply(List<Cursor> cursors, int amount) {
    Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();
    if (HasStatus(ItemStatus.ItemUsable)) {
      int level = GetHoldLevel();
      foreach (Cursor cursor in cursors) {
        Dictionary<ItemData, int> c_items = cursor.item.ToolApply(cursor.grid, toolType, level);
        foreach (KeyValuePair<ItemData, int> pair in c_items) {
          if (!items.ContainsKey(pair.Key)) {
            items[pair.Key] = pair.Value;
          } else {
            items[pair.Key] += pair.Value;
          }
        }
      }
    }
    return items;
  }

  protected override bool Dropable(FieldGrid grid) {
    return false;
  }

  protected virtual bool GridUsable(FieldGrid grid) {
    return true;
  }

  protected override Vector2Int GetScopeRange() {
    int level = GetHoldLevel();
    if (level <= 1) { return new Vector2Int(3, 1); }
    if (level == 2) { return new Vector2Int(3, 3); }
    if (level == 3) { return new Vector2Int(9, 3); }
    return new Vector2Int(9, 9);
  }

  protected virtual int GetUseCount() {
    if (GetHoldLevel() <= 0) { return 1; }
    Vector2Int range = GetScopeRange();
    return range.x * range.y;
  }

  protected override int holdLevelMax { get { return 4; } }

  protected virtual ToolType toolType { get { return ToolType.Tool; } }
}

using System.Collections.Generic;
using UnityEngine;

public class Tool : Item {
  public override List<Vector3> EffectField(List<FieldGrid> grids, FieldGrid start, Vector3 pos) {
    ResetStatus();
    if (!HasStatus(ItemStatus.Holding)) {
      if (!grids.Contains(start)) {
        AddStatus(ItemStatus.Unusable);
        return new List<Vector3> { pos };
      }
      if (GridUsable(start)) {
        AddStatus(ItemStatus.GridUsable);
        return new List<Vector3> { pos };
      }
      foreach (Item item in start.items) {
        if (ItemUsable(item)) {
          AddStatus(ItemStatus.ItemUsable);
          return new List<Vector3> { item.transform.position };
        }
      }
      AddStatus(ItemStatus.Unusable);
      return new List<Vector3> { pos };
    }
    List<Vector3> positions = new List<Vector3>();
    List<FieldGrid> v_grids = GetHoldLevel() == 0 ? new List<FieldGrid> { start } : grids;
    int use_count = GetUseCount();
    foreach (FieldGrid grid in v_grids) {
      if (HasStatus(ItemStatus.GridUsable)) {
        if (GridUsable(grid)) {
          positions.Add(grid.GetCenter());
        }
      } else if (HasStatus(ItemStatus.ItemUsable)) {
        foreach (Item item in grid.items) {
          if (ItemUsable(item)) {
            positions.Add(item.transform.position);
          }
          if (positions.Count >= use_count) {
            break;
          }
        }
      }
      if (positions.Count >= use_count) {
        break;
      }
    }
    return positions;
  }

  protected override bool Dropable(FieldGrid grid) {
    return false;
  }

  protected virtual bool GridUsable(FieldGrid grid) {
    return true;
  }

  protected virtual bool ItemUsable(Item other) {
    return true;
  }

  protected override Vector2Int GetScopeRange() {
    int level = GetHoldLevel();
    if (level == -1) { return new Vector2Int(3, 3); }
    if (level <= 1) { return new Vector2Int(3, 1); }
    if (level == 2) { return new Vector2Int(3, 3); }
    if (level == 3) { return new Vector2Int(9, 3); }
    if (level == 4) { return new Vector2Int(9, 9); }
    return Vector2Int.one;
  }

  public virtual int GetUseCount() {
    if (GetHoldLevel() <= 0) { return 1; }
    Vector2Int range = GetScopeRange();
    return range.x * range.y;
  }

  protected override int holdLevelMax { get { return 4; } }
}

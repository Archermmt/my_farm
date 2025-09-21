using System;
using System.Collections.Generic;
using UnityEngine;

public class Seed : Item {
    [SerializeField] private string cropName_;

    public override List<Vector3> EffectField(List<FieldGrid> grids, FieldGrid start, Vector3 pos) {
        ResetStatus();
        if (!HasStatus(ItemStatus.Holding)) {
            if (Plantable(start)) {
                AddStatus(ItemStatus.GridUsable);
                return new List<Vector3> { start.GetCenter() };
            }
            return base.EffectField(grids, start, pos);
        }
        List<Vector3> positions = new List<Vector3>();
        List<FieldGrid> v_grids = GetHoldLevel() == 0 ? new List<FieldGrid> { start } : grids;
        int use_count = GetUseCount();
        foreach (FieldGrid grid in v_grids) {
            if (HasStatus(ItemStatus.GridUsable) && Plantable(grid)) {
                positions.Add(grid.GetCenter());
            }
            if (positions.Count >= use_count) {
                break;
            }
        }
        return positions;
    }

    public override int Apply(List<Cursor> cursors, int amount) {
        int crop_num = Math.Min(cursors.Count, amount);
        for (int i = 0; i < crop_num; i++) {
            Item crop = ItemManager.Instance.CreateItem(cropName_, cursors[i].grid.GetCenter());
            cursors[i].grid.AddItem(crop);
        }
        return crop_num;
    }

    protected virtual bool Plantable(FieldGrid grid) {
        return grid.HasTag(FieldTag.Dug) && grid.items.Count == 0;
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

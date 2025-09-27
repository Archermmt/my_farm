using System;
using System.Collections.Generic;
using UnityEngine;

public class Seed : Item {
    [SerializeField] private string cropName_;

    public override List<CursorMeta> GetCursorMetas(List<FieldGrid> grids, FieldGrid start, Vector3 pos) {
        ResetStatus();
        if (!HasStatus(ItemStatus.Holding)) {
            if (Plantable(start)) {
                AddStatus(ItemStatus.GridUsable);
                return new List<CursorMeta> { new CursorMeta(start.GetCenter(), start, null, CursorMode.ValidGrid) };
            }
            return base.GetCursorMetas(grids, start, pos);
        }
        List<CursorMeta> metas = new List<CursorMeta>();
        List<FieldGrid> v_grids = GetHoldLevel() == 0 ? new List<FieldGrid> { start } : grids;
        int use_count = GetUseCount();
        foreach (FieldGrid grid in v_grids) {
            if (HasStatus(ItemStatus.GridUsable) && Plantable(grid)) {
                metas.Add(new CursorMeta(grid.GetCenter(), grid, null, CursorMode.ValidGrid));
            }
            if (metas.Count >= use_count) {
                break;
            }
        }
        return metas;
    }

    public override Dictionary<string, int> Apply(List<Cursor> cursors, int amount) {
        int crop_num = Math.Min(cursors.Count, amount);
        for (int i = 0; i < crop_num; i++) {
            Item crop = ItemManager.Instance.CreateItem(cropName_, cursors[i].grid.GetCenter());
            cursors[i].grid.AddItem(crop);
        }
        Dictionary<string, int> crops = new Dictionary<string, int> { { meta.name, -crop_num } };
        return crops;
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

using System.Collections.Generic;
using UnityEngine;

public class GridTool : Tool {
    protected override void Awake() {
        base.Awake();
        scopeRanges_ = new List<Vector2Int> {
          new Vector2Int(3, 1),
          new Vector2Int(3, 3),
          new Vector2Int(9, 3),
          new Vector2Int(9, 9)
        };
    }

    public override List<CursorMeta> GetCursorMetas(List<FieldGrid> grids, FieldGrid start, Vector3 pos, int amount) {
        ResetStatus();
        int hold_level = GetHoldLevel();
        if (hold_level == -1) {
            if (grids.Contains(start) && GridUsable(start)) {
                AddStatus(ItemStatus.Usable);
                return new List<CursorMeta> { new CursorMeta(start.GetCenter(), start, null, CursorMode.ValidGrid) };
            }
            return new List<CursorMeta> { new CursorMeta(pos, start, null, CursorMode.Invalid) };
        }
        List<CursorMeta> metas = new List<CursorMeta>();
        List<FieldGrid> v_grids = hold_level == 0 ? new List<FieldGrid> { start } : grids;
        int use_count = GetUseCount();
        foreach (FieldGrid grid in v_grids) {
            if (GridUsable(grid)) {
                metas.Add(new CursorMeta(grid.GetCenter(), grid, null, CursorMode.ValidGrid));
            }
            if (metas.Count >= use_count) {
                break;
            }
        }
        return metas;
    }

    public override Dictionary<ItemData, int> Apply(List<Cursor> cursors, int amount) {
        AudioManager.Instance.PlaySound(toolType.ToString());
        FieldLayer layer = FieldManager.Instance.GetLayer(fieldTag);
        foreach (Cursor cursor in cursors) {
            layer.AddGrid(cursor.grid);
        }
        return new Dictionary<ItemData, int>();
    }

    protected virtual bool GridUsable(FieldGrid grid) {
        return false;
    }

    protected override Vector2Int GetScopeRange() {
        int level = GetHoldLevel();
        if (level <= 1) { return scopeRanges_[0]; }
        if (level == 2) { return scopeRanges_[1]; }
        if (level == 3) { return scopeRanges_[2]; }
        return scopeRanges_[3];
    }

    protected override int GetUseCount() {
        if (GetHoldLevel() <= 0) { return 1; }
        Vector2Int range = GetScopeRange();
        return range.x * range.y;
    }

    protected virtual FieldTag fieldTag { get { return FieldTag.Basic; } }
}

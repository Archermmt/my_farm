using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class WaterCan : Tool
{
    [SerializeField] private RuleTile wateredTile_;

    protected override bool GridUsable(AreaGrid grid)
    {
        return grid.HasTag(AreaTag.Dug);
    }

    public override int Apply(List<Cursor> cursors)
    {
        Tilemap layer = GridManager.Instance.GetLayer(AreaTag.Watered).GetComponent<Tilemap>();
        foreach (Cursor cursor in cursors)
        {
            Vector3 pos = cursor.transform.position;
            GridManager.Instance.GetGrid(pos).AddTag(AreaTag.Watered);
            layer.SetTile(new Vector3Int((int)(pos.x - Settings.gridCellSize / 2), (int)(pos.y - Settings.gridCellSize / 2), 0), wateredTile_);
        }
        return 0;
    }
}

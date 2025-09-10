using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class Hoe : Tool
{
  [SerializeField] private RuleTile dugTile_;

  protected override bool GridUsable(AreaGrid grid)
  {
    return grid.HasTag(AreaTag.Diggable);
  }

  public override int Apply(List<Cursor> cursors)
  {
    Tilemap layer = GridManager.Instance.GetLayer(AreaTag.Dug).GetComponent<Tilemap>();
    foreach (Cursor cursor in cursors)
    {
      Vector3 pos = cursor.transform.position;
      GridManager.Instance.GetGrid(pos).AddTag(AreaTag.Dug);
      layer.SetTile(new Vector3Int((int)(pos.x - Settings.gridCellSize / 2), (int)(pos.y - Settings.gridCellSize / 2), 0), dugTile_);
    }
    return 0;
  }
}

using UnityEngine;
using System.Collections.Generic;

public class Hoe : Tool
{
  [SerializeField] private RuleTile dugTile_;

  protected override bool GridUsable(FieldGrid grid)
  {
    return grid.HasTag(FieldTag.Diggable) && !grid.HasTag(FieldTag.Dug) && grid.items.Count == 0;
  }

  public override int Apply(List<Cursor> cursors)
  {
    Field field = FieldManager.Instance.GetField(FieldTag.Dug);
    foreach (Cursor cursor in cursors)
    {
      field.AddTile(cursor.transform.position, dugTile_, FieldTag.Dug);
    }
    return 0;
  }

  public override AnimationTag animationTag { get { return AnimationTag.Hoe; } }
}

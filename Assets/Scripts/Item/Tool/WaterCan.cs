using UnityEngine;
using System.Collections.Generic;

public class WaterCan : Tool
{
    [SerializeField] private RuleTile wateredTile_;

    protected override bool GridUsable(FieldGrid grid)
    {
        return grid.HasTag(FieldTag.Dug) && !grid.HasTag(FieldTag.Watered);
    }

    public override int Apply(List<Cursor> cursors)
    {
        Field field = FieldManager.Instance.GetField(FieldTag.Watered);
        foreach (Cursor cursor in cursors)
        {
            field.AddTile(cursor.transform.position, wateredTile_, FieldTag.Watered);
        }
        return 0;
    }
}

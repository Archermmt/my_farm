using System.Collections.Generic;
using UnityEngine;

public class Seed : Item
{
    public override List<Vector3> EffectField(List<FieldGrid> grids, FieldGrid start, Vector3 pos, Vector3 min, Vector3 max)
    {
        ResetStatus();
        if (Plantable(start))
        {
            AddStatus(ItemStatus.GridUsable);
            return new List<Vector3> { start.GetCenter() };
        }
        return base.EffectField(grids, start, pos, min, max);
    }

    protected virtual bool Plantable(FieldGrid grid)
    {
        return grid.HasTag(FieldTag.Dug);
    }
}

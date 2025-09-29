using System.Collections.Generic;
using System.Linq;

public class WateredLayer : FieldLayer {
    public override void UpdateTime(TimeType time_type, TimeData time, int delta) {
        if (time_type == TimeType.Day) {
            List<FieldGrid> watered_grids = grids_.Where(g => g.HasTag(FieldTag.Watered)).ToList();
            foreach (FieldGrid grid in watered_grids) {
                RemoveGrid(grid);
            }
        }
    }
}

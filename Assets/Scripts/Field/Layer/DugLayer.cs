public class DugLayer : FieldLayer {
  public override void UpdateTime(TimeType time_type, TimeData time, int delta) {
    if (time_type == TimeType.Day) {
      foreach (FieldGrid grid in grids_) {
        if (!grid.HasTag(FieldTag.Watered)) {
          continue;
        }
        foreach (Item item in grid.items) {
          if (item is Crop) {
            Crop crop = (Crop)item;
            crop.Growth(delta);
          }
        }
      }
    }
  }
}

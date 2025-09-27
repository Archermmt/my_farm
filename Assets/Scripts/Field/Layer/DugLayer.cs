public class DugLayer : FieldLayer {
  public override void UpdateTime(TimeType time_type, int year, Season season, int month, int week, int day, int hour, int minute, int second) {
    if (time_type == TimeType.Day) {
      foreach (FieldGrid grid in grids_) {
        if (!grid.HasTag(FieldTag.Watered)) {
          continue;
        }
        foreach (Item item in grid.items) {
          if (item is Crop) {
            Crop crop = (Crop)item;
            crop.Growth();
          }
        }
      }
    }
  }
}

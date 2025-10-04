public class Crop : Plant {
  public override void UpdateTime(TimeType time_type, TimeData time, int delta, FieldGrid grid) {
    if (time_type == TimeType.Day && grid.HasTag(FieldTag.Watered)) {
      Growth(delta);
    }
  }
}

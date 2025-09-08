public class Hoe : Tool
{
  protected override bool GridUsable(EnvGrid grid)
  {
    return grid.HasTag(AreaTag.Diggable);
  }
}

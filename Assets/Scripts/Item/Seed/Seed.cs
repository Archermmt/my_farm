public class Seed : Item
{
    protected override bool GridUsable(EnvGrid grid)
    {
        return grid.HasTag(AreaTag.Dug);
    }
}

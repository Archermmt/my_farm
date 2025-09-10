public class Seed : Item
{
    protected override bool GridUsable(AreaGrid grid)
    {
        return grid.HasTag(AreaTag.Dug);
    }
}

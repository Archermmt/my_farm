public class Seed : Item
{
    protected override bool GridUsable(FieldGrid grid)
    {
        return grid.HasTag(FieldTag.Dug);
    }
}

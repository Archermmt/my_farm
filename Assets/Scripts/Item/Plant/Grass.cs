public class Grass : Plant {
    public override void SetItem(ItemData item_data) {
        base.SetItem(item_data);
        AddStatus(ItemStatus.Nudgable);
        AddStatus(ItemStatus.Fadable);
    }

    public override bool ToolUsable(FieldGrid grid, ToolType tool_type, int hold_level) {
        return tool_type == ToolType.Scythe;
    }
}

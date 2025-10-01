public class Rock : Obstacle {
    public override bool ToolUsable(FieldGrid grid, ToolType tool_type, int hold_level) {
        return tool_type == ToolType.Pickaxe;
    }
}

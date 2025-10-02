public class TreeStump : Plant {
    public override bool ToolUsable(FieldGrid grid, ToolType tool_type, int hold_level) {
        return tool_type == ToolType.Axe;
    }
}

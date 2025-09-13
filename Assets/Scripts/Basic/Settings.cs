public enum Direction
{
    Up,
    Down,
    Left,
    Right,
    Around
}

public enum Action
{
    Idle,
    Walk,
    Run,
    DropItem,
    UseItem,
    HoldItem
}

public enum AnimationTag
{
    Carry,
    Tool,
    Hoe
}

public enum ContainerType
{
    ToolBar,
    Pocket,
    Backpack
}

public enum ItemType
{
    Seed,
    Food,
    Object,
    Tool,
    Plant,
    Obstacle,
    Furniture,
}

public enum ItemStatus
{
    Dropable,
    GridUsable,
    ItemUsable,
    GridUnusable,
    PosUnusable,
    Holding,
    None
}

public enum FieldTag
{
    Basic,
    Diggable,
    Dropable,
    Dug,
    Watered,
}

public enum CursorMode
{
    ValidGrid,
    ValidPos,
    Invalid,
    Mask,
    Mute
}

public static class Settings
{
    // Tilemap
    public const float gridCellSize = 1f;

    // animation
    public static float useToolPause = 0.35f;
}

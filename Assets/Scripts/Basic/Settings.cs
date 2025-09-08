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
    UseItem
}

public enum AnimationTag
{
    Carry
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
    None
}

public enum AreaTag
{
    Basic,
    Diggable,
    Dropable,
    Dug,
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
}

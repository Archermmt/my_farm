public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public enum HumanAction
{
    Idle,
    Walk,
    Run
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

public enum AnimationTag
{
    Carry
}

public enum AreaTag
{
    Basic,
    Diggable,
    Dropable,
}

public static class Settings
{
    // Tilemap
    public const float gridCellSize = 1f;
}

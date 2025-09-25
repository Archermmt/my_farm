public enum Direction {
    Up,
    Down,
    Left,
    Right,
    Around
}

public enum Action {
    Idle,
    Walk,
    Run,
    DropItem,
    UseItem,
    HoldItem
}

public enum AnimationTag {
    Carry,
    Hoe,
    WaterCan,
    Scythe
}

public enum ContainerType {
    ToolBar,
    Pocket,
    Backpack
}

public enum ItemType {
    Seed,
    Food,
    Object,
    Tool,
    Plant,
    Obstacle,
    Furniture,
}

public enum ToolType {
    Tool,
    Hoe,
    WaterCan,
    Scythe,
    Basket,
}

public enum ItemStatus {
    Dropable,
    GridUsable,
    ItemUsable,
    Unusable,
    Holding,
    Destroyable,
    None
}

public enum FieldTag {
    Basic,
    Diggable,
    Dropable,
    Dug,
    Watered,
}

public enum CursorMode {
    ValidGrid,
    ValidPos,
    Invalid,
    Mask,
    Mute
}

public enum Season {
    Spring,
    Summer,
    Autumn,
    Winter
}

public enum TimeType {
    Year,
    Season,
    Month,
    Week,
    Day,
    Hour,
    Minute,
    Second
}

public enum SceneName {
    StartScene,
    FarmScene,
    FieldScene,
    CabinScene,
}

public static class Settings {
    // Tilemap
    public const float gridCellSize = 1f;
}

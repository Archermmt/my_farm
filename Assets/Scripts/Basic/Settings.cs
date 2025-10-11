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
    Wave,
    Pick,
    Lift,
    Swing,
    Hoe,
    WaterCan,
    Scythe,
    Basket,
    Pickaxe,
    Axe
}

public enum ContainerType {
    ToolBar,
    Pocket,
    Backpack,
    Any
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
    Pickaxe,
    Axe
}

public enum ItemStatus {
    Dropable,
    Usable,
    Holding,
    Nudgable,
    Fadable,
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
    Month,
    Day,
    Hour,
    Minute,
    Second
}

public enum EffectType {
    Harvest,
    GrassScatter,
    ConeFall,
    LeavesFall,
    StoneBreak,
    TrunkChunk,
    Destroy,
    None
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

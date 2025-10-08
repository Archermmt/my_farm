using System.Collections.Generic;

public class Axe : ItemTool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Wave, AnimationTag.Axe };
    }

    public override ToolType toolType { get { return ToolType.Axe; } }
}

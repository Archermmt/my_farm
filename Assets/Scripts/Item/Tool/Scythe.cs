using System.Collections.Generic;
using UnityEngine;

public class Scythe : ItemTool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Swing, AnimationTag.Scythe };
    }

    public override ToolType toolType { get { return ToolType.Scythe; } }
}

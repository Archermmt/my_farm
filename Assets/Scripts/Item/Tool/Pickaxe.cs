using System.Collections.Generic;
using UnityEngine;

public class Pickaxe : ItemTool {
    protected override void Awake() {
        base.Awake();
        animationTags_ = new List<AnimationTag> { AnimationTag.Wave, AnimationTag.Pickaxe };
    }

    public override ToolType toolType { get { return ToolType.Pickaxe; } }
}

using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SwapClips {
    public AnimationClip[] srcClips;
    public AnimationClip[] dstClips;
    public AnimationTag animationTag;
}

public class AnimationSwapper : MonoBehaviour {
    [SerializeField] private SwapClips[] swapClipsList_;
    private Dictionary<AnimationTag, List<KeyValuePair<AnimationClip, AnimationClip>>> swapAnimations_;
    private Dictionary<AnimationTag, List<KeyValuePair<AnimationClip, AnimationClip>>> restoreAnimations_;
    private Animator animator_;

    public void Restore(AnimationTag animation_tag) {
        if (restoreAnimations_.ContainsKey(animation_tag)) {
            AnimatorOverrideController controller = new AnimatorOverrideController(animator_.runtimeAnimatorController);
            controller.ApplyOverrides(restoreAnimations_[animation_tag]);
            animator_.runtimeAnimatorController = controller;
        }
    }

    public void Swap(AnimationTag animation_tag) {
        if (swapAnimations_.ContainsKey(animation_tag)) {
            AnimatorOverrideController controller = new AnimatorOverrideController(animator_.runtimeAnimatorController);
            controller.ApplyOverrides(swapAnimations_[animation_tag]);
            animator_.runtimeAnimatorController = controller;
        }
    }

    private void Awake() {
        animator_ = GetComponent<Animator>();
        swapAnimations_ = new Dictionary<AnimationTag, List<KeyValuePair<AnimationClip, AnimationClip>>>();
        restoreAnimations_ = new Dictionary<AnimationTag, List<KeyValuePair<AnimationClip, AnimationClip>>>();
        foreach (SwapClips clips in swapClipsList_) {
            swapAnimations_[clips.animationTag] = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            restoreAnimations_[clips.animationTag] = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            for (int i = 0; i < clips.srcClips.Length; i++) {
                swapAnimations_[clips.animationTag].Add(new KeyValuePair<AnimationClip, AnimationClip>(clips.srcClips[i], clips.dstClips[i]));
                restoreAnimations_[clips.animationTag].Add(new KeyValuePair<AnimationClip, AnimationClip>(clips.dstClips[i], clips.srcClips[i]));
            }
        }
    }


}

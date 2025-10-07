using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TreeStump : Plant {
    [Header("TreeStump")]
    [SerializeField] private float wobbleSecs_ = 0.5f;
    [SerializeField] private float destroySecs_ = 0.5f;
    private Animator animator_;
    private WaitForSeconds wobbleWait_;
    private WaitForSeconds destroyWait_;
    private bool wobbling_;

    protected override void Awake() {
        animator_ = GetComponent<Animator>();
        wobbleWait_ = new WaitForSeconds(wobbleSecs_);
        destroyWait_ = new WaitForSeconds(destroySecs_);
        base.Awake();
    }

    public override void DestroyItem(FieldGrid grid) {
        StartCoroutine(DestroyRoutine(grid));
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, Tool tool, int hold_level) {
        direction_ = tool.transform.position.x > transform.position.x ? Direction.Left : Direction.Right;
        if (!wobbling_) {
            StartCoroutine(WobbleRoutine(direction_));
        }
        return base.ToolApply(grid, tool, hold_level);
    }

    private IEnumerator WobbleRoutine(Direction direction) {
        wobbling_ = true;
        animator_.SetTrigger("wobble");
        animator_.SetInteger("direction", (int)direction);
        yield return wobbleWait_;
        wobbling_ = false;
    }

    private IEnumerator DestroyRoutine(FieldGrid grid) {
        animator_.SetTrigger("destroy");
        animator_.SetInteger("direction", (int)direction_);
        yield return destroyWait_;
        base.DestroyItem(grid);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TreeTrunk : Plant {
    [Header("TreeTrunk")]
    [SerializeField] private TreeStump stump_;
    [SerializeField] private int maturePeriod_;
    [SerializeField] private float wobbleSecs_ = 0.5f;
    [SerializeField] private float destroySecs_ = 1f;
    private BoxCollider2D collider_;
    private Animator animator_;
    private WaitForSeconds wobbleWait_;
    private WaitForSeconds destroyWait_;
    private Vector2 matureSize_;
    private Vector2 matureOffset_;
    private bool wobbling_;

    protected override void Awake() {
        collider_ = GetComponent<BoxCollider2D>();
        animator_ = GetComponent<Animator>();
        wobbleWait_ = new WaitForSeconds(wobbleSecs_);
        destroyWait_ = new WaitForSeconds(destroySecs_);
        matureSize_ = collider_.size;
        matureOffset_ = collider_.offset;
        base.Awake();
        if (stump_ != null) {
            stump_.SetFreeze(true);
            stump_.gameObject.SetActive(false);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);
        if (stump_ != null && currentPeriod_ >= maturePeriod_) {
            triggerable_.TriggerItemEnter(collision, stump_);
        } else if (triggerable_.Nudgable(collision, this) && !wobbling_) {
            Direction direction = transform.position.x > collision.transform.position.x ? Direction.Right : Direction.Left;
            StartCoroutine(WobbleRoutine(direction));
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        triggerable_.TriggerItemExit(collision, this);
        if (stump_ != null && currentPeriod_ >= maturePeriod_) {
            triggerable_.TriggerItemExit(collision, stump_);
        } else if (triggerable_.Nudgable(collision, this) && !wobbling_) {
            Direction direction = transform.position.x > collision.transform.position.x ? Direction.Right : Direction.Left;
            StartCoroutine(WobbleRoutine(direction));
        }
    }

    public override void DestroyItem(FieldGrid grid) {
        if (currentPeriod_ >= maturePeriod_) {
            AudioManager.Instance.PlaySound("TreeFall");
            StartCoroutine(DestroyRoutine(grid));
        } else {
            base.DestroyItem(grid);
        }
    }

    protected override void UpdatePeriod() {
        base.UpdatePeriod();
        if (currentPeriod_ >= maturePeriod_) {
            collider_.size = matureSize_;
            collider_.offset = matureOffset_;
            if (stump_ != null) {
                stump_.gameObject.SetActive(true);
                stump_.transform.parent = transform.parent;
                stump_.gameObject.name = gameObject.name + "_Stump";
                stump_.SetGenerate(true);
                stump_.Growth(0);
            }
        } else {
            collider_.size = render_.sprite.bounds.size;
            collider_.offset = new Vector2(0, 0);
        }
    }

    public override Dictionary<ItemData, int> ToolApply(FieldGrid grid, Tool tool, int hold_level) {
        direction_ = tool.transform.position.x > transform.position.x ? Direction.Left : Direction.Right;
        if (currentPeriod_ >= maturePeriod_ && !wobbling_) {
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
        if (stump_ != null && currentPeriod_ >= maturePeriod_) {
            stump_.SetFreeze(false);
            grid.AddItem(stump_);
        }
        base.DestroyItem(grid);
    }
}

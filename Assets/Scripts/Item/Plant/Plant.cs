using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : Fadable {
    [Header("Growth")]
    [SerializeField] private List<int> growthPeriods_;
    [SerializeField] private List<Sprite> growthSprites_;
    [SerializeField] private int growthDay_ = 0;

    [Header("Nudge")]
    [SerializeField] private List<string> nudgeTargets_;
    [SerializeField] private float nudgePauseSecs_ = 0.04f;
    private bool rotating_ = false;
    private WaitForSeconds nudgePause_;
    private int currentPeriod_;
    protected int totalPeriod_;

    protected override void OnTriggerEnter2D(Collider2D collision) {
        if (Nudgable(collision)) {
            if (!rotating_) {
                if (transform.position.x < collision.transform.position.x) {
                    StartCoroutine(RotateAntiClockwise());
                } else {
                    StartCoroutine(RotateClockwise());
                }
            }
        } else {
            base.OnTriggerEnter2D(collision);
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        if (Nudgable(collision)) {
            if (!rotating_) {
                if (transform.position.x > collision.transform.position.x) {
                    StartCoroutine(RotateAntiClockwise());
                } else {
                    StartCoroutine(RotateClockwise());
                }
            }
        } else {
            base.OnTriggerExit2D(collision);
        }
    }

    public override void SetItem(ItemData item_data) {
        base.SetItem(item_data);
        UpdatePeriod();
        nudgePause_ = new WaitForSeconds(nudgePauseSecs_);
        totalPeriod_ = growthPeriods_.Count;
    }

    public virtual void Growth() {
        growthDay_ += 1;
        UpdatePeriod();
    }

    protected virtual void UpdatePeriod() {
        currentPeriod_ = growthPeriods_.Count - 1;
        for (int i = 0; i < growthPeriods_.Count; i++) {
            if (growthPeriods_[i] >= growthDay_) {
                currentPeriod_ = i;
                break;
            }
        }
        ChangeSprite(growthSprites_[currentPeriod_]);
    }

    private IEnumerator RotateClockwise() {
        rotating_ = true;
        for (int i = 0; i < 4; i++) {
            transform.Rotate(0f, 0f, -2f);
            yield return nudgePause_;
        }
        for (int i = 0; i < 5; i++) {
            transform.Rotate(0f, 0f, 2f);
            yield return nudgePause_;
        }
        transform.Rotate(0f, 0f, -2f);
        yield return nudgePause_;
        rotating_ = false;
    }

    private IEnumerator RotateAntiClockwise() {
        rotating_ = true;
        for (int i = 0; i < 4; i++) {
            transform.Rotate(0f, 0f, 2f);
            yield return nudgePause_;
        }
        for (int i = 0; i < 5; i++) {
            transform.Rotate(0f, 0f, -2f);
            yield return nudgePause_;
        }
        transform.Rotate(0f, 0f, 2f);
        yield return nudgePause_;
        rotating_ = false;
    }

    protected virtual bool Nudgable(Collider2D collision) {
        return gameObject.activeInHierarchy && nudgeTargets_ != null && nudgeTargets_.Contains(collision.tag);
    }

    protected override bool Pickable(FieldGrid grid) {
        return false;
    }

    protected override bool Dropable(FieldGrid grid) {
        return false;
    }

    public override bool ToolUsable(ToolType tool_type, int hold_level) {
        return tool_type == ToolType.Scythe;
    }

    public override ItemStatus ToolApply(ToolType tool_type, int hold_level) {
        if (tool_type == ToolType.Scythe) {
            return ItemStatus.Destroyable;
        }
        return base.ToolApply(tool_type, hold_level);
    }

    public int currentPeriod { get { return currentPeriod_; } }
}

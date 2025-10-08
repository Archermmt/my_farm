using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Triggerable))]
public class TreeTrunk : TreeBase {
    [Header("TreeTrunk")]
    [SerializeField] private Sprite stumpSprite_;
    [SerializeField] private int maturePeriod_;
    private TreeBase stump_;
    private Triggerable triggerable_;
    private BoxCollider2D collider_;
    private Vector2 matureSize_;
    private Vector2 matureOffset_;

    protected override void Awake() {
        collider_ = GetComponent<BoxCollider2D>();
        triggerable_ = GetComponent<Triggerable>();
        matureSize_ = collider_.size;
        matureOffset_ = collider_.offset;
        base.Awake();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        triggerable_.TriggerItemEnter(collision, this);
        if (stump_ != null) {
            triggerable_.TriggerItemEnter(collision, stump_);
        } else if (triggerable_.Nudgable(collision, this) && !wobbling_) {
            Direction direction = transform.position.x > collision.transform.position.x ? Direction.Right : Direction.Left;
            StartCoroutine(WobbleRoutine(direction));
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        triggerable_.TriggerItemExit(collision, this);
        if (stump_ != null) {
            triggerable_.TriggerItemExit(collision, stump_);
        } else if (triggerable_.Nudgable(collision, this) && !wobbling_) {
            Direction direction = transform.position.x > collision.transform.position.x ? Direction.Right : Direction.Left;
            StartCoroutine(WobbleRoutine(direction));
        }
    }

    public override void DestroyItem(FieldGrid grid) {
        if (currentPeriod_ >= maturePeriod_) {
            AudioManager.Instance.PlaySound("TreeFall");
            base.DestroyItem(grid);
            if (stump_ != null) {
                stump_.SetFreeze(false);
                grid.AddItem(stump_);
            }
        } else {
            Destroy(gameObject);
        }
    }

    protected override void UpdatePeriod() {
        base.UpdatePeriod();
        if (currentPeriod_ >= maturePeriod_) {
            collider_.size = matureSize_;
            collider_.offset = matureOffset_;
            if (stumpSprite_ != null) {
                stump_ = (TreeBase)ItemManager.Instance.CreateItem(stumpSprite_, transform.position, transform.parent, gameObject.name + "_Stump");
                stump_.gameObject.SetActive(true);
                stump_.SetFreeze(true);
                stump_.SetGenerate(true);
                stump_.Growth(0);
            }
        } else {
            collider_.size = render_.sprite.bounds.size;
            collider_.offset = new Vector2(0, collider_.size.y / 2);
        }
    }
}

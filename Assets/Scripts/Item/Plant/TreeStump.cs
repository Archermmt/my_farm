using UnityEngine;

public class TreeStump : Plant {

    private Animator animator_;

    protected override void Awake() {
        animator_ = GetComponent<Animator>();
        base.Awake();
    }

    public override void DestroyItem(FieldGrid grid) {
        base.DestroyItem(grid);
        Destroy(transform.parent.gameObject);
    }
}

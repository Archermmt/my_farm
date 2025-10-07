using UnityEngine;

[RequireComponent(typeof(Triggerable))]
public class Plant : Harvestable {
    protected Triggerable triggerable_;

    protected override void Awake() {
        base.Awake();
        triggerable_ = GetComponent<Triggerable>();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        triggerable_.TriggerItemEnter(collision, this);
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
        triggerable_.TriggerItemExit(collision, this);
    }
}

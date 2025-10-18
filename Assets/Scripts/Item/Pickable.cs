using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Pickable : Item {
    [Header("Pickable")]
    [SerializeField] private List<string> trackTargets_;
    [SerializeField] private float speedFactor_ = 2f;
    [SerializeField] private float pickRadius_ = 0.2f;
    [SerializeField] private float trackRadius_ = 3f;
    private CircleCollider2D collider_;
    private Transform target_;

    protected override void Awake() {
        base.Awake();
        collider_ = GetComponent<CircleCollider2D>();
        collider_.radius = trackRadius_;
    }

    private void Update() {
        if (target_ == null || ItemManager.Instance.freezed || freezed_) {
            return;
        }
        float distance = Vector3.Distance(target_.position, transform.position);
        float step = speedFactor_ / distance * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target_.position, step);
        if (distance <= pickRadius_) {
            string tag = target_.tag;
            EventHandler.CallAddInventoryItem(tag, meta, 1);
            AudioManager.Instance.TriggerSound("PickUp");
            ItemManager.Instance.RemovePickable(this);
            target_ = null;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (trackTargets_.Contains(collision.tag) && target_ == null) {
            target_ = collision.transform;
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
        if (trackTargets_.Contains(collision.tag) && target_ != null && target_.tag == collision.tag) {
            target_ = null;
            SetFreeze(false);
        }
    }
}

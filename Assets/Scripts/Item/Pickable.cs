using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Pickable : MonoBehaviour {
    [SerializeField] private List<string> trackTargets_;
    [SerializeField] private float speedFactor_ = 2f;
    [SerializeField] private float pickRadius_ = 0.2f;
    [SerializeField] private float trackRadius_ = 3f;
    private CircleCollider2D collider_;
    private Transform target_;

    private void Awake() {
        collider_ = GetComponent<CircleCollider2D>();
        collider_.radius = trackRadius_;
    }

    public void StartTrack(Collider2D collision) {
        if (trackTargets_.Contains(collision.tag) && target_ == null) {
            target_ = collision.transform;
        }
    }

    public void EndTrack(Collider2D collision) {
        if (trackTargets_.Contains(collision.tag) && target_ != null && target_.tag == collision.tag) {
            target_ = null;
        }
    }

    public string Track(Item item) {
        if (target_ == null) {
            return "";
        }
        float distance = Vector3.Distance(target_.position, item.transform.position);
        float step = speedFactor_ / distance * Time.deltaTime;
        item.transform.position = Vector3.MoveTowards(transform.position, target_.position, step);
        if (distance <= pickRadius_) {
            string tag = target_.tag;
            EventHandler.CallAddInventoryItem(tag, item.meta, 1);
            AudioManager.Instance.TriggerSound("PickUp");
            target_ = null;
            return tag;
        }
        return "";
    }
}

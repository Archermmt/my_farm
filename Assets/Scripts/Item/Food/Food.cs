using UnityEngine;

[RequireComponent(typeof(Pickable))]
public class Food : Item {
  private Pickable pickable_;

  protected override void Awake() {
    base.Awake();
    pickable_ = GetComponent<Pickable>();
  }

  private void FixedUpdate() {
    string owner = pickable_.Track(this);
    if (owner.Length > 0) {
      Destroy(gameObject);
    }
  }

  protected virtual void OnTriggerEnter2D(Collider2D collision) {
    pickable_.StartTrack(collision);
  }

  protected virtual void OnTriggerExit2D(Collider2D collision) {
    pickable_.EndTrack(collision);
  }
}

using UnityEngine;

public class Crop : Plant {
  [SerializeField] private int seedPeriod_ = 0;

  protected override bool Nudgable(Collider2D collision) {
    return currentPeriod > seedPeriod_ && base.Nudgable(collision);
  }
}

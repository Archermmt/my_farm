using UnityEngine;

public class Food : Pickable {
  [SerializeField] private int recover_ = 1;

  public int RecoverEnergy() {
    return recover_;
  }
}

using System.Collections;
using UnityEngine;

public class Crop : Plant {
  [Header("Crop.Basic")]
  [SerializeField] private string matureName_;
  [SerializeField] private int seedPeriod_ = 0;
  [SerializeField] private int reapPeriod_ = -1;
  [SerializeField] private float harvestStart_ = 0.1f;
  [SerializeField] private float harvestEnd_ = 1f;

  private Animator animator_;
  // harvest
  private SpriteRenderer harvestRender_;
  private ItemData mature_data_;

  public override void SetItem(ItemData item_data) {
    base.SetItem(item_data);
    animator_ = GetComponent<Animator>();
    harvestRender_ = transform.Find("Harvest").GetComponent<SpriteRenderer>();
    mature_data_ = ItemManager.Instance.FindItem(matureName_);
  }

  public override bool ToolUsable(FieldGrid grid, ToolType tool_type, int hold_level) {
    if (currentPeriod_ <= seedPeriod_) {
      return false;
    }
    if (tool_type == ToolType.Scythe) {
      return reapPeriod_ == -1 || currentPeriod_ <= reapPeriod_;
    }
    if (tool_type == ToolType.Basket) {
      return currentPeriod_ == totalPeriod_ - 1;
    }
    return false;
  }

  public override void ToolApply(FieldGrid grid, ToolType tool_type, int hold_level) {
    if (tool_type == ToolType.Scythe) {
      grid.RemoveItem(this);
      Destroy(gameObject);
    } else if (tool_type == ToolType.Basket) {
      StartCoroutine(Harvest(grid));
    } else {
      base.ToolApply(grid, tool_type, hold_level);
    }
  }

  private IEnumerator Harvest(FieldGrid grid) {
    renderer_.sprite = null;
    yield return new WaitForSeconds(Random.Range(0.0f, harvestStart_));
    harvestRender_.sprite = mature_data_.sprite;
    animator_.SetTrigger("harvest");
    yield return new WaitForSeconds(harvestEnd_);
    grid.RemoveItem(this);
    Destroy(gameObject);
  }

  protected override bool Nudgable(Collider2D collision) {
    return currentPeriod_ > seedPeriod_ && base.Nudgable(collision);
  }

  public string matureName { get { return mature_data_.name; } }
}

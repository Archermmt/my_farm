using System;
using System.Collections.Generic;
using UnityEngine;

public class Tool : Item
{
  private DateTime holdStart_;
  private Direction direction_;

  public virtual void Hold(Direction direction)
  {
    AddStatus(ItemStatus.Holding);
    direction_ = direction;
    holdStart_ = DateTime.Now;
  }

  public virtual void Unhold()
  {
    RemoveStatus(ItemStatus.Holding);
  }

  public override (Vector3, Vector3) GetScope(FieldGrid center, Vector3 pos, Vector3 grid_min, Vector3 grid_max)
  {
    if (!HasStatus(ItemStatus.Holding)) { return base.GetScope(center, pos, grid_min, grid_max); }
    int level = GetHoldingLevel();
    if (level == 0) { return GetScopeByRadius(center, pos, grid_min, grid_max, useRadius, direction_); }
    Vector3 c_pos = center.position;
    Vector3 min = Vector3.zero;
    Vector3 max = Vector3.zero;
    Vector2Int range = GetHoldingRange(level);
    int near = (range.x - 1) / 2;
    int far = range.y;
    if (direction_ == Direction.Up)
    {
      min = new Vector3(Mathf.Max(c_pos.x - near, grid_min.x), Mathf.Max(c_pos.y + 1, grid_min.y), c_pos.z);
      max = new Vector3(Mathf.Min(c_pos.x + near, grid_max.x), Mathf.Min(c_pos.y + far, grid_max.y), c_pos.z);
    }
    else if (direction_ == Direction.Down)
    {
      min = new Vector3(Mathf.Max(c_pos.x - near, grid_min.x), Mathf.Max(c_pos.y - far, grid_min.y), c_pos.z);
      max = new Vector3(Mathf.Min(c_pos.x + near, grid_max.x), Mathf.Min(c_pos.y - 1, grid_max.y), c_pos.z);
    }
    else if (direction_ == Direction.Left)
    {
      min = new Vector3(Mathf.Max(c_pos.x - far, grid_min.x), Mathf.Max(c_pos.y - near, grid_min.y), c_pos.z);
      max = new Vector3(Mathf.Min(c_pos.x - 1, grid_max.x), Mathf.Min(c_pos.y + near, grid_max.y), c_pos.z);
    }
    else if (direction_ == Direction.Right)
    {
      min = new Vector3(Mathf.Max(c_pos.x + 1, grid_min.x), Mathf.Max(c_pos.y - near, grid_min.y), c_pos.z);
      max = new Vector3(Mathf.Min(c_pos.x + far, grid_max.x), Mathf.Min(c_pos.y + near, grid_max.y), c_pos.z);
    }
    return (min, max);

  }

  public override List<Vector3> EffectField(List<FieldGrid> grids, Vector3 pos, Vector3 min, Vector3 max)
  {
    if (!HasStatus(ItemStatus.Holding)) { return base.EffectField(grids, pos, min, max); }
    int level = GetHoldingLevel();
    if (level == 0) { return base.EffectField(grids, pos, min, max); }
    List<Vector3> positions = new List<Vector3>();
    foreach (FieldGrid grid in grids)
    {
      if (!GridUsable(grid))
      {
        continue;
      }
      if (HasStatus(ItemStatus.GridUsable))
      {
        positions.Add(grid.GetCenter());
      }
      else if (HasStatus(ItemStatus.ItemUsable))
      {
        foreach (Item item in grid.items)
        {
          if (ItemUsable(item) && positions.Count < effectNum - 1)
          {
            positions.Add(item.transform.position);
          }
        }
      }
      if (positions.Count >= effectNum)
      {
        break;
      }
    }
    return positions;
  }

  protected float GetHoldingTime()
  {
    TimeSpan diff = DateTime.Now - holdStart_;
    return (float)diff.TotalSeconds;
  }

  protected int GetHoldingLevel()
  {
    return Math.Min(Mathf.FloorToInt(GetHoldingTime()), holdingMax);
  }

  protected virtual Vector2Int GetHoldingRange(int level)
  {
    if (level == 1) { return new Vector2Int(3, 1); }
    if (level == 2) { return new Vector2Int(3, 3); }
    if (level == 3) { return new Vector2Int(9, 3); }
    if (level == 4) { return new Vector2Int(9, 9); }
    return Vector2Int.one;
  }

  public virtual int holdingMax { get { return 4; } }

  public override int useRadius { get { return HasStatus(ItemStatus.Holding) ? Math.Max(GetHoldingLevel(), 1) : 1; } }

  public override int effectNum
  {
    get
    {
      if (!HasStatus(ItemStatus.Holding)) { return 1; }
      int level = GetHoldingLevel();
      if (level == 0) { return 1; }
      Vector2Int range = GetHoldingRange(level);
      return range.x * range.y;
    }
  }

  public virtual AnimationTag animationTag { get { return AnimationTag.Tool; } }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class Tool : Item
{
  private DateTime holdStart_;

  public virtual void Hold(Direction direction)
  {
    AddStatus(ItemStatus.Holding);
    direction_ = direction;
    holdStart_ = DateTime.Now;
  }

  public virtual void Unhold()
  {
    RemoveStatus(ItemStatus.Holding);
    direction_ = Direction.Around;
  }

  public override List<Vector3> EffectField(List<FieldGrid> grids, FieldGrid start, Vector3 pos, Vector3 min, Vector3 max)
  {
    if (!HasStatus(ItemStatus.Holding))
    {
      ResetStatus();
      if (!grids.Contains(start))
      {
        AddStatus(ItemStatus.ItemUnusable);
        return new List<Vector3> { pos };
      }
      if (GridUsable(start))
      {
        AddStatus(ItemStatus.GridUsable);
        return new List<Vector3> { start.GetCenter() };
      }
      foreach (Item item in start.items)
      {
        if (ItemUsable(item))
        {
          AddStatus(ItemStatus.ItemUsable);
          return new List<Vector3> { item.transform.position };
        }
      }
      AddStatus(ItemStatus.ItemUnusable);
      return new List<Vector3> { pos };
    }
    List<Vector3> positions = new List<Vector3>();
    foreach (FieldGrid grid in grids)
    {
      if (HasStatus(ItemStatus.GridUsable))
      {
        if (GridUsable(grid))
        {
          positions.Add(grid.GetCenter());
        }
      }
      else if (HasStatus(ItemStatus.ItemUsable))
      {
        foreach (Item item in grid.items)
        {
          if (ItemUsable(item))
          {
            positions.Add(item.transform.position);
          }
          if (positions.Count >= useCount)
          {
            break;
          }
        }
      }
      if (positions.Count >= useCount)
      {
        break;
      }
    }
    return positions;
  }

  protected override bool Dropable(FieldGrid grid)
  {
    return false;
  }

  protected virtual bool GridUsable(FieldGrid grid)
  {
    return true;
  }

  protected virtual bool ItemUsable(Item other)
  {
    return true;
  }

  private float GetHoldingTime()
  {
    TimeSpan diff = DateTime.Now - holdStart_;
    return (float)diff.TotalSeconds;
  }

  protected int GetHoldingLevel()
  {
    if (!HasStatus(ItemStatus.Holding)) { return 0; }
    return Math.Min(Mathf.FloorToInt(GetHoldingTime()), holdingMax);
  }

  protected override Vector2Int GetScopeRange()
  {
    if (!HasStatus(ItemStatus.Holding))
    {
      return new Vector2Int(3, 3);
    }
    int level = GetHoldingLevel();
    if (level <= 1) { return new Vector2Int(3, 1); }
    if (level == 2) { return new Vector2Int(3, 3); }
    if (level == 3) { return new Vector2Int(9, 3); }
    if (level == 4) { return new Vector2Int(9, 9); }
    return Vector2Int.one;
  }

  public virtual int holdingMax { get { return 4; } }

  public virtual int useCount
  {
    get
    {
      if (!HasStatus(ItemStatus.Holding)) { return 1; }
      int level = GetHoldingLevel();
      if (level == 0) { return 1; }
      Vector2Int range = GetScopeRange();
      return range.x * range.y;
    }
  }
}

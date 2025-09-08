using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class EnvGrid
{
    private Vector3 position_;
    private Vector2Int coord_;
    private List<AreaTag> areaTags_;
    private Dictionary<Vector3, Item> items_;

    public EnvGrid(Vector3 position, Vector2Int coord)
    {
        position_ = position;
        coord_ = coord;
        areaTags_ = new List<AreaTag>();
        items_ = new Dictionary<Vector3, Item>();
    }

    public void AddTag(AreaTag tag)
    {
        if (!areaTags_.Contains(tag))
        {
            areaTags_.Add(tag);
        }
    }

    public bool HasTag(AreaTag tag)
    {
        return areaTags_.Contains(tag);
    }

    public void RemoveTag(AreaTag tag)
    {
        if (areaTags_.Contains(tag))
        {
            areaTags_.Remove(tag);
        }
    }

    public void AddItem(Item item)
    {
        if (!items_.ContainsKey(item.transform.position))
        {
            items_[item.transform.position] = item;
        }
    }

    public Item FindItem(Vector3 pos)
    {
        if (items_.ContainsKey(pos))
        {
            return items_[pos];
        }
        return null;
    }

    public void RemoveItem(Item item)
    {
        if (items_.ContainsKey(item.transform.position))
        {
            items_.Remove(item.transform.position);
        }
    }

    public Vector3 Position
    {
        get { return position_; }
    }

    public Vector2Int Coord
    {
        get { return coord_; }
    }

    public List<AreaTag> AreaTags
    {
        get { return areaTags_; }
    }

    public Dictionary<Vector3, Item> Items
    {
        get { return items_; }
    }

    public override string ToString()
    {
        string str = "Coord[" + coord_ + "] @ " + position_ + ":";
        if (areaTags_ != null && areaTags_.Count > 0)
        {
            str += " <" + areaTags_.Count + " Tags>:";
            foreach (AreaTag tag in areaTags_)
            {
                str += tag.ToString() + ",";
            }
        }
        if (items_ != null && items_.Count > 0)
        {
            str += " <" + items_.Count + " Items>:";
            foreach (Item item in items_.Values)
            {
                str += item.ToString() + " @ " + item.transform.position + ",";
            }
        }
        return str;
    }
}


public class EnvManager : Singleton<EnvManager>
{
    private EnvGrid[,] envGrids_;
    private Grid grid_;
    private Vector3Int start_, end_;
    private List<Cursor> envCursors_;
    private List<Cursor> maskCursors_;
    private bool freezed_;

    protected override void Awake()
    {
        base.Awake();
        envCursors_ = new List<Cursor>();
        maskCursors_ = new List<Cursor>();
        freezed_ = false;
        ParseGrids();
    }

    public EnvGrid GridAt(int x, int y)
    {
        if (x >= envGrids_.GetLength(0) || y >= envGrids_.GetLength(1))
        {
            return null;
        }
        return envGrids_[x, y];
    }

    public EnvGrid GetGrid(Vector3 world_pos)
    {
        Vector3 rounded_pos = new Vector3(Mathf.Round(world_pos.x), Mathf.Round(world_pos.y), world_pos.z);
        Vector3Int cell_pos = grid_.WorldToCell(rounded_pos);
        return GridAt(cell_pos.x - start_.x, cell_pos.y - start_.y);
    }

    public Item DropItem(Item item, Cursor cursor)
    {
        Item new_item = ItemManager.Instance.CreateItem(item.Meta, cursor.transform.position);
        EnvGrid grid = GetGrid(cursor.transform.position);
        grid.AddItem(new_item);
        return new_item;
    }

    public int UseItem(Item item, Cursor cursor)
    {
        EnvGrid grid = GetGrid(cursor.transform.position);
        Debug.Log("[TMINFO] use item!! " + item + " @ " + cursor);
        Debug.Log("grid " + grid);
        return 0;
    }

    public Cursor GetEnvCursor(int idx)
    {
        while (idx >= envCursors_.Count)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefab/Env/Cursor");
            Assert.AreNotEqual(prefab, null, "Can not find cursor prefab");
            GameObject cursor_obj = Instantiate(prefab, transform);
            cursor_obj.name = "EnvCursor_" + envCursors_.Count.ToString();
            envCursors_.Add(cursor_obj.GetComponent<Cursor>());
        }
        return envCursors_[idx];
    }

    public Cursor GetMaskCursor(int idx)
    {
        while (idx >= maskCursors_.Count)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefab/Env/Cursor");
            Assert.AreNotEqual(prefab, null, "Can not find cursor prefab");
            GameObject cursor_obj = Instantiate(prefab, transform);
            cursor_obj.GetComponent<Cursor>().SetMode(CursorMode.Mask);
            cursor_obj.name = "MaskCursor_" + maskCursors_.Count.ToString();
            maskCursors_.Add(cursor_obj.GetComponent<Cursor>());
        }
        return maskCursors_[idx];
    }

    public List<Cursor> CheckItem(Item item, Vector3 anchor, Vector3 world_pos)
    {
        if (freezed_)
        {
            return new List<Cursor>();
        }
        EnvGrid center = GetGrid(anchor);
        if (center == null)
        {
            return new List<Cursor>();
        }
        EnvGrid start = GetGrid(world_pos);
        if (start == null)
        {
            return new List<Cursor>();
        }

        (Vector3 min, Vector3 max) = item.GetScope(center, start, world_pos, start_, end_);
        List<EnvGrid> grids = ExpandGrids(center, min, max);
        for (int i = 0; i < grids.Count; i++)
        {
            GetMaskCursor(i).MoveTo(grids[i].Position, CursorMode.Mask);
        }
        for (int i = grids.Count; i < maskCursors_.Count; i++)
        {
            GetMaskCursor(i).SetMode(CursorMode.Mute);
        }
        (List<Vector3> positions, List<ItemStatus> status_list) = item.EffectEnv(grids, start, world_pos, min, max);
        List<Cursor> cursors = new List<Cursor>();
        for (int i = 0; i < positions.Count; i++)
        {
            Cursor cursor = GetEnvCursor(i);
            cursor.Reset();
            if (status_list.Contains(ItemStatus.GridUsable))
            {
                cursor.MoveTo(positions[i], CursorMode.ValidGrid);
            }
            else if (status_list.Contains(ItemStatus.ItemUsable))
            {
                cursor.MoveTo(positions[i], CursorMode.ValidPos);
                cursor.SetItem(GetGrid(positions[i]).FindItem(positions[i]));
            }
            else if (status_list.Contains(ItemStatus.Dropable))
            {
                cursor.MoveTo(positions[i], CursorMode.ValidPos);
            }
            else if (status_list.Contains(ItemStatus.GridUnusable))
            {
                cursor.MoveTo(positions[i], CursorMode.Invalid);
            }
            else
            {
                cursor.MoveTo(positions[i], CursorMode.Invalid);
            }
            cursor.SetStatusList(status_list);
            cursors.Add(cursor);
        }
        for (int i = positions.Count; i < envCursors_.Count; i++)
        {
            GetEnvCursor(i).SetMode(CursorMode.Mute);
        }
        return cursors;
    }

    public void Freeze()
    {
        freezed_ = true;
        foreach (Cursor cursor in envCursors_)
        {
            cursor.SetMode(CursorMode.Mute);
        }
        foreach (Cursor cursor in maskCursors_)
        {
            cursor.SetMode(CursorMode.Mute);
        }
    }

    public void Unfreeze()
    {
        freezed_ = false;
    }

    private void ParseGrids()
    {
        Transform parent = GameObject.FindGameObjectWithTag("Areas").transform;
        grid_ = parent.GetComponent<Grid>();
        Tilemap basicMap = parent.Find("Basic").GetComponent<Tilemap>();
        basicMap.CompressBounds();
        start_ = basicMap.cellBounds.min;
        end_ = basicMap.cellBounds.max;
        envGrids_ = new EnvGrid[end_.x - start_.x, end_.y - start_.y];
        foreach (Vector3Int pos in basicMap.cellBounds.allPositionsWithin)
        {
            Vector2Int coord = new Vector2Int(pos.x - start_.x, pos.y - start_.y);
            envGrids_[coord.x, coord.y] = new EnvGrid(pos, coord);
        }
        foreach (Transform child in parent)
        {
            Tilemap tilemap = child.GetComponent<Tilemap>();
            tilemap.CompressBounds();
            Area area = child.GetComponent<Area>();
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
            {
                TileBase tile = tilemap.GetTile(pos);
                if (tile == null)
                {
                    continue;
                }
                foreach (AreaTag tag in area.AreaTags)
                {
                    envGrids_[pos.x - start_.x, pos.y - start_.y].AddTag(tag);
                }
            }
        }
    }

    private List<EnvGrid> ExpandGrids(EnvGrid start, Vector3 min, Vector3 max, bool include_start = true)
    {
        List<EnvGrid> grids = new List<EnvGrid>();
        List<EnvGrid> frontier = new List<EnvGrid> { start };
        while (frontier.Count > 0)
        {
            EnvGrid current = frontier[0];
            grids.Add(current);
            frontier.RemoveAt(0);
            int x = current.Coord.x;
            int y = current.Coord.y;
            List<EnvGrid> next_grids = new List<EnvGrid> { GridAt(x, y - 1), GridAt(x + 1, y), GridAt(x, y + 1), GridAt(x - 1, y) };
            foreach (EnvGrid grid in next_grids)
            {
                if (grid == null || grid.Position.x < min.x || grid.Position.x > max.x || grid.Position.y < min.y || grid.Position.y > max.y)
                {
                    continue;
                }
                if (!grids.Contains(grid) && !frontier.Contains(grid))
                {
                    frontier.Add(grid);
                }
            }
        }
        if (!include_start)
        {
            grids.RemoveAt(0);
        }
        return grids;
    }
}

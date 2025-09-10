using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class AreaGrid
{
    private Vector3 position_;
    private Vector2Int coord_;
    private Dictionary<AreaTag, int> areaTags_;
    private Dictionary<Vector3, Item> items_;

    public AreaGrid(Vector3 position, Vector2Int coord)
    {
        position_ = position;
        coord_ = coord;
        areaTags_ = new Dictionary<AreaTag, int>();
        items_ = new Dictionary<Vector3, Item>();
    }

    public Vector3 GetCenter()
    {
        return new Vector3(position_.x + Settings.gridCellSize / 2, position_.y + Settings.gridCellSize / 2, position_.z);
    }

    public void AddTag(AreaTag tag, int duration = 0)
    {
        if (!areaTags_.ContainsKey(tag))
        {
            areaTags_[tag] = duration;
        }
    }

    public void IncreaseTag(AreaTag tag, int inc = 1)
    {
        if (areaTags_.ContainsKey(tag))
        {
            areaTags_[tag] += inc;
        }
    }

    public bool HasTag(AreaTag tag)
    {
        return areaTags_.ContainsKey(tag);
    }

    public void RemoveTag(AreaTag tag)
    {
        if (areaTags_.ContainsKey(tag))
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

    public Vector3 position
    {
        get { return position_; }
    }

    public Vector2Int coord
    {
        get { return coord_; }
    }

    public Dictionary<AreaTag, int> areaTags
    {
        get { return areaTags_; }
    }

    public Dictionary<Vector3, Item> items
    {
        get { return items_; }
    }

    public override string ToString()
    {
        string str = "Coord[" + coord_ + "] @ " + position_ + ":";
        if (areaTags_ != null && areaTags_.Count > 0)
        {
            str += " <" + areaTags_.Count + " Tags>:";
            foreach (KeyValuePair<AreaTag, int> pair in areaTags_)
            {
                str += pair.Key.ToString() + "|" + pair.Value.ToString() + ",";
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


public class GridManager : Singleton<GridManager>
{
    private AreaGrid[,] envGrids_;
    private Grid grid_;
    private Vector3Int start_, end_;
    private List<Cursor> envCursors_;
    private List<Cursor> maskCursors_;
    private Transform layersHolder_;
    private List<Transform> layers_;
    private bool freezed_;

    protected override void Awake()
    {
        base.Awake();
        envCursors_ = new List<Cursor>();
        maskCursors_ = new List<Cursor>();
        layers_ = new List<Transform>();
        freezed_ = false;
        ParseAreas();
    }

    public AreaGrid GridAt(int x, int y)
    {
        if (x >= envGrids_.GetLength(0) || y >= envGrids_.GetLength(1))
        {
            return null;
        }
        return envGrids_[x, y];
    }

    public AreaGrid GetGrid(Vector3 world_pos)
    {
        Vector3Int cell_pos = grid_.WorldToCell(world_pos);
        return GridAt(cell_pos.x - start_.x, cell_pos.y - start_.y);
    }

    public int DropItem(Item item, List<Cursor> cursors)
    {
        foreach (Cursor cursor in cursors)
        {
            Item new_item = ItemManager.Instance.CreateItem(item.meta, cursor.transform.position);
            AreaGrid grid = GetGrid(cursor.transform.position);
            grid.AddItem(new_item);
            cursor.SetMode(CursorMode.Mute);
        }
        return cursors.Count;
    }

    public int UseItem(Item item, List<Cursor> cursors)
    {
        foreach (Cursor cursor in cursors)
        {
            cursor.SetMode(CursorMode.Mute);
        }
        return item.Apply(cursors);
    }

    public Cursor GetEnvCursor(int idx)
    {
        while (idx >= envCursors_.Count)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefab/Env/Cursor");
            Assert.AreNotEqual(prefab, null, "Can not find cursor prefab");
            GameObject cursor_obj = Instantiate(prefab, transform.Find("EnvCursors").transform);
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
            GameObject cursor_obj = Instantiate(prefab, transform.Find("MaskCursors").transform);
            cursor_obj.GetComponent<Cursor>().SetMode(CursorMode.Mask);
            cursor_obj.name = "MaskCursor_" + maskCursors_.Count.ToString();
            maskCursors_.Add(cursor_obj.GetComponent<Cursor>());
        }
        return maskCursors_[idx];
    }

    public Transform GetLayer(AreaTag tag)
    {
        foreach (Transform layer in layers_)
        {
            if (layer.GetComponent<Area>().HasTag(tag))
            {
                return layer;
            }
        }
        GameObject prefab = Resources.Load<GameObject>("Prefab/Env/Layer");
        Assert.AreNotEqual(prefab, null, "Can not find cursor prefab");
        GameObject layer_obj = Instantiate(prefab, layersHolder_);
        layer_obj.GetComponent<Area>().AddTag(tag);
        layer_obj.name = "Layer_" + tag.ToString();
        TilemapRenderer render = layer_obj.GetComponent<TilemapRenderer>();
        render.sortingOrder += layers_.Count;
        layers_.Add(layer_obj.transform);
        return layer_obj.transform;
    }

    public List<Cursor> CheckItem(Item item, Vector3 anchor, Vector3 world_pos)
    {
        if (freezed_)
        {
            return new List<Cursor>();
        }
        AreaGrid center = GetGrid(anchor);
        if (center == null)
        {
            return new List<Cursor>();
        }
        AreaGrid start = GetGrid(world_pos);
        if (start == null)
        {
            return new List<Cursor>();
        }

        (Vector3 min, Vector3 max) = item.GetScope(center, start, world_pos, start_, end_);
        List<AreaGrid> grids = ExpandGrids(center, min, max);
        for (int i = 0; i < grids.Count; i++)
        {
            GetMaskCursor(i).MoveTo(grids[i].GetCenter(), CursorMode.Mask);
        }
        for (int i = grids.Count; i < maskCursors_.Count; i++)
        {
            GetMaskCursor(i).SetMode(CursorMode.Mute);
        }
        List<Vector3> positions = item.EffectEnv(grids, start, world_pos, min, max);
        List<Cursor> cursors = new List<Cursor>();
        for (int i = 0; i < positions.Count; i++)
        {
            Cursor cursor = GetEnvCursor(i);
            if (item.HasStatus(ItemStatus.GridUsable))
            {
                cursor.MoveTo(positions[i], CursorMode.ValidGrid);
            }
            else if (item.HasStatus(ItemStatus.ItemUsable))
            {
                cursor.MoveTo(positions[i], CursorMode.ValidPos);
            }
            else if (item.HasStatus(ItemStatus.Dropable))
            {
                cursor.MoveTo(positions[i], CursorMode.ValidPos);
            }
            else if (item.HasStatus(ItemStatus.GridUnusable))
            {
                cursor.MoveTo(positions[i], CursorMode.Invalid);
            }
            else
            {
                cursor.MoveTo(positions[i], CursorMode.Invalid);
            }
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

    private void ParseAreas()
    {
        Transform parent = GameObject.FindGameObjectWithTag("Areas").transform;
        grid_ = parent.GetComponent<Grid>();
        layersHolder_ = parent.Find("Layers").transform;
        Transform masks = parent.Find("Masks").transform;
        Tilemap basicMap = masks.Find("Basic").GetComponent<Tilemap>();
        basicMap.CompressBounds();
        start_ = basicMap.cellBounds.min;
        end_ = basicMap.cellBounds.max;
        envGrids_ = new AreaGrid[end_.x - start_.x, end_.y - start_.y];
        foreach (Vector3Int pos in basicMap.cellBounds.allPositionsWithin)
        {
            Vector2Int coord = new Vector2Int(pos.x - start_.x, pos.y - start_.y);
            envGrids_[coord.x, coord.y] = new AreaGrid(pos, coord);
        }
        foreach (Transform child in masks)
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
                foreach (AreaTag tag in area.areaTags)
                {
                    envGrids_[pos.x - start_.x, pos.y - start_.y].AddTag(tag);
                }
            }
        }
    }

    private List<AreaGrid> ExpandGrids(AreaGrid start, Vector3 min, Vector3 max, bool include_start = true)
    {
        List<AreaGrid> grids = new List<AreaGrid>();
        List<AreaGrid> frontier = new List<AreaGrid> { start };
        while (frontier.Count > 0)
        {
            AreaGrid current = frontier[0];
            grids.Add(current);
            frontier.RemoveAt(0);
            int x = current.coord.x;
            int y = current.coord.y;
            List<AreaGrid> next_grids = new List<AreaGrid> { GridAt(x, y - 1), GridAt(x + 1, y), GridAt(x, y + 1), GridAt(x - 1, y) };
            foreach (AreaGrid grid in next_grids)
            {
                if (grid == null || grid.position.x < min.x || grid.position.x > max.x || grid.position.y < min.y || grid.position.y > max.y)
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

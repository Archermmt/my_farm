using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FieldManager : Singleton<FieldManager>
{
    private FieldGrid[,] fieldGrids_;
    private Grid grid_;
    private Vector3Int start_, end_;
    private List<Cursor> envCursors_;
    private List<Cursor> maskCursors_;
    private Transform layersHolder_;
    private List<Field> fields_;
    private bool freezed_;

    protected override void Awake()
    {
        base.Awake();
        envCursors_ = new List<Cursor>();
        maskCursors_ = new List<Cursor>();
        fields_ = new List<Field>();
        freezed_ = false;
        ParseFields();
    }

    public FieldGrid GridAt(int x, int y)
    {
        if (x >= fieldGrids_.GetLength(0) || y >= fieldGrids_.GetLength(1) || x < 0 || y < 0)
        {
            return null;
        }
        return fieldGrids_[x, y];
    }

    public FieldGrid GetGrid(Vector3 world_pos)
    {
        Vector3Int cell_pos = grid_.WorldToCell(world_pos);
        return GridAt(cell_pos.x - start_.x, cell_pos.y - start_.y);
    }

    public int DropItem(Item item, List<Cursor> cursors)
    {
        foreach (Cursor cursor in cursors)
        {
            Item new_item = ItemManager.Instance.CreateItem(item.meta, cursor.transform.position);
            FieldGrid grid = GetGrid(cursor.transform.position);
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
            GameObject prefab = Resources.Load<GameObject>("Prefab/Field/Cursor");
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
            GameObject prefab = Resources.Load<GameObject>("Prefab/Field/Cursor");
            Assert.AreNotEqual(prefab, null, "Can not find cursor prefab");
            GameObject cursor_obj = Instantiate(prefab, transform.Find("MaskCursors").transform);
            cursor_obj.GetComponent<Cursor>().SetMode(CursorMode.Mask);
            cursor_obj.name = "MaskCursor_" + maskCursors_.Count.ToString();
            maskCursors_.Add(cursor_obj.GetComponent<Cursor>());
        }
        return maskCursors_[idx];
    }

    public Field GetField(FieldTag tag)
    {
        foreach (Field field in fields_)
        {
            if (field.fieldTag == tag)
            {
                return field;
            }
        }
        GameObject prefab = Resources.Load<GameObject>("Prefab/Field/Field");
        Assert.AreNotEqual(prefab, null, "Can not find field prefab");
        GameObject field_obj = Instantiate(prefab, layersHolder_);
        Field new_field = field_obj.GetComponent<Field>();
        new_field.SetTag(tag);
        field_obj.name = "Field_" + tag.ToString();
        TilemapRenderer render = field_obj.GetComponent<TilemapRenderer>();
        render.sortingOrder += fields_.Count;
        fields_.Add(new_field);
        return new_field;
    }

    public List<Cursor> CheckItem(Item item, Vector3 anchor, Vector3 world_pos)
    {
        if (freezed_)
        {
            return new List<Cursor>();
        }
        FieldGrid center = GetGrid(anchor);
        if (center == null)
        {
            return new List<Cursor>();
        }
        (Vector3 min, Vector3 max) = item.GetScope(center, world_pos, start_, end_);
        if (min == max && min == Vector3.zero)
        {
            return new List<Cursor>();
        }
        List<FieldGrid> grids = ExpandGrids(center, min, max);
        for (int i = 0; i < grids.Count; i++)
        {
            GetMaskCursor(i).MoveTo(grids[i].GetCenter(), CursorMode.Mask);
        }
        for (int i = grids.Count; i < maskCursors_.Count; i++)
        {
            GetMaskCursor(i).SetMode(CursorMode.Mute);
        }
        List<Vector3> positions = item.EffectField(grids, world_pos, min, max);
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

    private void ParseFields()
    {
        Transform parent = GameObject.FindGameObjectWithTag("Fields").transform;
        grid_ = parent.GetComponent<Grid>();
        layersHolder_ = parent.Find("Layers").transform;
        Transform masks = parent.Find("Masks").transform;
        Tilemap basicMap = masks.Find("Basic").GetComponent<Tilemap>();
        basicMap.CompressBounds();
        start_ = basicMap.cellBounds.min;
        end_ = basicMap.cellBounds.max;
        fieldGrids_ = new FieldGrid[end_.x - start_.x, end_.y - start_.y];
        foreach (Vector3Int pos in basicMap.cellBounds.allPositionsWithin)
        {
            Vector2Int coord = new Vector2Int(pos.x - start_.x, pos.y - start_.y);
            fieldGrids_[coord.x, coord.y] = new FieldGrid(pos, coord);
        }
        foreach (Transform child in masks)
        {
            Tilemap tilemap = child.GetComponent<Tilemap>();
            tilemap.CompressBounds();
            Field field = child.GetComponent<Field>();
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
            {
                TileBase tile = tilemap.GetTile(pos);
                if (tile != null)
                {
                    fieldGrids_[pos.x - start_.x, pos.y - start_.y].AddTag(field.fieldTag);
                }
            }
        }
    }

    private List<FieldGrid> ExpandGrids(FieldGrid start, Vector3 min, Vector3 max, bool include_start = false)
    {
        List<FieldGrid> grids = new List<FieldGrid>();
        List<FieldGrid> frontier = new List<FieldGrid> { start };
        while (frontier.Count > 0)
        {
            FieldGrid current = frontier[0];
            grids.Add(current);
            frontier.RemoveAt(0);
            int x = current.coord.x;
            int y = current.coord.y;
            List<FieldGrid> next_grids = new List<FieldGrid> { GridAt(x, y - 1), GridAt(x + 1, y), GridAt(x, y + 1), GridAt(x - 1, y) };
            foreach (FieldGrid grid in next_grids)
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

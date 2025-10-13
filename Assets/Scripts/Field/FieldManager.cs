using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;


[System.Serializable]
public class GridSave {
    public FieldTag tag;
    public Vector2IntSave coord;

    public GridSave(FieldGrid grid, FieldTag tag) {
        coord = new Vector2IntSave(grid.coord);
        this.tag = tag;
    }
}

public class FieldManager : Singleton<FieldManager> {
    private List<Cursor> fieldCursors_;
    private List<Cursor> maskCursors_;
    private Grid grid_;
    private FieldGrid[,] fieldGrids_;
    private KeyValuePair<Vector3Int, Vector3Int> scope_;
    private Transform layerHolder_;
    private List<FieldLayer> layers_;
    private HashSet<FieldTag> maskTags_;
    private Dictionary<SceneName, List<GridSave>> gridSaves_;
    private bool freezed_;

    protected override void Awake() {
        base.Awake();
        fieldCursors_ = new List<Cursor>();
        maskCursors_ = new List<Cursor>();
        scope_ = new KeyValuePair<Vector3Int, Vector3Int>();
        layers_ = new List<FieldLayer>();
        maskTags_ = new HashSet<FieldTag>();
        gridSaves_ = new Dictionary<SceneName, List<GridSave>>();
        freezed_ = false;
    }

    private void OnEnable() {
        EventHandler.UpdateTimeEvent += UpdateTime;
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnload;
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable() {
        EventHandler.UpdateTimeEvent -= UpdateTime;
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnload;
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    public FieldGrid GridAt(int x, int y) {
        if (x >= fieldGrids_.GetLength(0) || y >= fieldGrids_.GetLength(1) || x < 0 || y < 0) {
            return null;
        }
        return fieldGrids_[x, y];
    }

    public FieldGrid GetGrid(Vector3 world_pos) {
        Vector3Int cell_pos = grid_.WorldToCell(world_pos);
        Vector3Int start = scope_.Key;
        return GridAt(cell_pos.x - start.x, cell_pos.y - start.y);
    }

    public FieldGrid GetRandomGrid(FieldTag tag) {
        foreach (FieldLayer layer in layers_) {
            if (layer.fieldTag == tag) {
                int idx = Random.Range(0, layer.grids.Count);
                return layer.grids[idx];
            }
        }
        return GridAt(Random.Range(0, fieldGrids_.GetLength(0)), Random.Range(0, fieldGrids_.GetLength(1)));
    }


    public Cursor GetFieldCursor(int idx) {
        while (idx >= fieldCursors_.Count) {
            GameObject prefab = Resources.Load<GameObject>("Prefab/Field/Cursor");
            Assert.AreNotEqual(prefab, null, "Can not find cursor prefab");
            GameObject cursor_obj = Instantiate(prefab, transform.Find("EnvCursors").transform);
            cursor_obj.name = "FieldCursor_" + fieldCursors_.Count.ToString();
            fieldCursors_.Add(cursor_obj.GetComponent<Cursor>());
        }
        return fieldCursors_[idx];
    }

    public Cursor GetMaskCursor(int idx) {
        while (idx >= maskCursors_.Count) {
            GameObject prefab = Resources.Load<GameObject>("Prefab/Field/Cursor");
            Assert.AreNotEqual(prefab, null, "Can not find cursor prefab");
            GameObject cursor_obj = Instantiate(prefab, transform.Find("MaskCursors").transform);
            cursor_obj.GetComponent<Cursor>().SetMode(CursorMode.Mask);
            cursor_obj.name = "MaskCursor_" + maskCursors_.Count.ToString();
            maskCursors_.Add(cursor_obj.GetComponent<Cursor>());
        }
        return maskCursors_[idx];
    }

    public FieldLayer GetLayer(FieldTag tag) {
        foreach (FieldLayer layer in layers_) {
            if (layer.fieldTag == tag) {
                return layer;
            }
        }
        GameObject prefab = Resources.Load<GameObject>("Prefab/Field/Layer/" + tag.ToString() + "Layer");
        if (!prefab) {
            prefab = Resources.Load<GameObject>("Prefab/Field/Layer/FieldLayer");
        }
        Assert.AreNotEqual(prefab, null, "Can not find field prefab");
        FieldLayer new_layer = Instantiate(prefab, layerHolder_).GetComponent<FieldLayer>();
        new_layer.SetTag(tag);
        new_layer.name = "Layer_" + tag.ToString();
        TilemapRenderer render = new_layer.transform.GetComponent<TilemapRenderer>();
        render.sortingOrder += layers_.Count;
        layers_.Add(new_layer);
        return new_layer;
    }

    public List<Cursor> CheckItem(Item item, Vector3 anchor, Vector3 world_pos, Direction mouse_direct, int amount) {
        if (freezed_) {
            return new List<Cursor>();
        }
        FieldGrid center = GetGrid(anchor);
        if (center == null) {
            return new List<Cursor>();
        }
        (Vector3 min, Vector3 max) = item.GetScope(center, scope_.Key, scope_.Value, mouse_direct);
        if (min == max && min == Vector3.zero) {
            return new List<Cursor>();
        }
        List<FieldGrid> grids = ExpandGrids(center, min, max, false);
        for (int i = 0; i < grids.Count; i++) {
            GetMaskCursor(i).MoveTo(grids[i].GetCenter(), CursorMode.Mask);
        }
        for (int i = grids.Count; i < maskCursors_.Count; i++) {
            GetMaskCursor(i).SetMode(CursorMode.Mute);
        }
        FieldGrid start = GetGrid(world_pos);
        if (start == null) {
            return new List<Cursor>();
        }
        List<CursorMeta> metas = item.GetCursorMetas(grids, start, world_pos, amount);
        List<Cursor> cursors = new List<Cursor>();
        for (int i = 0; i < metas.Count; i++) {
            Cursor cursor = GetFieldCursor(i);
            cursor.SetMeta(metas[i]);
            cursors.Add(cursor);
        }
        for (int i = metas.Count; i < fieldCursors_.Count; i++) {
            GetFieldCursor(i).SetMode(CursorMode.Mute);
        }
        return cursors;
    }

    public int DropItem(Item item, List<Cursor> cursors) {
        foreach (Cursor cursor in cursors) {
            Item new_item = ItemManager.Instance.CreateItem(item.meta, cursor.GetItemPos());
            if (new_item is Pickable) {
                ItemManager.Instance.AddPickable((Pickable)new_item, true);
            } else {
                FieldGrid grid = GetGrid(new_item.AlignGrid());
                grid.AddItem(new_item);
            }
            cursor.SetMode(CursorMode.Mute);
        }
        return cursors.Count;
    }

    public Dictionary<ItemData, int> UseItem(Item item, List<Cursor> cursors, int amount) {
        EffectManager.Instance.ClearEffects();
        AudioManager.Instance.ClearSounds();
        Dictionary<ItemData, int> item_amounts = item.Apply(cursors, amount);
        EffectManager.Instance.TriggerEffects();
        AudioManager.Instance.TriggerSounds();
        foreach (Cursor cursor in cursors) {
            cursor.SetMode(CursorMode.Mute);
        }
        return item_amounts;
    }


    private void BeforeSceneUnload(SceneName scene_name) {
        SetFreeze(true);
        gridSaves_[scene_name] = new List<GridSave>();
        foreach (FieldLayer layer in layers_) {
            if (maskTags_.Contains(layer.fieldTag)) {
                continue;
            }
            foreach (FieldGrid grid in layer.grids) {
                gridSaves_[scene_name].Add(new GridSave(grid, layer.fieldTag));
            }
        }
    }

    private void AfterSceneLoad(SceneName scene_name) {
        ParseFields();
        if (gridSaves_.ContainsKey(scene_name)) {
            foreach (GridSave saved in gridSaves_[scene_name]) {
                FieldLayer layer = GetLayer(saved.tag);
                layer.AddGrid(GridAt(saved.coord.x, saved.coord.y));
            }
        }
        SetFreeze(false);
    }

    public void SetFreeze(bool freeze) {
        freezed_ = freeze;
        if (freezed_) {
            foreach (Cursor cursor in fieldCursors_) {
                cursor.SetMode(CursorMode.Mute);
            }
            foreach (Cursor cursor in maskCursors_) {
                cursor.SetMode(CursorMode.Mute);
            }
        }
    }

    private void ParseFields() {
        Transform parent = GameObject.FindGameObjectWithTag("Fields").transform;
        grid_ = parent.GetComponent<Grid>();
        layerHolder_ = parent.Find("Layers").transform;
        Transform masks = parent.Find("Masks").transform;
        Tilemap basicMap = masks.Find("Basic").GetComponent<Tilemap>();
        basicMap.CompressBounds();
        Vector3Int start = basicMap.cellBounds.min;
        Vector3Int end = basicMap.cellBounds.max;
        scope_ = new KeyValuePair<Vector3Int, Vector3Int>(start, end);
        fieldGrids_ = new FieldGrid[end.x - start.x, end.y - start.y];
        foreach (Vector3Int pos in basicMap.cellBounds.allPositionsWithin) {
            Vector2Int coord = new Vector2Int(pos.x - start.x, pos.y - start.y);
            fieldGrids_[coord.x, coord.y] = new FieldGrid(pos, coord);
        }
        // set field tags
        layers_ = new List<FieldLayer>();
        maskTags_ = new HashSet<FieldTag>();
        foreach (Transform child in masks) {
            Tilemap tilemap = child.GetComponent<Tilemap>();
            tilemap.CompressBounds();
            FieldLayer layer = child.GetComponent<FieldLayer>();
            layers_.Add(layer);
            maskTags_.Add(layer.fieldTag);
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin) {
                TileBase tile = tilemap.GetTile(pos);
                if (tile != null) {
                    fieldGrids_[pos.x - start.x, pos.y - start.y].AddTag(layer.fieldTag);
                    layer.AddGrid(fieldGrids_[pos.x - start.x, pos.y - start.y]);
                }
            }
        }
        // add items
        Transform item_parent = GameObject.FindGameObjectWithTag("Items").transform;
        foreach (Item item in item_parent.GetComponentsInChildren<Item>()) {
            if (item is Pickable) {
                ItemManager.Instance.AddPickable((Pickable)item);
            } else {
                GetGrid(item.AlignGrid()).AddItem(item);
            }
        }
    }

    private List<FieldGrid> ExpandGrids(FieldGrid start, Vector3 min, Vector3 max, bool include_start = false) {
        List<FieldGrid> grids = new List<FieldGrid>();
        List<FieldGrid> frontier = new List<FieldGrid> { start };
        while (frontier.Count > 0) {
            FieldGrid current = frontier[0];
            grids.Add(current);
            frontier.RemoveAt(0);
            int x = current.coord.x;
            int y = current.coord.y;
            List<FieldGrid> next_grids = new List<FieldGrid> { GridAt(x, y - 1), GridAt(x + 1, y), GridAt(x, y + 1), GridAt(x - 1, y) };
            foreach (FieldGrid grid in next_grids) {
                if (grid == null || grid.position.x < min.x || grid.position.x > max.x || grid.position.y < min.y || grid.position.y > max.y) {
                    continue;
                }
                if (!grids.Contains(grid) && !frontier.Contains(grid)) {
                    frontier.Add(grid);
                }
            }
        }
        if (!include_start) {
            grids.RemoveAt(0);
        }
        return grids;
    }

    private void UpdateTime(TimeType time_type, TimeData time, int delta) {
        foreach (FieldGrid grid in fieldGrids_) {
            foreach (Item item in grid.items) {
                item.UpdateTime(time_type, time, delta, grid);
            }
        }
        foreach (FieldLayer layer in layers_) {
            layer.UpdateTime(time_type, time, delta);
        }
    }

    public KeyValuePair<Vector3Int, Vector3Int> scope { get { return scope_; } }
}

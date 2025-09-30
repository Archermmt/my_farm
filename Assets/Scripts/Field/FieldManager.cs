using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FieldManager : Singleton<FieldManager> {
    private List<Cursor> fieldCursors_;
    private List<Cursor> maskCursors_;
    private Dictionary<SceneName, KeyValuePair<Vector3Int, Vector3Int>> scopes_;
    private Dictionary<SceneName, Grid> grids_;
    private Dictionary<SceneName, FieldGrid[,]> fieldGrids_;
    private Dictionary<SceneName, Transform> layerHolders_;
    private Dictionary<SceneName, List<FieldLayer>> layers_;
    private SceneName currentScene_ = SceneName.StartScene;
    private bool freezed_;

    protected override void Awake() {
        base.Awake();
        fieldCursors_ = new List<Cursor>();
        maskCursors_ = new List<Cursor>();
        grids_ = new Dictionary<SceneName, Grid>();
        scopes_ = new Dictionary<SceneName, KeyValuePair<Vector3Int, Vector3Int>>();
        fieldGrids_ = new Dictionary<SceneName, FieldGrid[,]>();
        layerHolders_ = new Dictionary<SceneName, Transform>();
        layers_ = new Dictionary<SceneName, List<FieldLayer>>();
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
        FieldGrid[,] field_grids = fieldGrids_[currentScene_];
        if (x >= field_grids.GetLength(0) || y >= field_grids.GetLength(1) || x < 0 || y < 0) {
            return null;
        }
        return field_grids[x, y];
    }

    public FieldGrid GetGrid(Vector3 world_pos) {
        Vector3Int cell_pos = grids_[currentScene_].WorldToCell(world_pos);
        Vector3Int start = scopes_[currentScene_].Key;
        return GridAt(cell_pos.x - start.x, cell_pos.y - start.y);
    }

    public int DropItem(Item item, List<Cursor> cursors) {
        foreach (Cursor cursor in cursors) {
            Item new_item = ItemManager.Instance.CreateItem(item.meta, cursor.transform.position);
            FieldGrid grid = GetGrid(new_item.AlignGrid());
            grid.AddItem(new_item);
            cursor.SetMode(CursorMode.Mute);
        }
        return cursors.Count;
    }

    public Dictionary<ItemData, int> UseItem(Item item, List<Cursor> cursors, int amount) {
        Dictionary<ItemData, int> item_amounts = item.Apply(cursors, amount);
        foreach (Cursor cursor in cursors) {
            cursor.SetMode(CursorMode.Mute);
        }
        return item_amounts;
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
        List<FieldLayer> layers = layers_[currentScene_];
        foreach (FieldLayer layer in layers) {
            if (layer.fieldTag == tag) {
                return layer;
            }
        }
        GameObject prefab = Resources.Load<GameObject>("Prefab/Field/Layer/" + tag.ToString() + "Layer");
        if (!prefab) {
            prefab = Resources.Load<GameObject>("Prefab/Field/Layer/FieldLayer");
        }
        Assert.AreNotEqual(prefab, null, "Can not find field prefab");
        FieldLayer new_layer = Instantiate(prefab, layerHolders_[currentScene_]).GetComponent<FieldLayer>();
        new_layer.SetTag(tag);
        new_layer.name = "Layer_" + tag.ToString();
        TilemapRenderer render = new_layer.transform.GetComponent<TilemapRenderer>();
        render.sortingOrder += layers.Count;
        layers.Add(new_layer);
        return new_layer;
    }

    public List<Cursor> CheckItem(Item item, Vector3 anchor, Vector3 world_pos) {
        if (freezed_) {
            return new List<Cursor>();
        }
        FieldGrid center = GetGrid(anchor);
        if (center == null) {
            return new List<Cursor>();
        }
        (Vector3 min, Vector3 max) = item.GetScope(center, scopes_[currentScene_].Key, scopes_[currentScene_].Value);
        if (min == max && min == Vector3.zero) {
            return new List<Cursor>();
        }
        List<FieldGrid> grids = ExpandGrids(center, min, max, !item.HasStatus(ItemStatus.Holding));
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
        List<CursorMeta> metas = item.GetCursorMetas(grids, start, world_pos);
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

    private void BeforeSceneUnload(SceneName scene_name) {
    }


    private void AfterSceneLoad(SceneName scene_name) {
        currentScene_ = scene_name;
        if (!layerHolders_.ContainsKey(scene_name)) {
            ParseFields(scene_name);
        }
    }

    public void Freeze() {
        freezed_ = true;
        foreach (Cursor cursor in fieldCursors_) {
            cursor.SetMode(CursorMode.Mute);
        }
        foreach (Cursor cursor in maskCursors_) {
            cursor.SetMode(CursorMode.Mute);
        }
    }

    public void Unfreeze() {
        freezed_ = false;
    }

    private void ParseFields(SceneName scene_name) {
        Transform parent = GameObject.FindGameObjectWithTag("Fields").transform;
        grids_[scene_name] = parent.GetComponent<Grid>();
        layerHolders_[scene_name] = parent.Find("Layers").transform;
        Transform masks = parent.Find("Masks").transform;
        Tilemap basicMap = masks.Find("Basic").GetComponent<Tilemap>();
        basicMap.CompressBounds();
        Vector3Int start = basicMap.cellBounds.min;
        Vector3Int end = basicMap.cellBounds.max;
        scopes_[scene_name] = new KeyValuePair<Vector3Int, Vector3Int>(start, end);
        FieldGrid[,] field_grids = new FieldGrid[end.x - start.x, end.y - start.y];
        foreach (Vector3Int pos in basicMap.cellBounds.allPositionsWithin) {
            Vector2Int coord = new Vector2Int(pos.x - start.x, pos.y - start.y);
            field_grids[coord.x, coord.y] = new FieldGrid(pos, coord);
        }
        // set field tags
        foreach (Transform child in masks) {
            Tilemap tilemap = child.GetComponent<Tilemap>();
            tilemap.CompressBounds();
            FieldLayer layer = child.GetComponent<FieldLayer>();
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin) {
                TileBase tile = tilemap.GetTile(pos);
                if (tile != null) {
                    field_grids[pos.x - start.x, pos.y - start.y].AddTag(layer.fieldTag);
                }
            }
        }
        fieldGrids_[scene_name] = field_grids;
        // add items
        Transform item_parent = GameObject.FindGameObjectWithTag("Items").transform;
        foreach (Item item in item_parent.GetComponentsInChildren<Item>()) {
            GetGrid(item.AlignGrid()).AddItem(item);
        }
        // init field layers
        layers_[scene_name] = new List<FieldLayer>();
    }

    private List<FieldGrid> ExpandGrids(FieldGrid start, Vector3 min, Vector3 max, bool include_start = true) {
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
        foreach (FieldLayer layer in layers_[currentScene_]) {
            layer.UpdateTime(time_type, time, delta);
        }
    }
}

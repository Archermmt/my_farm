using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class EnvGrid
{
    private Vector3 coord_;
    private List<AreaTag> areaTags_;
    private List<Item> items_;

    public EnvGrid(Vector3 coord)
    {
        coord_ = coord;
    }

    public void SetCoord(Vector3 coord)
    {
        coord_ = coord;
    }

    public void AddTag(AreaTag area_tag)
    {
        if (areaTags_ == null)
        {
            areaTags_ = new List<AreaTag>();
        }
        if (!areaTags_.Contains(area_tag))
        {
            areaTags_.Add(area_tag);
        }
    }

    public bool AcceptTag(AreaTag area_tag)
    {
        return areaTags_.Contains(area_tag);
    }

    public void AddItem(Item item)
    {
        if (items_ == null)
        {
            items_ = new List<Item>();
        }
        if (!items_.Contains(item))
        {
            items_.Add(item);
        }
    }

    public Vector2 Coord
    {
        get { return coord_; }
    }

    public override string ToString()
    {
        string str = "Coord[" + coord_ + "]:";
        if (areaTags_ != null)
        {
            str += " <" + areaTags_.Count + " Tags>:";
            foreach (AreaTag tag in areaTags_)
            {
                str += tag.ToString() + ",";
            }
        }
        if (items_ != null)
        {
            str += " <" + items_.Count + " Items>:";
            foreach (Item item in items_)
            {
                str += item.ToString() + ",";
            }
        }
        return str;
    }
}


public class EnvManager : Singleton<EnvManager>
{
    private Camera camera_;
    private EnvGrid[,] envGrids_;
    private Grid grid_;
    private Vector3Int start_, end_;

    protected override void Awake()
    {
        base.Awake();
        camera_ = Camera.main;
        ParseGrids();
    }

    public EnvGrid GetGrid(int x, int y)
    {
        return envGrids_[x, y];
    }

    public EnvGrid GetGridInWorld(Vector3 world_pos)
    {
        Vector3Int cell_pos = grid_.WorldToCell(world_pos);
        return GetGrid(cell_pos.x - start_.x, cell_pos.y - start_.y);
    }

    public EnvGrid GetGridAtMouse()
    {
        return GetGridInWorld(CameraUtils.MouseToWorld(camera_));
    }

    public EnvGrid AddItem(Item item)
    {
        EnvGrid grid = GetGridInWorld(item.transform.position);
        grid.AddItem(item);
        return grid;
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
            envGrids_[pos.x - start_.x, pos.y - start_.y] = new EnvGrid(pos);
        }
        foreach (Transform child in parent)
        {
            Tilemap tilemap = child.GetComponent<Tilemap>();
            tilemap.CompressBounds();
            Area area = child.GetComponent<Area>();
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
            {
                foreach (AreaTag tag in area.areaTags)
                {
                    envGrids_[pos.x - start_.x, pos.y - start_.y].AddTag(tag);
                }
            }
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

[System.Serializable]
public class GenerateData {
    public string name;
    public int amountMin = 50;
    public int amountMax = 100;
    public int daysMin = 0;
    public int daysMax = 5;
}

[RequireComponent(typeof(Tilemap))]
public class Generator : MonoBehaviour {
    [SerializeField] private List<GenerateData> generateDatas_;
    private Tilemap tileMap_;
    private List<Vector3Int> tiles_;

    private void Awake() {
        tileMap_ = GetComponent<Tilemap>();
        tiles_ = new List<Vector3Int>();
        tileMap_.CompressBounds();
        foreach (Vector3Int pos in tileMap_.cellBounds.allPositionsWithin) {
            TileBase tile = tileMap_.GetTile(pos);
            if (tile != null) {
                tiles_.Add(pos);
            }
        }
    }

    public void Generate() {
        foreach (GenerateData data in generateDatas_) {
            int amount = Random.Range(data.amountMin, data.amountMax);
            for (int i = 0; i < amount; i++) {
                int idx = Random.Range(0, tiles_.Count);
                Vector3 pos = new Vector3(tiles_[idx].x + Random.Range(0.0f, Settings.gridCellSize), tiles_[idx].y + Random.Range(0.0f, Settings.gridCellSize), tiles_[idx].z);
                Item item = ItemManager.Instance.CreateItem(data.name, pos);
                item.Growth(Random.Range(data.daysMin, data.daysMax));
            }
        }
    }
}

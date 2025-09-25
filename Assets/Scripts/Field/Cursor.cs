using UnityEngine;

public class CursorMeta {
    public Vector3 position;
    public FieldGrid grid;
    public Item item;
    public CursorMode mode = CursorMode.Mute;

    public CursorMeta(Vector3 position, FieldGrid grid, Item item, CursorMode mode) {
        this.position = position;
        this.grid = grid;
        this.item = item;
        this.mode = mode;
    }
}


public class Cursor : MonoBehaviour {
    [SerializeField] private Sprite validGridSprite_;
    [SerializeField] private Sprite validPosSprite_;
    [SerializeField] private Sprite invalidSprite_;
    [SerializeField] private Sprite maskSprite_;
    private SpriteRenderer render_;
    private Sprite disabledSprite_;
    private CursorMeta meta_;
    private CursorMode mode_ = CursorMode.Mute;

    private void Awake() {
        render_ = GetComponent<SpriteRenderer>();
        disabledSprite_ = render_.sprite;
        meta_ = new CursorMeta(Vector3.zero, null, null, CursorMode.Mute);
    }

    public void SetMeta(CursorMeta meta) {
        MoveTo(meta.position, meta.mode);
        meta_ = meta;
    }

    public void MoveTo(Vector3 pos, CursorMode mode) {
        transform.position = pos;
        SetMode(mode);
    }

    public void SetMode(CursorMode mode) {
        if (mode_ == mode) {
            return;
        }
        if (mode == CursorMode.ValidGrid) {
            render_.sprite = validGridSprite_;
            render_.color = new Color(1, 1, 1, 1);
        } else if (mode == CursorMode.ValidPos) {
            render_.sprite = validPosSprite_;
            render_.color = new Color(1, 1, 1, 1);
        } else if (mode == CursorMode.Invalid) {
            render_.sprite = invalidSprite_;
            render_.color = new Color(1, 1, 1, 1);
        } else if (mode == CursorMode.Mask) {
            render_.sprite = maskSprite_;
            render_.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        } else if (mode == CursorMode.Mute) {
            render_.sprite = disabledSprite_;
            render_.color = new Color(1, 1, 1, 1);
        }
        mode_ = mode;
    }

    public override string ToString() {
        string str = "Cursor[" + mode_.ToString() + "] @ " + transform.position;
        if (meta_.grid != null) {
            str += ", Grid: " + meta_.grid;
        }
        if (meta_.item != null) {
            str += ", Item: " + meta_.item;
        }
        return str;
    }

    public CursorMode mode { get { return mode_; } }

    public FieldGrid grid { get { return meta_.grid; } }

    public Item item { get { return meta_.item; } }
}

using UnityEngine;

public class Cursor : MonoBehaviour {
    [SerializeField] private Sprite validGridSprite_;
    [SerializeField] private Sprite validPosSprite_;
    [SerializeField] private Sprite invalidSprite_;
    [SerializeField] private Sprite maskSprite_;
    private SpriteRenderer render_;
    private Sprite disabledSprite_;
    private CursorMode mode_;
    private FieldGrid grid_;

    private void Awake() {
        render_ = GetComponent<SpriteRenderer>();
        disabledSprite_ = render_.sprite;
        mode_ = CursorMode.Mute;
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

    public void BindGrid(FieldGrid grid) {
        grid_ = grid;
    }

    public void UnbindGrid() {
        grid_ = null;
    }

    public override string ToString() {
        string str = "Cursor[" + mode_.ToString() + "] @ " + transform.position;
        return str;
    }

    public CursorMode mode { get { return mode_; } }

    public FieldGrid grid { get { return grid_; } }
}

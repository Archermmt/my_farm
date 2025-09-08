using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] private Sprite validGridSprite_;
    [SerializeField] private Sprite validPosSprite_;
    [SerializeField] private Sprite invalidSprite_;
    [SerializeField] private Sprite maskSprite_;
    private SpriteRenderer render_;
    private Sprite disabledSprite_;
    private CursorMode mode_;
    private Item item_;
    private List<ItemStatus> statusList_;

    private void Awake()
    {
        render_ = GetComponent<SpriteRenderer>();
        disabledSprite_ = render_.sprite;
        mode_ = CursorMode.Mute;
        Reset();
    }

    public bool HasStatus(ItemStatus status)
    {
        return statusList_ != null && statusList_.Contains(status);
    }

    public void Reset()
    {
        item_ = null;
        statusList_ = new List<ItemStatus>();
    }

    public void MoveTo(Vector3 pos, CursorMode mode)
    {
        transform.position = pos;
        SetMode(mode);
    }

    public void SetMode(CursorMode mode)
    {
        if (mode_ == mode)
        {
            return;
        }
        if (mode == CursorMode.ValidGrid)
        {
            render_.sprite = validGridSprite_;
            render_.color = new Color(1, 1, 1, 1);
        }
        else if (mode == CursorMode.ValidPos)
        {
            render_.sprite = validPosSprite_;
            render_.color = new Color(1, 1, 1, 1);
        }
        else if (mode == CursorMode.Invalid)
        {
            render_.sprite = invalidSprite_;
            render_.color = new Color(1, 1, 1, 1);
        }
        else if (mode == CursorMode.Mask)
        {
            render_.sprite = maskSprite_;
            render_.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
        else if (mode == CursorMode.Mute)
        {
            render_.sprite = disabledSprite_;
            render_.color = new Color(1, 1, 1, 1);
        }
        mode_ = mode;
    }

    public void AddStatus(ItemStatus status)
    {
        if (statusList_ == null)
        {
            statusList_ = new List<ItemStatus>();
        }
        if (!statusList_.Contains(status))
        {
            statusList_.Add(status);
        }
    }

    public void SetStatusList(List<ItemStatus> status_list)
    {
        statusList_ = status_list;
    }

    public void SetItem(Item item)
    {
        item_ = item;
    }

    public override string ToString()
    {
        string str = "Cursor[" + mode_.ToString() + "]:";
        if (statusList_ != null && statusList_.Count > 0)
        {
            str += " <" + statusList_.Count + " Status>:";
            foreach (ItemStatus status in statusList_)
            {
                str += status.ToString() + ",";
            }
        }
        return str;
    }


    public CursorMode Mode
    {
        get { return mode_; }
    }

    public List<ItemStatus> StatusList
    {
        get { return statusList_; }
    }
}

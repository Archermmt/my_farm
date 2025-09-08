using UnityEngine;

public class ToolBar : Container
{
    private RectTransform rect_;
    private bool at_bottom_ = true;

    public override void Setup(Transform owner)
    {
        base.Setup(owner);
        rect_ = GetComponent<RectTransform>();
    }

    private void Update()
    {
        SwitchPosition();
    }

    private void SwitchPosition()
    {
        Vector3 playerPosition = Player.Instance.GetViewportPosition();

        if (playerPosition.y > 0.3f && !at_bottom_)
        {
            rect_.pivot = new Vector2(0.5f, 0f);
            rect_.anchorMin = new Vector2(0.5f, 0f);
            rect_.anchorMax = new Vector2(0.5f, 0f);
            rect_.anchoredPosition = new Vector2(0f, 2.5f);
            at_bottom_ = true;
        }
        else if (playerPosition.y <= 0.3f && at_bottom_)
        {
            rect_.pivot = new Vector2(0.5f, 1f);
            rect_.anchorMin = new Vector2(0.5f, 1f);
            rect_.anchorMax = new Vector2(0.5f, 1f);
            rect_.anchoredPosition = new Vector2(0f, -2.5f);
            at_bottom_ = false;
        }
    }
}

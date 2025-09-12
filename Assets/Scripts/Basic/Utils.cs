using UnityEngine;

public static class BaseUtils
{
    public static Direction GetDirection(Vector3 pos, Vector3 anchor)
    {
        float diff_width = pos.x - anchor.x;
        float diff_height = pos.y - anchor.y;
        if (Mathf.Abs(diff_height) >= Mathf.Abs(diff_width))
        {
            return diff_height < 0 ? Direction.Down : Direction.Up;
        }
        return diff_width < 0 ? Direction.Left : Direction.Right;
    }
}

public static class MouseUtils
{
    public static Vector3 MouseToWorld(Camera camera)
    {
        Vector3 mouse_pos = Input.mousePosition;
        return camera.ScreenToWorldPoint(new Vector3(mouse_pos.x, mouse_pos.y, -camera.transform.position.z));
    }

    public static Direction GetDirection(Camera camera, Vector3 anchor)
    {
        return BaseUtils.GetDirection(MouseToWorld(camera), anchor);
    }
}

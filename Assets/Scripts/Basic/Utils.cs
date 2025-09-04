using UnityEngine;

public static class CameraUtils
{
    public static Vector3 MouseToWorld(Camera camera)
    {
        Vector3 mouse_pos = Input.mousePosition;
        return camera.ScreenToWorldPoint(new Vector3(mouse_pos.x, mouse_pos.y, -camera.transform.position.z));
    }
}

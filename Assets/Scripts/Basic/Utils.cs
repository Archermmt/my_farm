using UnityEngine;


[System.Serializable]
public class Vector3Save {
    public float x, y, z;

    public Vector3Save(Vector3 position) {
        x = position.x;
        y = position.y;
        z = position.z;
    }

    public Vector3 ToVector3() {
        return new Vector3(x, y, z);
    }
}

[System.Serializable]
public class Vector2IntSave {
    public int x, y;

    public Vector2IntSave(Vector2Int coord) {
        x = coord.x;
        y = coord.y;
    }

    public Vector2Int ToVector2Int() {
        return new Vector2Int(x, y);
    }
}

public static class BaseUtils {
    public static Direction GetDirection(Vector3 pos, Vector3 anchor) {
        float diff_width = pos.x - anchor.x;
        float diff_height = pos.y - anchor.y;
        if (Mathf.Abs(diff_height) >= Mathf.Abs(diff_width)) {
            return diff_height < 0 ? Direction.Down : Direction.Up;
        }
        return diff_width < 0 ? Direction.Left : Direction.Right;
    }
}

public static class MouseUtils {
    public static Vector3 MouseToWorld(Camera camera) {
        Vector3 mouse_pos = Input.mousePosition;
        return camera.ScreenToWorldPoint(new Vector3(mouse_pos.x, mouse_pos.y, -camera.transform.position.z));
    }

    public static Direction GetDirection(Camera camera, Vector3 anchor) {
        return BaseUtils.GetDirection(MouseToWorld(camera), anchor);
    }
}

using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

[System.Serializable]
public class ScenePortSave {
    public SceneName src;
    public SceneName dst;
    public Vector3 enter;
    public Vector3 exit;

    public ScenePortSave(SceneName src, SceneName dst, Vector3 enter, Vector3 exit) {
        this.src = src;
        this.dst = dst;
        this.enter = enter;
        this.exit = exit;
    }

    public override string ToString() {
        return "From " + enter + "(" + src + ") To " + exit + "(" + dst + ")";
    }
}

public class ScenePort : MonoBehaviour {
    [SerializeField] private SceneName dstScene_;
    [SerializeField] private Vector3 dstPos_;
    [SerializeField] private List<string> portTargets_;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (portTargets_.Contains(collision.tag)) {
            float x = Mathf.Approximately(dstPos_.x, 0f) ? collision.transform.position.x : dstPos_.x;
            float y = Mathf.Approximately(dstPos_.y, 0f) ? collision.transform.position.y : dstPos_.y;
            SceneController.Instance.LoadScene(dstScene_, new Vector3(x, y, 0));
        }
    }

    public ScenePortSave ToSavable() {
        SceneName current_scene = SceneController.Instance.currentScene;
        return new ScenePortSave(current_scene, dstScene_, transform.position, dstPos_);
    }

    public SceneName dstScene { get { return dstScene_; } }
    public Vector3 dstPos { get { return dstPos_; } }
}

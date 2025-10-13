using System.Collections.Generic;
using UnityEngine;

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

    public SceneName dstScene { get { return dstScene_; } }
    public Vector3 dstPos { get { return dstPos_; } }
}

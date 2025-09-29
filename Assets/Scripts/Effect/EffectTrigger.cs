using UnityEngine;
using UnityEngine.Pool;

public class EffectTrigger : MonoBehaviour {
    [SerializeField] private GameObject prefab_;
    [SerializeField] EffectType type_;
    [SerializeField] private int capacity_ = 10;
    [SerializeField] private int maxSize_ = 100;
    private ObjectPool<GameObject> pool_;
    private int count_ = 0;

    private void Awake() {
        pool_ = new ObjectPool<GameObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, true, capacity_, maxSize_);
    }

    public GameObject GetObj(Vector3 position) {
        GameObject obj = pool_.Get();
        obj.transform.position = position;
        obj.name = type_.ToString() + "_" + count_;
        count_ += 1;
        return obj;
    }

    public void ReleaseObj(GameObject obj) {
        pool_.Release(obj);
    }

    private GameObject CreateFunc() {
        return Instantiate(prefab_, transform);
    }

    private void ActionOnGet(GameObject obj) {
        obj.SetActive(true);
    }

    private void ActionOnRelease(GameObject obj) {
        obj.SetActive(false);
    }

    private void ActionOnDestroy(GameObject obj) {
        Destroy(obj);
    }

    public EffectType type { get { return type_; } }
}

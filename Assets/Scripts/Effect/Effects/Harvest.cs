using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Harvest : MonoBehaviour {
    [SerializeField] private Image itemImg_;
    [SerializeField] private TextMeshProUGUI amount_;

    private Animator animator_;
    private void Awake() {
        animator_ = GetComponent<Animator>();
    }

    public void Trigger(Transform transform, ItemData item, int amount) {
        transform.position = transform.position;
        itemImg_.sprite = item.sprite;
        amount_.text = "X " + amount;
        animator_.SetTrigger("harvest");
    }
}

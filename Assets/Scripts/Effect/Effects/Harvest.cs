using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Harvest : MonoBehaviour {
    [SerializeField] private Image itemImg_;
    [SerializeField] private TextMeshProUGUI amount_;
    [SerializeField] private float startSeconds_ = 0.3f;
    [SerializeField] private float endSeconds_ = 1f;
    private Animator animator_;
    private Sprite emptySprite_;
    private WaitForSeconds endWait_;


    private void Awake() {
        animator_ = GetComponent<Animator>();
        emptySprite_ = itemImg_.sprite;
        endWait_ = new WaitForSeconds(endSeconds_);
    }

    public IEnumerator Trigger(Vector3 position, ItemData item, int amount, int offset = 0) {
        itemImg_.sprite = emptySprite_;
        amount_.text = "";
        yield return new WaitForSeconds(offset * startSeconds_ + Random.Range(0.0f, startSeconds_));
        transform.position = position;
        itemImg_.sprite = item.sprite;
        amount_.text = "X " + amount;
        animator_.SetTrigger("harvest");
        yield return endWait_;
    }
}

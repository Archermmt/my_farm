using UnityEngine;

public class ObjectMask : MonoBehaviour
{
    [SerializeField] private string triggerBy_;
    [SerializeField] private Transform maskObj_;
    [SerializeField] private int dst_order_ = 9;
    private Renderer render_;
    private int src_order_;

    private void Awake()
    {
        render_ = maskObj_.GetComponent<Renderer>();
        src_order_ = render_.sortingOrder;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (render_ != null && collision.tag == triggerBy_)
        {
            render_.sortingOrder = dst_order_;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (render_ != null && collision.tag == triggerBy_)
        {
            render_.sortingOrder = src_order_;
        }
    }
}

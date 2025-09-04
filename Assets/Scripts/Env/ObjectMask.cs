using UnityEngine;

public class ObjectMask : MonoBehaviour
{
    public string triggerBy;
    public Transform maskObj;
    public int dst_order = 9;
    private Renderer render_;
    private int src_order_;

    private void Awake()
    {
        render_ = maskObj.GetComponent<Renderer>();
        src_order_ = render_.sortingOrder;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (render_ != null && collision.tag == triggerBy)
        {
            render_.sortingOrder = dst_order;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (render_ != null && collision.tag == triggerBy)
        {
            render_.sortingOrder = src_order_;
        }
    }
}

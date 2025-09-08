using TMPro;
using UnityEngine;

public class ItemDescriber : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title_ = null;
    [SerializeField] private TextMeshProUGUI describe_ = null;
    [SerializeField] private TextMeshProUGUI detail_ = null;

    public void DescribeItem(ItemData item_data)
    {
        title_.text = item_data.name;
        describe_.text = "[" + item_data.type.ToString() + "] " + item_data.description;
        detail_.text += "Price: " + item_data.price.ToString() + "/" + item_data.value.ToString() + "\n";
    }
}

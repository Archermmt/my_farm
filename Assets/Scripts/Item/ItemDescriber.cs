using TMPro;
using UnityEngine;

public class ItemDescriber : MonoBehaviour
{
    public TextMeshProUGUI title = null;
    public TextMeshProUGUI describe = null;
    public TextMeshProUGUI detail = null;

    public void DescribeItem(ItemData item_data)
    {
        title.text = item_data.name;
        describe.text = "[" + item_data.type.ToString() + "] " + item_data.description;
        detail.text = "Radius: " + item_data.useRadius.ToString() + "\n";
        detail.text += "Price: " + item_data.price.ToString() + "/" + item_data.value.ToString() + "\n";
    }
}

using UnityEngine;

public class PlayerInventory : BaseInventory {
    private bool backpackOpening_ = false;
    public int AddItem(ItemData item_data, int amount = 1) {
        int left = GetContainer(ContainerType.ToolBar).AddItem(item_data, amount);
        if (left > 0) {
            left = GetContainer(ContainerType.Backpack).AddItem(item_data, left);
        }
        return left;
    }

    public int RemoveItem(ItemData item_data, int amount) {
        int left = GetContainer(ContainerType.Backpack).RemoveItem(item_data, amount);
        if (left > 0) {
            left = GetContainer(ContainerType.ToolBar).RemoveItem(item_data, left);
        }
        return left;
    }

    public void OpenBackpack() {
        if (!backpackOpening_) {
            GetContainer(ContainerType.ToolBar).gameObject.SetActive(false);
            GetContainer(ContainerType.Pocket).gameObject.SetActive(true);
            GetContainer(ContainerType.Pocket).CopyFrom(GetContainer(ContainerType.ToolBar));
            GetContainer(ContainerType.Backpack).gameObject.SetActive(true);
            GetContainer(ContainerType.Backpack).transform.parent.gameObject.SetActive(true);
            backpackOpening_ = true;
        }
    }
    public void CloseBackpack() {
        if (backpackOpening_) {
            GetContainer(ContainerType.ToolBar).gameObject.SetActive(true);
            GetContainer(ContainerType.ToolBar).CopyFrom(GetContainer(ContainerType.Pocket));
            GetContainer(ContainerType.Pocket).gameObject.SetActive(false);
            GetContainer(ContainerType.Backpack).gameObject.SetActive(false);
            GetContainer(ContainerType.Backpack).transform.parent.gameObject.SetActive(false);
            backpackOpening_ = false;
        }
    }

    public bool backpackOpening { get { return backpackOpening_; } }
}

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

    public void UpdateContainer(ContainerType container_type, bool sort, bool deselect) {
        Container container = GetContainer(container_type);
        container.UpdateSlots(sort, deselect);
        if (container_type == ContainerType.Pocket || container_type == ContainerType.ToolBar) {
            FillContainer(container_type);
        }
    }

    public void OpenBackpack() {
        if (!backpackOpening_) {
            GetContainer(ContainerType.ToolBar).Close();
            GetContainer(ContainerType.Pocket).Open();
            GetContainer(ContainerType.Backpack).Open();
            GetContainer(ContainerType.Pocket).CopyFrom(GetContainer(ContainerType.ToolBar));
            GetContainer(ContainerType.Backpack).transform.parent.gameObject.SetActive(true);
            backpackOpening_ = true;
        }
    }

    public void CloseBackpack() {
        if (backpackOpening_) {
            GetContainer(ContainerType.ToolBar).Open();
            GetContainer(ContainerType.ToolBar).CopyFrom(GetContainer(ContainerType.Pocket));
            GetContainer(ContainerType.Pocket).Close();
            GetContainer(ContainerType.Backpack).Close();
            GetContainer(ContainerType.Backpack).transform.parent.gameObject.SetActive(false);
            backpackOpening_ = false;
        }
    }

    private void FillContainer(ContainerType container_type) {
        Container container = GetContainer(container_type);
        Container backpack = GetContainer(ContainerType.Backpack);
        Slot empty = container.FindEmptySlot();
        Slot non_empty = backpack.FindNonEmptySlot();
        while (empty != null && non_empty != null) {
            empty.Swap(non_empty);
            empty = container.FindEmptySlot();
            non_empty = backpack.FindNonEmptySlot();
        }
    }

    public bool backpackOpening { get { return backpackOpening_; } }
}

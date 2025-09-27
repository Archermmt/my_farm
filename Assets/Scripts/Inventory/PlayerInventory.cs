public class PlayerInventory : BaseInventory {
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
}

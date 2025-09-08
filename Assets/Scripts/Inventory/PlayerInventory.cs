public class PlayerInventory : BaseInventory
{
    public void AddItem(ItemData item_data)
    {
        if (!GetContainer(ContainerType.ToolBar).AddItem(item_data))
        {
            GetContainer(ContainerType.Backpack).AddItem(item_data);
        }
    }
}

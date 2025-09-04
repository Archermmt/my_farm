public class PlayerInventory : BaseInventory
{
    public void AddItem(ItemData item)
    {
        if (!GetContainer(ContainerType.ToolBar).AddItem(item))
        {
            GetContainer(ContainerType.Backpack).AddItem(item);
        }
    }

    public Slot FindSelectSlot()
    {
        return GetContainer(ContainerType.ToolBar).FindSelectedSlot();
    }

    private void UpdateInventory(bool sort, bool deselect)
    {
        UpdateContainers(sort, deselect);
    }

    private void OnEnable()
    {
        EventHandler.UpdateInventoryEvent += UpdateInventory;
    }

    private void OnDisable()
    {
        EventHandler.UpdateInventoryEvent -= UpdateInventory;
    }
}

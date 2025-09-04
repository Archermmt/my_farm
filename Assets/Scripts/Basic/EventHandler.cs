using System;

public static class EventHandler
{
    public static event Action<bool, bool> UpdateInventoryEvent;

    public static void CallUpdateInventory(bool sort, bool deselect)
    {
        if (UpdateInventoryEvent != null)
        {
            UpdateInventoryEvent(sort, deselect);
        }
    }

    public static event Action UpdateHandsEvent;

    public static void CallUpdateHands()
    {
        if (UpdateHandsEvent != null)
        {
            UpdateHandsEvent();
        }
    }
}
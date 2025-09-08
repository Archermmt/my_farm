using System;
using UnityEngine;

public static class EventHandler
{
    public static event Action<Transform, ContainerType, bool, bool> UpdateInventoryEvent;

    public static void CallUpdateInventory(Transform owner, ContainerType type, bool sort, bool deselect)
    {
        if (UpdateInventoryEvent != null)
        {
            UpdateInventoryEvent(owner, type, sort, deselect);
        }
    }

    public static event Action<Transform, ContainerType> UpdateHandsEvent;

    public static void CallUpdateHands(Transform owner, ContainerType type)
    {
        if (UpdateHandsEvent != null)
        {
            UpdateHandsEvent(owner, type);
        }
    }
}
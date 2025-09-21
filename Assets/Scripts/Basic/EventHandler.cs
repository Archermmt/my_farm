using System;
using UnityEngine;

public static class EventHandler {
    public static event Action<Transform, ContainerType, bool, bool> UpdateInventoryEvent;

    public static void CallUpdateInventory(Transform owner, ContainerType type, bool sort, bool deselect) {
        if (UpdateInventoryEvent != null) {
            UpdateInventoryEvent(owner, type, sort, deselect);
        }
    }

    public static event Action<Transform, ContainerType> UpdateHandsEvent;

    public static void CallUpdateHands(Transform owner, ContainerType type) {
        if (UpdateHandsEvent != null) {
            UpdateHandsEvent(owner, type);
        }
    }

    public static event Action<TimeType, int, Season, int, int, int, int, int, int> UpdateTimeEvent;

    public static void CallUpdateTime(TimeType time_type, int year, Season season, int month, int week, int day, int hour, int minute, int second) {
        if (UpdateTimeEvent != null) {
            UpdateTimeEvent(time_type, year, season, month, week, day, hour, minute, second);
        }
    }
}
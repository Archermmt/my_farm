using System;

public static class EventHandler {

    public static event Action<SceneName> BeforeSceneUnloadEvent;

    public static void CallBeforeSceneUnload(SceneName scene_name) {
        if (BeforeSceneUnloadEvent != null) {
            BeforeSceneUnloadEvent(scene_name);
        }
    }

    public static event Action<SceneName> AfterSceneLoadEvent;

    public static void CallAfterSceneLoad(SceneName scene_name) {
        if (AfterSceneLoadEvent != null) {
            AfterSceneLoadEvent(scene_name);
        }
    }

    public static event Action<string, ContainerType, bool, bool> UpdateInventoryEvent;

    public static void CallUpdateInventory(string owner, ContainerType type, bool sort, bool deselect) {
        if (UpdateInventoryEvent != null) {
            UpdateInventoryEvent(owner, type, sort, deselect);
        }
    }

    public static event Action<string, ItemData, int> AddInventoryItemEvent;

    public static void CallAddInventoryItem(string owner, ItemData item, int amount) {
        if (AddInventoryItemEvent != null) {
            AddInventoryItemEvent(owner, item, amount);
        }
    }

    public static event Action<string, ContainerType> UpdateHandsEvent;

    public static void CallUpdateHands(string owner, ContainerType type) {
        if (UpdateHandsEvent != null) {
            UpdateHandsEvent(owner, type);
        }
    }

    public static event Action<TimeType, TimeData, int> UpdateTimeEvent;

    public static void CallUpdateTime(TimeType time_type, TimeData time, int delta) {
        if (UpdateTimeEvent != null) {
            UpdateTimeEvent(time_type, time, delta);
        }
    }
}
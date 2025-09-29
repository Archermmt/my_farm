using System;
using UnityEngine;

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

    public static event Action<TimeType, TimeData, int> UpdateTimeEvent;

    public static void CallUpdateTime(TimeType time_type, TimeData time, int delta) {
        if (UpdateTimeEvent != null) {
            UpdateTimeEvent(time_type, time, delta);
        }
    }
}
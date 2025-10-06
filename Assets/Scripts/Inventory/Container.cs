using System;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour {
    [SerializeField] private ContainerType containerType_;
    [SerializeField] private int max_capacity_;
    [SerializeField] private int start_capacity_;
    private List<Slot> slots_;
    private int current_;

    private void OnEnable() {
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnload;
    }

    private void OnDisable() {
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnload;
    }

    public virtual void Setup(string owner) {
        slots_ = new List<Slot>();
        GameObject slot_prefab = Resources.Load<GameObject>("Prefab/Inventory/Slot");
        for (int i = 0; i < max_capacity_; i++) {
            Slot slot = Instantiate(slot_prefab, gameObject.transform).GetComponent<Slot>();
            slot.gameObject.name = "Slot_" + i;
            slot.Setup(owner, containerType_);
            slot.Disable();
            slots_.Add(slot);
        }
        current_ = 0;
        IncreaseCapacity(start_capacity_);
    }

    public void CopyFrom(Container other) {
        int amount = Math.Min(current_, other.current);
        for (int i = 0; i < amount; i++) {
            if (other.GetSlot(i).current > 0) {
                slots_[i].CopyFrom(other.GetSlot(i));
            }
        }
    }

    public int AddItem(ItemData item_data, int amount = 1) {
        Slot slot = FindSlotToAdd(item_data);
        if (slot == null) {
            return amount;
        }
        int left = amount;
        if (slot.current == 0) {
            slot.SetItem(item_data, 0);
        }
        while (slot.GetSurplus(item_data) < left) {
            slot.IncreaseAmount(slot.GetSurplus(item_data));
            left -= slot.GetSurplus(item_data);
            slot = FindSlotToAdd(item_data);
            if (slot == null) {
                return left;
            }
        }
        slot.IncreaseAmount(left);
        return 0;
    }

    public int RemoveItem(ItemData item_data, int amount = 1) {
        Slot slot = FindSlotToRemove(item_data);
        if (slot == null) {
            return amount;
        }
        int left = amount;
        while (slot.current < left) {
            slot.DecreaseAmount(slot.current);
            left -= slot.current;
            slot = FindSlotToRemove(item_data);
            if (slot == null) {
                return left;
            }
        }
        slot.DecreaseAmount(left);
        return 0;
    }

    public Slot FindSlotToAdd(ItemData item_data, int amount = 1) {
        foreach (Slot slot in slots_) {
            if (slot.GetSurplus(item_data) >= amount) {
                return slot;
            }
        }
        return null;
    }

    public Slot FindSlotToRemove(ItemData item_data, int amount = 1) {
        foreach (Slot slot in slots_) {
            if (slot.current >= amount && slot.itemMeta.name == item_data.name) {
                return slot;
            }
        }
        return null;
    }

    public Slot FindSelectedSlot() {
        foreach (Slot slot in slots_) {
            if (slot.selected) {
                return slot;
            }
        }
        return null;
    }

    public void UpdateSlots(bool sort, bool deselect) {
        if (sort) {
            List<Slot> used_slots = new List<Slot>();
            for (int i = 0; i < current_; i++) {
                if (slots_[i].current > 0) {
                    used_slots.Add(slots_[i]);
                }
            }
            for (int i = 0; i < current_; i++) {
                if (i < used_slots.Count) {
                    slots_[i].CopyFrom(used_slots[i]);
                } else {
                    slots_[i].Empty();
                }
            }
        } else if (deselect) {
            for (int i = 0; i < current_; i++) {
                slots_[i].Deselect();
            }
        }
    }

    protected void IncreaseCapacity(int capacity) {
        for (int i = 0; i < capacity; i++) {
            slots_[current_].Enable();
            current_++;
        }
    }

    protected void DecreaseCapacity(int capacity) {
        for (int i = 0; i < capacity; i++) {
            if (current_ < 1) {
                break;
            }
            slots_[current_].Disable();
            current_--;
        }
    }

    public Slot GetSlot(int idx) {
        return slots_[idx];
    }

    public void Deselect() {
        foreach (Slot slot in slots_) {
            slot.Deselect();
        }
    }

    public void Open() {
        Deselect();
        gameObject.SetActive(true);
    }

    public void Close() {
        Deselect();
        gameObject.SetActive(false);
    }

    private void BeforeSceneUnload(SceneName scene_name) {
        Deselect();
    }

    public int current { get { return current_; } }

    public ContainerType containerType { get { return containerType_; } }
}

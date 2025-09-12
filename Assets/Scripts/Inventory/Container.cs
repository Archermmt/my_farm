using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    [SerializeField] private ContainerType containerType_;
    [SerializeField] private int max_capacity_;
    [SerializeField] private int start_capacity_;
    private Transform transform_;
    private List<Slot> slots_;
    private GameObject slot_prefab_;
    private int current_;

    public virtual void Setup(Transform owner)
    {
        slots_ = new List<Slot>();
        GameObject slot_prefab = Resources.Load<GameObject>("Prefab/Slot");
        for (int i = 0; i < max_capacity_; i++)
        {
            Slot slot = Instantiate(slot_prefab, gameObject.transform).GetComponent<Slot>();
            slot.Setup(owner, containerType_);
            slot.Disable();
            slots_.Add(slot);
        }
        current_ = 0;
        IncreaseCapacity(start_capacity_);
    }


    public bool AddItem(ItemData item_data)
    {
        Slot slot = FindSlot(item_data);
        if (slot == null)
        {
            return false;
        }
        if (slot.current == 0)
        {
            slot.SetItem(item_data);
        }
        else
        {
            slot.IncreaseAmount();
        }
        return true;
    }

    public Slot FindSlot(ItemData item_data, int amount = 1)
    {
        foreach (Slot slot in slots_)
        {
            if (slot.GetSurplus(item_data) >= amount)
            {
                return slot;
            }
        }
        return null;
    }

    public Slot FindSelectedSlot()
    {
        foreach (Slot slot in slots_)
        {
            if (slot.selected)
            {
                return slot;
            }
        }
        return null;
    }

    public void UpdateSlots(bool sort, bool deselect)
    {
        if (sort)
        {
            List<Slot> used_slots = new List<Slot>();
            for (int i = 0; i < current_; i++)
            {
                if (slots_[i].current > 0)
                {
                    used_slots.Add(slots_[i]);
                }
            }
            for (int i = 0; i < current_; i++)
            {
                if (i < used_slots.Count)
                {
                    slots_[i].CopyFrom(used_slots[i]);
                }
                else
                {
                    slots_[i].Empty();
                }
            }
        }
        else if (deselect)
        {
            for (int i = 0; i < current_; i++)
            {
                slots_[i].Deselect();
            }
        }
    }

    protected void IncreaseCapacity(int capacity)
    {
        for (int i = 0; i < capacity; i++)
        {
            slots_[current_].Enable();
            current_++;
        }
    }

    protected void DecreaseCapacity(int capacity)
    {
        for (int i = 0; i < capacity; i++)
        {
            if (current_ < 1)
            {
                break;
            }
            slots_[current_].Disable();
            current_--;
        }
    }

    public Slot GetSlot(int idx)
    {
        return slots_[idx];
    }

    public int current { get { return current_; } }

    public ContainerType containerType { get { return containerType_; } }
}

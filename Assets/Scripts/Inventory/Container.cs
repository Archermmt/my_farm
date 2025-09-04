using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public ContainerType containerType;
    public int max_capacity;
    public int start_capacity;
    private Transform transform_;
    private List<Slot> slots_;
    private GameObject slot_prefab_;
    private int current_;

    public bool AddItem(ItemData item)
    {
        Slot slot = FindSlot(item);
        if (slot == null)
        {
            return false;
        }
        if (slot.Current == 0)
        {
            slot.SetItem(item);
        }
        else
        {
            slot.IncreaseAmount();
        }
        return true;
    }

    public Slot FindSlot(ItemData item, int amount = 1)
    {
        foreach (Slot slot in slots_)
        {
            if (slot.GetSurplus(item) >= amount)
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
            if (slot.Selected)
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
                if (slots_[i].Current > 0)
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

    protected virtual void Awake()
    {
        transform_ = gameObject.transform;
        slot_prefab_ = Resources.Load<GameObject>("Prefab/Slot");
        Setup();
    }

    protected void Setup()
    {
        slots_ = new List<Slot>();
        for (int i = 0; i < max_capacity; i++)
        {
            Slot slot = Instantiate(slot_prefab_, transform_).GetComponent<Slot>();
            slot.Disable();
            slots_.Add(slot);
        }
        current_ = 0;
        IncreaseCapacity(start_capacity);
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

    public int Current
    {
        get { return current_; }
    }
}

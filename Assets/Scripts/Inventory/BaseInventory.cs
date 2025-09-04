using System.Collections.Generic;
using UnityEngine;

public class BaseInventory : MonoBehaviour
{
    public Container[] containers;
    private Dictionary<ContainerType, Container> containersMap_;

    public Container GetContainer(ContainerType type)
    {
        return containersMap_[type];
    }

    protected void UpdateContainers(bool sort, bool deselect, ContainerType[] types = null)
    {
        if (types == null)
        {
            foreach (Container container in containersMap_.Values)
            {
                container.UpdateSlots(sort, deselect);
            }
        }
        else
        {
            foreach (ContainerType type in types)
            {
                if (containersMap_.ContainsKey(type))
                {
                    containersMap_[type].UpdateSlots(sort, deselect);
                }
            }
        }
    }

    private void Awake()
    {
        containersMap_ = new Dictionary<ContainerType, Container>();
        foreach (Container container in containers)
        {
            containersMap_.Add(container.containerType, container);
        }
    }
}

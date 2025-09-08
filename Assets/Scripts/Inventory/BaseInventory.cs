using System.Collections.Generic;
using UnityEngine;

public class BaseInventory : MonoBehaviour
{
    [SerializeField] private Container[] containers_;
    private Dictionary<ContainerType, Container> containersMap_;

    public void Setup(Transform owner)
    {
        foreach (Container container in containersMap_.Values)
        {
            container.Setup(owner);
        }
    }

    public Container GetContainer(ContainerType type)
    {
        return containersMap_[type];
    }

    public void UpdateContainers(bool sort, bool deselect, ContainerType[] types = null)
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
        foreach (Container container in containers_)
        {
            containersMap_.Add(container.Type, container);
        }
    }
}

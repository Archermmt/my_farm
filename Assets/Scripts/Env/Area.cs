using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField] private List<AreaTag> areaTags_;

    public void AddTag(AreaTag tag)
    {
        if (areaTags_ == null)
        {
            areaTags_ = new List<AreaTag>();
        }
        areaTags_.Add(tag);
    }

    public bool HasTag(AreaTag tag)
    {
        if (areaTags_ == null)
        {
            return false;
        }
        return areaTags_.Contains(tag);
    }

    public List<AreaTag> areaTags
    {
        get { return areaTags_; }
    }
}

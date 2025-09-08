using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField] private AreaTag[] areaTags_;

    public AreaTag[] AreaTags
    {
        get { return areaTags_; }
    }
}

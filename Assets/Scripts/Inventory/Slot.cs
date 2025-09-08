using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
  [SerializeField] private Image highlight_;
  [SerializeField] private Image mask_;
  [SerializeField] private Image itemImg_;
  [SerializeField] private TextMeshProUGUI number_;
  [SerializeField] private int capacity_ = 99;
  private ItemData itemMeta_;
  private GameObject dragging_;
  private Sprite emptySprite_;
  private Transform owner_;
  private ContainerType holderType_;
  private int current_ = 0;
  private bool selected_ = false;

  public void Setup(Transform owner, ContainerType container_type)
  {
    owner_ = owner;
    holderType_ = container_type;
  }

  public int GetSurplus(ItemData item_data = null)
  {
    if (current_ < 0)
    {
      return 0;
    }
    if (current_ == 0)
    {
      return capacity_;
    }
    if (item_data == null)
    {
      return current_ < capacity_ ? capacity_ - current_ : 0;
    }
    if (current_ > 0 && current_ < capacity_ && item_data.name == itemMeta_.name)
    {
      return capacity_ - current_;
    }
    return 0;
  }

  public void SetItem(ItemData item_data, int amount = 1)
  {
    itemMeta_ = item_data;
    current_ = amount;
    itemImg_.sprite = item_data.sprite;
    number_.text = current_.ToString();
  }

  public void Swap(Slot other)
  {
    Transform otherOwner = other.Owner;
    ContainerType otherHolderType = other.HolderType;
    other.Setup(owner_, holderType_);
    Setup(otherOwner, otherHolderType);
    if (other.Current == 0)
    {
      other.SetItem(itemMeta_, current_);
      Empty();
      UpdateContainer(true, true);
      if (other.Owner != owner_ || other.HolderType != holderType_)
      {
        other.UpdateContainer(true, true);
      }
    }
    else if (other.Current > 0)
    {
      ItemData otherItemMeta = other.ItemMeta;
      int otherCurrent = other.Current;
      other.SetItem(itemMeta_, current_);
      SetItem(otherItemMeta, otherCurrent);
    }
  }

  public void CopyFrom(Slot other)
  {
    Setup(other.Owner, other.HolderType);
    SetItem(other.ItemMeta, other.Current);
  }

  public int IncreaseAmount(int amount = 1)
  {
    current_ = current_ + amount > capacity_ ? capacity_ : current_ + amount;
    number_.text = current_.ToString();
    return GetSurplus();
  }

  public int DecreaseAmount(int amount = 1)
  {
    current_ = current_ - amount < 0 ? 0 : current_ - amount;
    number_.text = current_.ToString();
    if (current_ == 0)
    {
      Deselect();
      Empty();
      UpdateContainer(true, true);
    }
    return GetSurplus();
  }

  public void Select()
  {
    if (current_ >= 0 && !selected_)
    {
      selected_ = true;
      highlight_.color = new Color(1, 1, 1, 1);
      EventHandler.CallUpdateHands(owner_, holderType_);
    }
  }

  public void Deselect()
  {
    if (current_ >= 0 && selected_)
    {
      selected_ = false;
      highlight_.color = new Color(0, 0, 0, 0);
      EventHandler.CallUpdateHands(owner_, holderType_);
    }
  }

  public void Enable()
  {
    mask_.color = new Color(0, 0, 0, 0);
    current_ = 0;
    number_.text = "";
  }

  public void Disable()
  {
    mask_.color = new Color(1, 1, 1, 1);
    current_ = -1;
    number_.text = "";
  }

  public void Empty()
  {
    current_ = 0;
    selected_ = false;
    highlight_.color = new Color(0, 0, 0, 0);
    itemImg_.sprite = emptySprite_;
    number_.text = "";
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    if (itemMeta_ != null)
    {
      Player.Instance.Freeze();
      GameObject prefab = Resources.Load<GameObject>("Prefab/Item/Dragged");
      dragging_ = Instantiate(prefab, transform);
      dragging_.GetComponent<Image>().sprite = itemMeta_.sprite;
      Select();
    }
  }

  public void OnDrag(PointerEventData eventData)
  {
    if (dragging_ != null)
    {
      dragging_.transform.position = Input.mousePosition;
    }
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    Destroy(dragging_);
    if (eventData.pointerCurrentRaycast.gameObject != null && eventData.pointerCurrentRaycast.gameObject.GetComponent<Slot>() != null)
    {
      Slot dst_slot = eventData.pointerCurrentRaycast.gameObject.GetComponent<Slot>();
      Swap(dst_slot);
    }
    Deselect();
    Player.Instance.Unfreeze();
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    EnvManager.Instance.Freeze();
    if (current_ > 0)
    {
      ItemManager.Instance.CommentItem(itemMeta_, transform);
    }
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    EnvManager.Instance.Unfreeze();
    ItemManager.Instance.UnCommentItem();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (eventData.button == PointerEventData.InputButton.Left)
    {
      if (selected_)
      {
        Deselect();
      }
      else if (current_ > 0)
      {
        UpdateContainer(false, true);
        Select();
      }
    }
  }

  private void Awake()
  {
    emptySprite_ = itemImg_.sprite;
  }

  private void UpdateContainer(bool sort, bool deselect)
  {
    EventHandler.CallUpdateInventory(owner_, holderType_, sort, deselect);
  }

  public ItemData ItemMeta
  {
    get { return itemMeta_; }
  }

  public int Current
  {
    get { return current_; }
  }

  public bool Selected
  {
    get { return selected_; }
  }

  public Transform Owner
  {
    get { return owner_; }
  }

  public ContainerType HolderType
  {
    get { return holderType_; }
  }
}

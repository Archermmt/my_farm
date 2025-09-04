using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
  public Image highlight;
  public Image mask;
  public Image itemImg;
  public TextMeshProUGUI number;
  public int capacity = 99;
  private ItemData itemData_;
  private GameObject dragging_;
  private Sprite emptySprite_;
  private int current_ = 0;
  private bool selected_ = false;

  public int GetSurplus(ItemData item_data = null)
  {
    if (current_ < 0)
    {
      return 0;
    }
    if (current_ == 0)
    {
      return capacity;
    }
    if (item_data == null)
    {
      return current_ < capacity ? capacity - current_ : 0;
    }
    if (current_ > 0 && current_ < capacity && item_data.name == itemData_.name)
    {
      return capacity - current_;
    }
    return 0;
  }

  public void SetItem(ItemData item_data, int amount = 1)
  {
    itemData_ = item_data;
    current_ = amount;
    itemImg.sprite = item_data.sprite;
    number.text = current_.ToString();
  }

  public void Swap(Slot other)
  {
    if (other.Current == 0)
    {
      other.SetItem(itemData_, current_);
      Empty();
      EventHandler.CallUpdateInventory(true, true);
    }
    else if (other.Current > 0)
    {
      ItemData otherItem = other.ItemData;
      int amount = other.Current;
      other.SetItem(itemData_, current_);
      SetItem(otherItem, amount);
    }
  }

  public void CopyFrom(Slot other)
  {
    SetItem(other.ItemData, other.Current);
  }

  public int IncreaseAmount(int amount = 1)
  {
    current_ = current_ + amount > capacity ? capacity : current_ + amount;
    number.text = current_.ToString();
    return GetSurplus();
  }

  public int DecreaseAmount(int amount = 1)
  {
    current_ = current_ - amount < 0 ? 0 : current_ - amount;
    number.text = current_.ToString();
    if (current_ == 0)
    {
      Deselect();
      Empty();
      EventHandler.CallUpdateInventory(true, true);
    }
    return GetSurplus();
  }

  public void Select()
  {
    if (current_ >= 0 && !selected_)
    {
      selected_ = true;
      highlight.color = new Color(1, 1, 1, 1);
      EventHandler.CallUpdateHands();
    }
  }

  public void Deselect()
  {
    if (current_ >= 0 && selected_)
    {
      selected_ = false;
      highlight.color = new Color(0, 0, 0, 0);
      EventHandler.CallUpdateHands();
    }
  }

  public void Enable()
  {
    mask.color = new Color(0, 0, 0, 0);
    current_ = 0;
    number.text = "";
  }

  public void Disable()
  {
    mask.color = new Color(1, 1, 1, 1);
    current_ = -1;
    number.text = "";
  }

  public void Empty()
  {
    current_ = 0;
    selected_ = false;
    highlight.color = new Color(0, 0, 0, 0);
    itemImg.sprite = emptySprite_;
    number.text = "";
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    if (itemData_ != null)
    {
      Player.Instance.Freeze();
      GameObject prefab = Resources.Load<GameObject>("Prefab/Item/Dragged");
      dragging_ = Instantiate(prefab, transform);
      dragging_.GetComponent<Image>().sprite = itemData_.sprite;
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
    else if (itemData_ != null && itemData_.dropable)
    {
      DropItemAtMouse();
    }
    Deselect();
    Player.Instance.Unfreeze();
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (current_ > 0)
    {
      ItemManager.Instance.CommentItem(itemData_, transform);
    }
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    ItemManager.Instance.UnCommentItem();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (eventData.button == PointerEventData.InputButton.Left)
    {
      EventHandler.CallUpdateInventory(false, true);
      if (selected_)
      {
        Deselect();
      }
      else if (current_ > 0)
      {
        Select();
      }
    }
  }

  public Item DropItemAtMouse()
  {
    Item item = ItemManager.Instance.CreateItemAtMouse(itemData_);
    EnvManager.Instance.AddItem(item);
    DecreaseAmount();
    return item;
  }

  private void Awake()
  {
    emptySprite_ = itemImg.sprite;
  }

  public ItemData ItemData
  {
    get { return itemData_; }
  }

  public int Current
  {
    get { return current_; }
  }

  public bool Selected
  {
    get { return selected_; }
  }
}

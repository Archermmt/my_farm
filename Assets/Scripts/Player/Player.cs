using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player>
{
    [SerializeField] private float runSpeed_ = 5f;
    [SerializeField] private float walkSpeed_ = 3f;
    [SerializeField] private PlayerInventory inventory_;
    // basic
    private bool freezed_ = false;
    private Camera camera_;
    private Rigidbody2D rigidBody_;
    // movement
    private Direction direction_ = Direction.Down;
    // hands
    private Transform hands_;
    private SpriteRenderer handsRender_;
    private KeyValuePair<Item, Slot> carried_;
    private Dictionary<string, Item> carriedCache_;
    // animation
    private Action action_ = Action.Idle;
    private Animator[] animators_;
    private AnimationSwapper[] animationSwappers_;
    private WaitForSeconds useToolPause_;

    protected override void Awake()
    {
        base.Awake();
        rigidBody_ = GetComponent<Rigidbody2D>();
        animators_ = GetComponentsInChildren<Animator>();
        useToolPause_ = new WaitForSeconds(Settings.useToolPause);
        inventory_.Setup(transform);
        animationSwappers_ = GetComponentsInChildren<AnimationSwapper>();
        hands_ = transform.Find("Hands");
        handsRender_ = hands_.GetComponent<SpriteRenderer>();
        camera_ = Camera.main;
        carried_ = new KeyValuePair<Item, Slot>(null, null);
        carriedCache_ = new Dictionary<string, Item>();
    }

    private void OnEnable()
    {
        UpdateAnimators();
        EventHandler.UpdateHandsEvent += UpdateHands;
        EventHandler.UpdateInventoryEvent += UpdateInventory;
    }

    private void OnDisable()
    {
        EventHandler.UpdateHandsEvent -= UpdateHands;
        EventHandler.UpdateInventoryEvent -= UpdateInventory;
    }

    private void Update()
    {
        if (carried_.Key != null)
        {
            Action action = Action.Idle;
            if (Input.GetMouseButtonDown(0))
            {
                action = Action.DropItem;
            }
            else if (Input.GetMouseButtonDown(1))
            {
                action = Action.HoldItem;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                action = Action.UseItem;
            }
            //ProcessCarried(action);
        }
        InputTest();
    }

    private void FixedUpdate()
    {
        if (!freezed_)
        {
            ProcessMovement();
        }
    }

    private void ProcessMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 movement = new Vector2(horizontal, vertical).normalized;
        float speed;
        if (movement == Vector2.zero)
        {
            action_ = Action.Idle;
            speed = 0;
        }
        else
        {
            // set speed
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                action_ = Action.Walk;
                speed = walkSpeed_;
            }
            else
            {
                action_ = Action.Run;
                speed = runSpeed_;
            }
            // set direction
            if (movement.x > 0)
                direction_ = Direction.Right;
            else if (movement.x < 0)
                direction_ = Direction.Left;
            else if (movement.y < 0)
                direction_ = Direction.Down;
            else
                direction_ = Direction.Up;
        }
        UpdateAnimators();
        rigidBody_.MovePosition(rigidBody_.position + movement * speed * Time.deltaTime);
    }

    private void ProcessCarried(Action action)
    {
        List<Cursor> cursors = FieldManager.Instance.CheckItem(carried_.Key, transform.position, MouseUtils.MouseToWorld(camera_));
        if (cursors.Count > 0)
        {
            if (action == Action.DropItem && carried_.Key.HasStatus(ItemStatus.Dropable))
            {
                int drop_amount = FieldManager.Instance.DropItem(carried_.Key, cursors);
                carried_.Value.DecreaseAmount(drop_amount);
            }
            else if (action == Action.HoldItem && (carried_.Key.HasStatus(ItemStatus.GridUsable) || carried_.Key.HasStatus(ItemStatus.ItemUsable)))
            {
                Tool tool = (Tool)carried_.Key;
                direction_ = MouseUtils.GetDirection(camera_, transform.position);
                action_ = Action.Idle;
                tool.Hold(direction_);
                SwapAnimations(tool.animationTag);
                foreach (Animator animator in animators_)
                {
                    animator.SetInteger("direction", (int)direction_);
                    animator.SetInteger("action", (int)action_);
                }
                Freeze();
            }
            else if (action == Action.UseItem && carried_.Key.HasStatus(ItemStatus.Holding))
            {
                StartCoroutine(UseToolRoutine((Tool)carried_.Key, cursors));
            }
        }
    }

    private IEnumerator UseToolRoutine(Tool tool, List<Cursor> cursors)
    {
        tool.Unhold();
        int use_amount = FieldManager.Instance.UseItem(tool, cursors);
        foreach (Animator animator in animators_)
        {
            Debug.Log("[TMINFO] set trigger for " + animator);
            animator.SetTrigger("useTool");
        }
        yield return useToolPause_;
        /*
        RestoreAnimations(tool.animationTag);
        foreach (Animator animator in animators_)
        {
            animator.SetInteger("direction", (int)direction_);
            animator.SetInteger("action", (int)action_);
        }
        */
        Unfreeze();
        carried_.Value.DecreaseAmount(use_amount);
    }


    private void UpdateAnimators()
    {
        foreach (Animator animator in animators_)
        {
            animator.SetInteger("direction", (int)direction_);
            animator.SetInteger("action", (int)action_);
        }
    }

    private void SwapAnimations(AnimationTag animation_tag)
    {
        foreach (AnimationSwapper swapper in animationSwappers_)
        {
            swapper.Swap(animation_tag);
        }
    }

    private void RestoreAnimations(AnimationTag animation_tag)
    {
        foreach (AnimationSwapper swapper in animationSwappers_)
        {
            swapper.Restore(animation_tag);
        }
    }

    private void UpdateHands(Transform owner, ContainerType container_type)
    {
        if (owner == transform && container_type == ContainerType.ToolBar)
        {
            if (carried_.Key != null)
            {
                carried_.Key.gameObject.SetActive(false);
                carried_.Key.transform.parent = hands_.Find("Cache");
            }
            Slot slot = inventory_.GetContainer(container_type).FindSelectedSlot();
            if (!freezed_ && slot != null)
            {
                string item_unique = slot.itemMeta.Unique();
                if (!carriedCache_.ContainsKey(item_unique))
                {
                    carriedCache_[item_unique] = ItemManager.Instance.CreateItem(slot.itemMeta, transform.position, hands_.Find("Cache"));
                }
                Item item = carriedCache_[item_unique];
                item.gameObject.SetActive(false);
                item.transform.parent = hands_;
                carried_ = new KeyValuePair<Item, Slot>(item, slot);
            }
            else
            {
                carried_ = new KeyValuePair<Item, Slot>(null, null);
            }
            if (carried_.Key != null && carried_.Key.dropRadius > 0)
            {
                handsRender_.sprite = carried_.Key.meta.sprite;
                handsRender_.color = new Color(1f, 1f, 1f, 1f);
                SwapAnimations(AnimationTag.Carry);
                FieldManager.Instance.Unfreeze();
            }
            else
            {
                handsRender_.sprite = null;
                handsRender_.color = new Color(1f, 1f, 1f, 0f);
                RestoreAnimations(AnimationTag.Carry);
                FieldManager.Instance.Freeze();
            }
        }
    }

    private void UpdateInventory(Transform owner, ContainerType container_type, bool sort, bool deselect)
    {
        if (owner == transform)
        {
            inventory_.GetContainer(container_type).UpdateSlots(sort, deselect);
        }
    }

    // TMINFO:debug only
    private void InputTest()
    {
        // pick item test
        if (Input.GetKeyDown(KeyCode.P))
        {
            ItemData item_data = ItemManager.Instance.RandomItem();
            inventory_.AddItem(item_data);
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            ItemData hoe = ItemManager.Instance.FindItem("Hoe");
            ItemData water_can = ItemManager.Instance.FindItem("WaterCan");
            inventory_.AddItem(hoe);
            inventory_.AddItem(water_can);
        }
        else if (Input.GetKeyDown(KeyCode.C) && carried_.Key != null)
        {
            ProcessCarried(Action.HoldItem);
            ProcessCarried(Action.UseItem);
        }
    }

    public Vector3 GetViewportPosition()
    {
        return camera_.WorldToViewportPoint(transform.position);
    }

    public void Freeze()
    {
        freezed_ = true;
        foreach (Animator animator in animators_)
        {
            animator.SetInteger("action", (int)Action.Idle);
        }
    }

    public void Unfreeze()
    {
        freezed_ = false;
    }
}

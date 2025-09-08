using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player>
{
    [SerializeField] private float runSpeed_ = 5f;
    [SerializeField] private float walkSpeed_ = 3f;
    [SerializeField] private PlayerInventory inventory_;
    private bool freezed_ = false;
    private Direction direction_ = Direction.Down;
    private Action action_ = Action.Idle;
    private Rigidbody2D rigidBody_;
    private SpriteRenderer hands_;
    private Animator[] animators_;
    private AnimationSwapper[] animationSwappers_;
    private Camera camera_;
    private KeyValuePair<Item, Slot> carried_;

    protected override void Awake()
    {
        base.Awake();
        rigidBody_ = GetComponent<Rigidbody2D>();
        animators_ = GetComponentsInChildren<Animator>();
        inventory_.Setup(transform);
        animationSwappers_ = GetComponentsInChildren<AnimationSwapper>();
        Transform hands_transform = transform.Find("Hands");
        hands_ = hands_transform.GetComponent<SpriteRenderer>();
        camera_ = Camera.main;
        carried_ = new KeyValuePair<Item, Slot>(null, null);
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
        /*
        if (!freezed_ && carried_.Key != null)
        {
            Action action = Action.Idle;
            if (Input.GetMouseButtonDown(0))
            {
                action = Action.DropItem;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                action = Action.UseItem;
            }
            ProcessCarried(action);
        }
        */
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
        List<Cursor> cursors = EnvManager.Instance.CheckItem(carried_.Key, transform.position, CameraUtils.MouseToWorld(camera_));
        if (cursors != null && cursors.Count > 0)
        {
            foreach (Cursor cursor in cursors)
            {
                if (action == Action.DropItem && cursor.HasStatus(ItemStatus.Dropable))
                {
                    EnvManager.Instance.DropItem(carried_.Key, cursor);
                    carried_.Value.DecreaseAmount();
                }
                else if (action == Action.UseItem && (cursor.HasStatus(ItemStatus.GridUsable) || cursor.HasStatus(ItemStatus.ItemUsable)))
                {
                    int use_amount = EnvManager.Instance.UseItem(carried_.Key, cursor);
                    carried_.Value.DecreaseAmount(use_amount);
                }
            }
        }
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
            Slot slot = inventory_.GetContainer(container_type).FindSelectedSlot();
            if (!freezed_ && slot != null)
            {
                Item item = ItemManager.Instance.CreateItem(slot.ItemMeta, transform.position, transform);
                item.gameObject.SetActive(false);
                carried_ = new KeyValuePair<Item, Slot>(item, slot);
            }
            else
            {
                if (carried_.Key != null)
                {
                    Destroy(carried_.Key);
                }
                carried_ = new KeyValuePair<Item, Slot>(null, null);
            }
            if (carried_.Key != null && carried_.Key.DropRadius > 0)
            {
                hands_.sprite = carried_.Key.Meta.sprite;
                hands_.color = new Color(1f, 1f, 1f, 1f);
                SwapAnimations(AnimationTag.Carry);
                EnvManager.Instance.Unfreeze();
            }
            else
            {
                hands_.sprite = null;
                hands_.color = new Color(1f, 1f, 1f, 0f);
                RestoreAnimations(AnimationTag.Carry);
                EnvManager.Instance.Freeze();
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
            ItemData item_data = ItemManager.Instance.FindItem("Hoe");
            inventory_.AddItem(item_data);
        }
        else if (Input.GetKeyDown(KeyCode.C) && carried_.Key != null)
        {
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

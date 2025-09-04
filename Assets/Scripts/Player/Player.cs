using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player>
{
    public float runSpeed = 5f;
    public float walkSpeed = 3f;
    public PlayerInventory inventory;
    private bool freezed_ = false;
    private Direction direction_ = Direction.Down;
    private HumanAction action_ = HumanAction.Idle;
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
    }

    private void OnDisable()
    {
        EventHandler.UpdateHandsEvent -= UpdateHands;
    }

    private void Update()
    {
        if (carried_.Key != null && !freezed_)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (carried_.Key.Dropable)
                    DropItem(carried_.Key, carried_.Value);
                else
                    UseItem(carried_.Key, carried_.Value);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (carried_.Key.Usable)
                    UseItem(carried_.Key, carried_.Value);
                else
                    DropItem(carried_.Key, carried_.Value);
            }
        }
        InputTest();
    }

    private void FixedUpdate()
    {
        if (!freezed_)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector2 movement = new Vector2(horizontal, vertical).normalized;
            float speed;
            if (movement == Vector2.zero)
            {
                action_ = HumanAction.Idle;
                speed = 0;
            }
            else
            {
                // set speed
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    action_ = HumanAction.Walk;
                    speed = walkSpeed;
                }
                else
                {
                    action_ = HumanAction.Run;
                    speed = runSpeed;
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

    private void UpdateHands()
    {
        Slot slot = inventory.FindSelectSlot();
        if (slot != null && slot.ItemData.dropable)
        {
            hands_.sprite = slot.ItemData.sprite;
            hands_.color = new Color(1f, 1f, 1f, 1f);
            SwapAnimations(AnimationTag.Carry);
        }
        else
        {
            hands_.sprite = null;
            hands_.color = new Color(1f, 1f, 1f, 0f);
            RestoreAnimations(AnimationTag.Carry);
        }
        if (!freezed_ && slot != null)
        {
            Item item = ItemManager.Instance.CreateItemInWorld(slot.ItemData, transform.position, transform);
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
    }

    private void DropItem(Item item, Slot slot)
    {
        slot.DropItemAtMouse();
    }

    private void UseItem(Item item, Slot slot)
    {
        Debug.Log("UseItem is not suuported");
    }

    // TMINFO:debug only
    private void InputTest()
    {
        // pick item test
        if (Input.GetKeyDown(KeyCode.P))
        {
            ItemData item = ItemManager.Instance.RandomItem();
            inventory.AddItem(item);
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
            animator.SetInteger("action", (int)HumanAction.Idle);
        }
    }

    public void Unfreeze()
    {
        freezed_ = false;
    }
}

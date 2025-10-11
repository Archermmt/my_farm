using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Singleton<Player> {
    [SerializeField] private float runSpeed_ = 5f;
    [SerializeField] private float walkSpeed_ = 3f;
    [SerializeField] private PlayerInventory inventory_;
    [SerializeField] private float useTooleSec_ = 0.5f;
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
    private WaitForSeconds useToolWait_;

    protected override void Awake() {
        base.Awake();
        rigidBody_ = GetComponent<Rigidbody2D>();
        animators_ = GetComponentsInChildren<Animator>();
        useToolWait_ = new WaitForSeconds(useTooleSec_);
        inventory_.Setup(transform.tag);
        animationSwappers_ = GetComponentsInChildren<AnimationSwapper>();
        hands_ = transform.Find("Hands");
        handsRender_ = hands_.GetComponent<SpriteRenderer>();
        camera_ = Camera.main;
        carried_ = new KeyValuePair<Item, Slot>(null, null);
        carriedCache_ = new Dictionary<string, Item>();
    }

    private void OnEnable() {
        inventory_.AddItem(ItemManager.Instance.FindItem("Hoe"));
        inventory_.AddItem(ItemManager.Instance.FindItem("WaterCan"));
        inventory_.AddItem(ItemManager.Instance.FindItem("Scythe"));
        inventory_.AddItem(ItemManager.Instance.FindItem("Basket"));
        inventory_.AddItem(ItemManager.Instance.FindItem("Pickaxe"));
        inventory_.AddItem(ItemManager.Instance.FindItem("Axe"));
        ItemManager.Instance.SetFreeze(true);
        UpdateAnimators(direction_, action_);
        EventHandler.UpdateHandsEvent += UpdateHands;
        EventHandler.UpdateInventoryEvent += UpdateInventory;
        EventHandler.AddInventoryItemEvent += AddInventoryItem;
    }

    private void OnDisable() {
        EventHandler.UpdateHandsEvent -= UpdateHands;
        EventHandler.UpdateInventoryEvent -= UpdateInventory;
        EventHandler.AddInventoryItemEvent -= AddInventoryItem;
    }

    private void Update() {
        if (carried_.Key != null && !ItemManager.Instance.freezed) {
            Action action = Action.Idle;
            if (Input.GetMouseButtonDown(0)) {
                action = Action.DropItem;
            } else if (Input.GetMouseButtonDown(1)) {
                action = Action.HoldItem;
            } else if (Input.GetMouseButtonUp(1)) {
                action = Action.UseItem;
            }
            ProcessCarried(action);
        }
        ProcessInput();
        InputTest();
    }

    private void FixedUpdate() {
        if (!freezed_) {
            ProcessMovement();
        }
    }

    private void ProcessMovement() {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 movement = new Vector2(horizontal, vertical).normalized;
        float speed;
        if (movement == Vector2.zero) {
            action_ = Action.Idle;
            speed = 0;
        } else {
            // set speed
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                action_ = Action.Walk;
                speed = walkSpeed_;
                AudioManager.Instance.PlaySound("FootStepSoft");
            } else {
                action_ = Action.Run;
                speed = runSpeed_;
                AudioManager.Instance.PlaySound("FootStepHard");
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
            ItemManager.Instance.SetFreeze(false);
        }
        UpdateAnimators(direction_, action_);
        rigidBody_.MovePosition(rigidBody_.position + movement * speed * Time.deltaTime);
    }

    private void ProcessCarried(Action action) {
        direction_ = MouseUtils.GetDirection(camera_, transform.position);
        List<Cursor> cursors = FieldManager.Instance.CheckItem(carried_.Key, transform.position, MouseUtils.MouseToWorld(camera_), direction_, carried_.Value.current);
        if (cursors.Count > 0) {
            if (action == Action.DropItem && carried_.Key.HasStatus(ItemStatus.Dropable)) {
                int drop_amount = FieldManager.Instance.DropItem(carried_.Key, cursors);
                carried_.Value.DecreaseAmount(drop_amount);
            } else if (action == Action.HoldItem && !carried_.Key.HasStatus(ItemStatus.Holding)) {
                SetFreeze(true);
                if (carried_.Key.meta.type == ItemType.Tool || carried_.Key.meta.type == ItemType.Seed) {
                    carried_.Key.Hold(direction_);
                    Action act = carried_.Key.meta.type == ItemType.Tool ? Action.HoldItem : Action.Idle;
                    UpdateAnimators(direction_, act);
                } else {
                    carried_.Key.Hold(Direction.Around);
                }
            } else if (action == Action.UseItem && carried_.Key.HasStatus(ItemStatus.Holding)) {
                Dictionary<ItemData, int> item_amounts = FieldManager.Instance.UseItem(carried_.Key, cursors, carried_.Value.current);
                carried_.Key.Unhold();
                if (carried_.Key.meta.type == ItemType.Tool) {
                    StartCoroutine(UseToolRoutine(direction_));
                } else {
                    SetFreeze(false);
                }
                foreach (KeyValuePair<ItemData, int> pair in item_amounts) {
                    if (pair.Value > 0) {
                        inventory_.AddItem(pair.Key, pair.Value);
                    } else {
                        inventory_.RemoveItem(pair.Key, -pair.Value);
                    }
                }
            } else if (carried_.Key.HasStatus(ItemStatus.Holding)) {
                UpdateAnimators(direction_, carried_.Key.meta.type == ItemType.Tool ? Action.HoldItem : Action.Idle);
            }
        }
    }

    private IEnumerator UseToolRoutine(Direction direction) {
        foreach (Animator animator in animators_) {
            animator.SetTrigger("useTool");
        }
        yield return useToolWait_;
        UpdateAnimators(direction, Action.Idle);
        SetFreeze(false);
    }


    private void UpdateAnimators(Direction direct, Action act) {
        foreach (Animator animator in animators_) {
            animator.SetInteger("direction", (int)direct);
            animator.SetInteger("action", (int)act);
        }
    }

    private void SwapAnimations(AnimationTag animation_tag) {
        foreach (AnimationSwapper swapper in animationSwappers_) {
            swapper.Swap(animation_tag);
        }
    }

    private void RestoreAnimations(AnimationTag animation_tag) {
        foreach (AnimationSwapper swapper in animationSwappers_) {
            swapper.Restore(animation_tag);
        }
    }

    private void UpdateHands(string owner, ContainerType container_type) {
        if (owner == transform.tag && container_type == ContainerType.ToolBar) {
            if (carried_.Key != null) {
                carried_.Key.gameObject.SetActive(false);
                carried_.Key.transform.parent = hands_.Find("Cache");
                foreach (AnimationTag tag in carried_.Key.animationTags) {
                    RestoreAnimations(tag);
                }
            }
            Slot slot = inventory_.GetContainer(container_type).FindSelectedSlot();
            if (!freezed_ && slot != null) {
                string item_unique = slot.itemMeta.Unique();
                if (!carriedCache_.ContainsKey(item_unique)) {
                    carriedCache_[item_unique] = ItemManager.Instance.CreateItem(slot.itemMeta, transform.position, hands_.Find("Cache"));
                }
                Item item = carriedCache_[item_unique];
                item.gameObject.SetActive(false);
                item.transform.parent = hands_;
                carried_ = new KeyValuePair<Item, Slot>(item, slot);
            } else {
                carried_ = new KeyValuePair<Item, Slot>(null, null);
            }
            if (carried_.Key != null) {
                if (carried_.Key.animationTags.Contains(AnimationTag.Carry)) {
                    handsRender_.sprite = carried_.Key.meta.sprite;
                    handsRender_.color = new Color(1f, 1f, 1f, 1f);
                }
                foreach (AnimationTag tag in carried_.Key.animationTags) {
                    SwapAnimations(tag);
                }
                FieldManager.Instance.SetFreeze(false);
            } else {
                handsRender_.sprite = null;
                handsRender_.color = new Color(1f, 1f, 1f, 0f);
                FieldManager.Instance.SetFreeze(true);
            }
        }
    }

    private void UpdateInventory(string owner, ContainerType container_type, bool sort, bool deselect) {
        if (owner == transform.tag) {
            inventory_.UpdateContainer(container_type, sort, deselect);
        }
    }

    private void AddInventoryItem(string owner, ItemData item, int amount) {
        if (owner == transform.tag) {
            inventory_.AddItem(item, amount);
        }
    }

    private void ProcessInput() {
        if (Input.GetKeyDown(KeyCode.P)) {
            if (inventory_.backpackOpening) {
                SetFreeze(false);
                ItemManager.Instance.SetFreeze(false);
                inventory_.CloseBackpack();
            } else {
                SetFreeze(true);
                ItemManager.Instance.SetFreeze(true);
                inventory_.OpenBackpack();
            }
        } else if (Input.GetKeyDown(KeyCode.Escape)) {
            if (carried_.Key != null && carried_.Key.HasStatus(ItemStatus.Holding)) {
                carried_.Key.Unhold();
                SetFreeze(false);
            }
        }

    }

    // TMINFO:debug only
    private void InputTest() {
        // pick item test
        if (Input.GetKeyUp(KeyCode.T)) {
            EnvManager.Instance.UpdateTime(TimeType.Day, 5);
        } else if (Input.GetKeyDown(KeyCode.C)) {
            inventory_.AddItem(ItemManager.Instance.FindItem("Weed"), 100);
            //ItemData item = ItemManager.Instance.FindItem("ParsnipSeed");
            //inventory_.AddItem(item, 10);
            //inventory_.RemoveItem(item, 10);
        }
    }

    public Vector3 GetViewportPosition() {
        return camera_.WorldToViewportPoint(transform.position);
    }

    public void SetFreeze(bool freeze) {
        freezed_ = freeze;
        if (freezed_) {
            foreach (Animator animator in animators_) {
                animator.SetInteger("action", (int)Action.Idle);
            }
        }
    }
}

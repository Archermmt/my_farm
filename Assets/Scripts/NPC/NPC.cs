using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class NPCStep {
    public SceneName scene;
    public Vector3 pos;
    public float minute;
    public FieldTag tag;

    public NPCStep(SceneName scene, Vector3 pos, float minute) {
        this.scene = scene;
        this.pos = pos;
        this.minute = minute;
    }

    public override string ToString() {
        string str = pos + "(" + scene + ") @ " + (int)minute / 60 + ":" + minute % 60;
        if (tag != FieldTag.Basic) {
            str += ", tag " + tag;
        }
        return str;
    }
}

public class PathNode : IComparable<PathNode> {
    public FieldGrid grid;
    public float gCost = 0;
    public float hCost = 0;
    public float penalty;
    public PathNode parent;

    public PathNode(FieldGrid grid) {
        this.grid = grid;
        parent = null;
    }

    public int CompareTo(PathNode other) {
        int res = FCost.CompareTo(other.FCost);
        return res == 0 ? hCost.CompareTo(other.hCost) : res;
    }

    public void Reset() {
        gCost = 0;
        hCost = 0;
        parent = null;
    }

    public float FCost { get { return gCost + hCost; } }
    public Vector2Int coord { get { return grid.coord; } }
    public override string ToString() {
        return "Grid: " + grid + ", hCost: " + hCost + ",gCost: " + gCost + ",penalty: " + penalty;
    }
}

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class NPC : MonoBehaviour {
    [SerializeField] private float speed_ = 3f;
    [SerializeField] private bool usePenalty = true;
    [UnityEngine.Range(0, 20)]
    [SerializeField] private int pathPenalty = 0;
    [UnityEngine.Range(0, 20)]
    [SerializeField] private int defaultPenalty = 5;
    private Animator animator_;
    private SpriteRenderer render_;
    private BoxCollider2D collider_;
    private Rigidbody2D rigidBody_;
    private bool freezed_ = false;
    // path
    private PathNode[,] pathNodes_;
    private bool followPath_ = false;
    // movement
    private Direction direction_ = Direction.Down;
    private Stack<NPCStep> path_;
    private NPCStep currentStep_;

    private void Awake() {
        animator_ = GetComponent<Animator>();
        rigidBody_ = GetComponent<Rigidbody2D>();
        collider_ = GetComponent<BoxCollider2D>();
        render_ = GetComponent<SpriteRenderer>();
        path_ = new Stack<NPCStep>();
    }

    private void Start() {
        TimeData time = EnvManager.Instance.time;
        currentStep_ = new NPCStep(SceneController.Instance.currentScene, transform.position, time.hour * 60 + time.minute);
    }

    private void Update() {
        InputTest();
    }

    private void FixedUpdate() {
        if (!freezed_) {
            ProcessMovement();
        }
    }

    private void OnEnable() {
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnload;
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable() {
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnload;
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        Item item = collision.transform.GetComponent<Item>();
        if (item != null && item.meta.type == ItemType.Obstacle) {
            BoxCollider2D collider = collision.transform.GetComponent<BoxCollider2D>();
            Vector2 normal = collision.GetContact(0).normal;
            Vector3 pos = transform.position;
            if (normal.x == 0) {
                if (transform.position.x > collider.transform.position.x) {
                    Vector3 new_pos = new Vector3(pos.x + collider.size.x, pos.y, pos.z);
                    rigidBody_.MovePosition(new_pos);
                } else {
                    Vector3 new_pos = new Vector3(pos.x - collider.size.x, pos.y, pos.z);
                    rigidBody_.MovePosition(new_pos);
                }
            } else if (normal.x == 1) {
                if (transform.position.y > collider.transform.position.y) {
                    Vector3 new_pos = new Vector3(pos.x, pos.y + collider.size.y, pos.z);
                    rigidBody_.MovePosition(new_pos);
                } else {
                    Vector3 new_pos = new Vector3(pos.x, pos.y - collider.size.y, pos.z);
                    rigidBody_.MovePosition(new_pos);
                }
            }
        }
    }

    public void MoveTo(SceneName dst_scene, FieldGrid dst, int hour, int minute, FieldTag dst_tag = FieldTag.Basic) {
        MoveTo(dst_scene, dst.GetCenter(), hour, minute, dst_tag);
    }

    public void MoveTo(SceneName dst_scene, FieldTag tag, int hour, int minute) {
        SceneName current_scene = SceneController.Instance.currentScene;
        if (current_scene == dst_scene) {
            MoveTo(dst_scene, FieldManager.Instance.GetRandomGrid(tag), hour, minute, FieldTag.Basic);
        } else {
            ScenePort port = SceneController.Instance.FindPort(dst_scene);
            Assert.AreNotEqual(port, null, "Can not find port from " + current_scene + " to " + dst_scene);
            MoveTo(dst_scene, port.dstPos, hour, minute, tag);
        }
    }

    public void MoveTo(SceneName dst_scene, Vector3 dst_pos, int hour, int minute, FieldTag dst_tag = FieldTag.Basic) {
        path_ = new Stack<NPCStep>();
        SceneName current_scene = SceneController.Instance.currentScene;
        float dst_minute = hour * 60 + minute;
        if (current_scene == dst_scene) {
            if (currentStep_.scene == current_scene) {
                BuildPath(transform.position, dst_pos, dst_minute);
            } else {
                ScenePort port = SceneController.Instance.FindPort(currentStep_.scene);
                Assert.AreNotEqual(port, null, "Can not find port from " + current_scene + " to " + currentStep_.scene);
                float delta = BuildPath(port.transform.position, dst_pos, dst_minute);
                NPCStep step = new NPCStep(currentStep_.scene, port.dstPos, dst_minute - delta);
                step.tag = dst_tag;
                path_.Push(step);
            }
        } else {
            if (currentStep_.scene == current_scene) {
                ScenePort port = SceneController.Instance.FindPort(dst_scene);
                Assert.AreNotEqual(port, null, "Can not find port from " + current_scene + " to " + dst_scene);
                float delta = GetMoveTime(port.dstPos, dst_pos);
                NPCStep step = new NPCStep(dst_scene, port.dstPos, dst_minute - delta);
                step.tag = dst_tag;
                path_.Push(step);
                BuildPath(transform.position, port.transform.position, dst_minute - delta);
            } else {
                NPCStep step = new NPCStep(dst_scene, dst_pos, dst_minute);
                step.tag = dst_tag;
                path_.Push(step);
            }
        }
    }

    private float GetMoveTime(Vector3 src, Vector3 dst) {
        float distance = Vector3.Distance(src, dst);
        float delta = distance * EnvManager.Instance.minuteTick / speed_;
        return delta;
    }

    private float BuildPath(Vector3 src, Vector3 dst, float dst_minute) {
        // reset nodes
        FieldGrid src_grid = FieldManager.Instance.GetGrid(src);
        Debug.Log("[TMINFO] src_grid from " + src + " -> " + src_grid);
        FieldGrid dst_grid = FieldManager.Instance.GetGrid(dst);
        PathNode dst_node = GetNode(dst_grid);
        List<PathNode> frontier = new List<PathNode> { GetNode(src_grid) };
        HashSet<PathNode> visited = new HashSet<PathNode>();
        bool path_found = false;
        // find path
        while (frontier.Count > 0) {
            frontier.Sort();
            PathNode node = frontier[0];
            frontier.RemoveAt(0);
            visited.Add(node);
            if (node == dst_node) {
                path_found = true;
                break;
            }
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (i == 0 && j == 0) {
                        continue;
                    }
                    PathNode neighbour = NodeAt(node.coord.x + i, node.coord.y + j);
                    if (neighbour == null || neighbour.grid.HasTag(FieldTag.Obstacle) || neighbour.grid.HasItemType(ItemType.Obstacle) || visited.Contains(neighbour)) {
                        continue;
                    }
                    float cost = node.gCost + Vector2Int.Distance(node.coord, neighbour.coord) + (usePenalty ? neighbour.penalty : 0);
                    bool recorded = frontier.Contains(neighbour);
                    if (cost < neighbour.gCost || !recorded) {
                        neighbour.gCost = cost;
                        neighbour.hCost = Vector2Int.Distance(node.coord, dst_node.coord);
                        neighbour.parent = node;
                        if (!recorded) {
                            frontier.Add(neighbour);
                        }
                    }
                }
            }
        }
        float delta = 0;
        // build steps
        if (path_found) {
            SceneName current_scene = SceneController.Instance.currentScene;
            PathNode next_node = dst_node;
            delta += GetMoveTime(dst_node.grid.GetCenter(), next_node.grid.GetCenter());
            while (next_node != null) {
                path_.Push(new NPCStep(current_scene, next_node.grid.GetCenter(), dst_minute - delta));
                if (next_node.parent != null) {
                    delta += GetMoveTime(next_node.parent.grid.GetCenter(), next_node.grid.GetCenter());
                }
                next_node = next_node.parent;
            }
        }
        // cleanup
        foreach (PathNode node in visited) {
            node.Reset();
        }
        return delta;
    }

    private void BuildNodes() {
        KeyValuePair<Vector3Int, Vector3Int> scope = FieldManager.Instance.scope;
        pathNodes_ = new PathNode[scope.Value.x - scope.Key.x, scope.Value.y - scope.Key.y];
        for (int i = 0; i < pathNodes_.GetLength(0); i++) {
            for (int j = 0; j < pathNodes_.GetLength(1); j++) {
                FieldGrid grid = FieldManager.Instance.GridAt(i, j);
                if (grid == null) {
                    continue;
                }
                pathNodes_[i, j] = new PathNode(grid);
                pathNodes_[i, j].penalty = grid.HasTag(FieldTag.Path) ? pathPenalty : defaultPenalty;
            }
        }
    }

    public PathNode NodeAt(int x, int y) {
        if (x >= pathNodes_.GetLength(0) || y >= pathNodes_.GetLength(1) || x < 0 || y < 0) {
            return null;
        }
        return pathNodes_[x, y];
    }

    public PathNode GetNode(FieldGrid grid) {
        return NodeAt(grid.coord.x, grid.coord.y);
    }

    private void BeforeSceneUnload(SceneName scene_name) {
        SetFreeze(true);
    }

    private void AfterSceneLoad(SceneName scene_name) {
        BuildNodes();
        SetFreeze(false);
        if (currentStep_.scene == scene_name) {
            Debug.Log("[TMINFO] AfterSceneLoad has curretStep_ " + currentStep_);
            collider_.enabled = true;
            render_.enabled = true;
            if (currentStep_.tag != FieldTag.Basic) {
                TimeData time = EnvManager.Instance.time;
                int hour = time.hour;
                int minute = time.minute + 10;
                if (minute >= 60) {
                    hour += 1;
                    minute -= 60;
                }
                MoveTo(scene_name, currentStep_.tag, hour, minute);
                foreach (NPCStep step in path_) {
                    Debug.Log("has step " + step);
                }
            }
        }
    }

    public void ProcessMovement() {
        TimeData time = EnvManager.Instance.time;
        SceneName current_scene = SceneController.Instance.currentScene;
        float current_minute = time.hour * 60 + time.minute;
        if (!followPath_ && path_.Count > 0) {
            NPCStep first_step = path_.Peek();
            while (first_step != null && current_minute >= first_step.minute) {
                first_step = path_.Count > 0 ? path_.Pop() : null;
            }
            if (first_step == null) {
                return;
            }
            float first_diff = first_step.minute - current_minute;
            if (first_step.scene != current_scene) {
                followPath_ = first_diff < 0.01f;
            } else {
                float first_delta = GetMoveTime(transform.position, first_step.pos);
                followPath_ = first_delta >= first_diff * 0.8;
            }
        }
        if (!followPath_) {
            return;
        }
        collider_.enabled = false;
        NPCStep next_step = path_.Peek();
        if (next_step.scene != current_scene) {
            currentStep_ = path_.Pop();
        } else {
            Vector3 src = transform.position;
            Vector3 dst = next_step.pos;
            float distance = Vector3.Distance(src, dst);
            if (Math.Abs(distance) < 0.05f) {
                currentStep_ = path_.Pop();
                rigidBody_.MovePosition(currentStep_.pos);
            } else {
                Vector2 movement = new Vector2(dst.x - src.x, dst.y - src.y).normalized;
                if (movement.x > 0)
                    direction_ = Direction.Right;
                else if (movement.x < 0)
                    direction_ = Direction.Left;
                else if (movement.y < 0)
                    direction_ = Direction.Down;
                else
                    direction_ = Direction.Up;
                rigidBody_.MovePosition(rigidBody_.position + movement * speed_ * Time.deltaTime);
                UpdateAnimator(direction_, Action.Walk);
            }
        }
        if (path_.Count == 0) {
            followPath_ = false;
            if (next_step.scene == current_scene) {
                collider_.enabled = true;
            } else {
                collider_.enabled = false;
                render_.enabled = false;
            }
        }
    }

    private void UpdateAnimator(Direction direct, Action act) {
        /*
        animator_.SetInteger("direction", (int)direct);
        animator_.SetInteger("action", (int)act);
        */
    }

    // Debug only
    private void InputTest() {
        // pick item test
        if (Input.GetKeyUp(KeyCode.N)) {
            TimeData time = EnvManager.Instance.time;
            currentStep_ = new NPCStep(SceneName.FarmScene, transform.position, time.hour * 60 + time.minute);
            MoveTo(SceneName.CabinScene, FieldTag.Dropable, 6, 20);
            //MoveTo(SceneName.FarmScene, FieldTag.Diggable, 6, 20);
            int cnt = 0;
            foreach (NPCStep step in path_) {
                //Debug.Log("step " + step);
                FieldManager.Instance.GetMaskCursor(cnt).MoveTo(step.pos, CursorMode.Mask);
                cnt += 1;
            }
        } else if (Input.GetKeyUp(KeyCode.M)) {
            ProcessMovement();
        }
    }


    public void SetFreeze(bool freeze) {
        freezed_ = freeze;
        if (freezed_) {
            UpdateAnimator(direction_, Action.Idle);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[Serializable]
public class NPCEvent : IComparable<NPCEvent> {
    public SceneName scene;
    public Season season = Season.Any;
    public FieldTag fieldTag = FieldTag.Dropable;
    public Action action = Action.Idle;
    public int hour;
    public int minute;
    public int durationMin = 20;
    public int durationMax = 60;
    public float actionStart = 10f;
    public float actionDuration = 10f;
    public List<int> weekDays;
    public List<int> days;
    private HashSet<int> weekDaysInclude_;
    private HashSet<int> weekDaysExclude_;
    private HashSet<int> daysInclude_;
    private HashSet<int> daysExclude_;
    private bool start_ = false;

    public int CompareTo(NPCEvent other) {
        return time.CompareTo(other.time);
    }

    public void Delay(int delta) {
        minute += delta;
        while (minute >= 60) {
            minute -= 60;
            hour += 1;
        }
    }

    public void Setup() {
        weekDaysInclude_ = new HashSet<int>();
        weekDaysExclude_ = new HashSet<int>();
        daysInclude_ = new HashSet<int>();
        daysExclude_ = new HashSet<int>();
        if (weekDays.Count > 0) {
            foreach (int d in weekDays) {
                if (d >= 0) {
                    weekDaysInclude_.Add(d);
                } else {
                    weekDaysExclude_.Add(-d);
                }
            }
        }
        if (days.Count > 0) {
            foreach (int d in days) {
                if (d >= 0) {
                    daysInclude_.Add(d);
                } else {
                    daysExclude_.Add(-d);
                }
            }
        }
    }

    public bool Validate(TimeData time) {
        if (season != Season.Any && season != time.season) {
            return false;
        }
        if (weekDaysInclude_.Count > 0 && !weekDaysInclude_.Contains(time.weekDay)) {
            return false;
        }
        if (weekDaysExclude_.Count > 0 && weekDaysExclude_.Contains(time.weekDay)) {
            return false;
        }
        if (daysInclude_.Count > 0 && !daysInclude_.Contains(time.weekDay)) {
            return false;
        }
        if (daysExclude_.Count > 0 && daysExclude_.Contains(time.weekDay)) {
            return false;
        }
        return true;
    }

    public void SetStart(bool start) {
        start_ = start;
    }

    public static NPCEvent Clone(NPCEvent other) {
        NPCEvent clone_event = new NPCEvent();
        clone_event.scene = other.scene;
        clone_event.season = other.season;
        clone_event.fieldTag = other.fieldTag;
        clone_event.action = other.action;
        clone_event.hour = other.hour;
        clone_event.minute = other.minute;
        clone_event.durationMin = other.durationMin;
        clone_event.durationMax = other.durationMax;
        clone_event.weekDays = other.weekDays;
        clone_event.days = other.days;
        clone_event.Setup();
        return clone_event;
    }

    public override string ToString() {
        string str = action + " in " + scene + "(" + fieldTag + ") @ " + hour + ":" + minute;
        if (weekDays.Count > 0) {
            str += ",[WeekDays] ";
            foreach (int d in weekDays) {
                str += d + ",";
            }
        }
        if (days.Count > 0) {
            str += ",[Day] ";
            foreach (int d in days) {
                str += d + ",";
            }
        }
        return str;
    }

    public int duration { get { return UnityEngine.Random.Range(durationMin, durationMax); } }
    public int time { get { return hour * 60 + minute; } }
    public bool start { get { return start_; } }
}

public class NPCStep {
    public SceneName scene;
    public Vector3 pos;
    public float minute;

    public NPCStep(SceneName scene, Vector3 pos, float minute) {
        this.scene = scene;
        this.pos = pos;
        this.minute = minute;
    }

    public override string ToString() {
        string str = pos + "(" + scene + ") @ " + (int)minute / 60 + ":" + minute % 60;
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
    [Header("Schedule")]
    [SerializeField] private SceneName startScene_;
    [SerializeField] private Vector3 startPos_;
    [SerializeField] private List<NPCEvent> eventList_;
    [Header("Movement")]
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
    // schedule
    private int eventId_ = 0;
    private List<NPCEvent> schedule_;
    private NPCEvent currentEvent_;
    private NPCEvent nextEvent_;
    private bool acting_ = false;
    // path
    private Dictionary<SceneName, PathNode[,]> pathNodes_;
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
        eventList_.Sort();
        pathNodes_ = new Dictionary<SceneName, PathNode[,]>();
        path_ = new Stack<NPCStep>();
        foreach (NPCEvent e in eventList_) {
            e.Setup();
        }
    }

    private void Start() {
        TimeData time = EnvManager.Instance.time;
        ChangeStep(new NPCStep(startScene_, startPos_, time.hour * 60 + time.minute));
        MakeSchedule();
        foreach (NPCEvent e in schedule_) {
            Debug.Log("Has event " + e);
        }
    }

    private void Update() {
        InputTest();
    }

    private void FixedUpdate() {
        if (!freezed_) {
            ProcessMovement();
            if (currentEvent_ != null && currentEvent_.start && !acting_) {
                Debug.Log("[TMINFO] start action with " + currentEvent_);
                StartCoroutine(ActionRoutine());
            }
        }
    }

    private void OnEnable() {
        EventHandler.UpdateTimeEvent += UpdateTime;
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnload;
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable() {
        EventHandler.UpdateTimeEvent -= UpdateTime;
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
                float x_diff = transform.position.x > collider.transform.position.x ? collider.size.x : -collider.size.x;
                Vector3 new_pos = new Vector3(pos.x + x_diff, pos.y, pos.z);
                rigidBody_.MovePosition(new_pos);
            } else if (normal.x == 1) {
                float y_diff = transform.position.y > collider.transform.position.y ? collider.size.y : -collider.size.y;
                Vector3 new_pos = new Vector3(pos.x, pos.y + y_diff, pos.z);
                rigidBody_.MovePosition(new_pos);
            }
        }
    }

    public void MoveTo(SceneName dst_scene, FieldGrid dst, int time) {
        MoveTo(dst_scene, dst.GetCenter(), time);
    }

    public void MoveTo(SceneName dst_scene, FieldTag tag, int time) {
        MoveTo(dst_scene, FieldManager.Instance.GetRandomGrid(tag, dst_scene), time);
    }

    public void MoveTo(SceneName dst_scene, Vector3 dst_pos, int time) {
        path_ = new Stack<NPCStep>();
        followPath_ = false;
        SceneName current_scene = SceneController.Instance.currentScene;
        if (currentStep_.scene == current_scene) {
            collider_.enabled = true;
        }
        float dst_time = time;
        if (currentStep_.scene == dst_scene) {
            BuildPath(currentStep_.scene, transform.position, dst_pos, dst_time);
        } else {
            ScenePortSave port = SceneController.Instance.FindPort(currentStep_.scene, dst_scene);
            Assert.AreNotEqual(port, null, "Can not find port from " + currentStep_.scene + " to " + dst_scene);
            float delta = BuildPath(dst_scene, port.exit, dst_pos, dst_time);
            BuildPath(currentStep_.scene, transform.position, port.enter, dst_time - delta);
        }
    }

    private void ChangeStep(NPCStep step) {
        currentStep_ = step;
        SceneName currect_scene = SceneController.Instance.currentScene;
        if (currentStep_.scene == currect_scene) {
            collider_.enabled = true;
            render_.enabled = true;
        } else {
            collider_.enabled = false;
            render_.enabled = false;
        }
        transform.position = currentStep_.pos;
    }

    private void MakeSchedule() {
        TimeData time = EnvManager.Instance.time;
        List<NPCEvent> valid_events = new List<NPCEvent>();
        foreach (NPCEvent e in eventList_) {
            if (e.Validate(time)) {
                valid_events.Add(e);
            }
        }
        schedule_ = new List<NPCEvent>();
        for (int i = 0; i < valid_events.Count; i++) {
            schedule_.Add(valid_events[i]);
            int next_time = i == valid_events.Count - 1 ? 23 * 60 : valid_events[i + 1].time;
            NPCEvent last = schedule_[schedule_.Count - 1];
            int duration = last.duration;
            while (last.time + duration + last.durationMin < next_time) {
                NPCEvent next = NPCEvent.Clone(last);
                next.Delay(duration);
                schedule_.Add(next);
                last = schedule_[schedule_.Count - 1];
                duration = last.duration;
            }
        }
        foreach (NPCEvent e in schedule_) {
            e.SetStart(false);
        }
    }

    private bool UpdateEvent() {
        TimeData time = EnvManager.Instance.time;
        int current_time = time.hour * 60 + time.minute;
        int start = eventId_;
        bool updated = false;
        for (int i = start; i < schedule_.Count; i++) {
            if (schedule_[i].time <= current_time) {
                continue;
            }
            updated = nextEvent_ == null || eventId_ != i;
            eventId_ = i;
            break;
        }
        nextEvent_ = schedule_[eventId_];
        if (updated) {
            nextEvent_.SetStart(false);
        }
        return updated;
    }

    private float GetMoveTime(Vector3 src, Vector3 dst) {
        float distance = Vector3.Distance(src, dst);
        float delta = distance * EnvManager.Instance.minuteTick / speed_;
        return delta;
    }

    private float BuildPath(SceneName scene, Vector3 src, Vector3 dst, float dst_time) {
        // reset nodes
        FieldGrid src_grid = FieldManager.Instance.GetGrid(src, scene);
        FieldGrid dst_grid = FieldManager.Instance.GetGrid(dst, scene);
        PathNode dst_node = GetNode(scene, dst_grid);
        List<PathNode> frontier = new List<PathNode> { GetNode(scene, src_grid) };
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
                    PathNode neighbour = NodeAt(scene, node.coord.x + i, node.coord.y + j);
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
            PathNode next_node = dst_node;
            delta += GetMoveTime(dst_node.grid.GetCenter(), next_node.grid.GetCenter());
            while (next_node != null) {
                path_.Push(new NPCStep(scene, next_node.grid.GetCenter(), dst_time - delta));
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

    private void BuildNodes(SceneName scene) {
        KeyValuePair<Vector3Int, Vector3Int> scope = FieldManager.Instance.scope;
        PathNode[,] nodes = new PathNode[scope.Value.x - scope.Key.x, scope.Value.y - scope.Key.y];
        for (int i = 0; i < nodes.GetLength(0); i++) {
            for (int j = 0; j < nodes.GetLength(1); j++) {
                FieldGrid grid = FieldManager.Instance.GridAt(i, j);
                if (grid == null) {
                    continue;
                }
                nodes[i, j] = new PathNode(grid);
                nodes[i, j].penalty = grid.HasTag(FieldTag.Path) ? pathPenalty : defaultPenalty;
            }
        }
        pathNodes_[scene] = nodes;
    }

    public PathNode NodeAt(SceneName scene, int x, int y) {
        PathNode[,] nodes = pathNodes_[scene];
        if (x >= nodes.GetLength(0) || y >= nodes.GetLength(1) || x < 0 || y < 0) {
            return null;
        }
        return nodes[x, y];
    }

    public PathNode GetNode(SceneName scene, FieldGrid grid) {
        return NodeAt(scene, grid.coord.x, grid.coord.y);
    }

    private void BeforeSceneUnload(SceneName scene_name) {
        SetFreeze(true);
    }

    private void AfterSceneLoad(SceneName scene_name) {
        BuildNodes(scene_name);
        SetFreeze(false);
        ChangeStep(currentStep_);
        ShowPath(scene_name);
    }

    public void ProcessMovement() {
        TimeData time = EnvManager.Instance.time;
        SceneName current_scene = SceneController.Instance.currentScene;
        float current_minute = time.hour * 60 + time.minute;
        if (!followPath_ && path_.Count > 0) {
            NPCStep first_step = path_.Peek();
            while (first_step != null && current_minute >= first_step.minute) {
                ChangeStep(first_step);
                first_step = path_.Count > 0 ? path_.Pop() : null;
            }
            if (first_step == null) {
                return;
            }
            followPath_ = (first_step.minute - current_minute) < 1f;
        }
        if (!followPath_) {
            return;
        }
        currentEvent_ = null;
        NPCStep next_step = path_.Peek();
        if (next_step.scene != current_scene || currentStep_.scene != current_scene) {
            if ((next_step.minute - current_minute) <= 0.01f) {
                ChangeStep(path_.Pop());
            }
        } else {
            collider_.enabled = false;
            Vector3 src = transform.position;
            Vector3 dst = next_step.pos;
            float distance = Vector3.Distance(src, dst);
            if (Math.Abs(distance) < 0.05f) {
                ChangeStep(path_.Pop());
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
            collider_.enabled = true;
            UpdateAnimator(direction_, Action.Idle);
            if (nextEvent_ != null) {
                currentEvent_ = nextEvent_;
                currentEvent_.SetStart(true);
                Debug.Log("[TMINFO] should start the event " + currentEvent_);
            }
        }
    }

    private void UpdateAnimator(Direction direct, Action act) {
        animator_.SetInteger("direction", (int)direct);
        animator_.SetInteger("action", (int)act);
    }

    private IEnumerator ActionRoutine() {
        acting_ = true;
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.0f, currentEvent_.actionStart));
        int direction = UnityEngine.Random.Range(0, 4);
        UpdateAnimator((Direction)direction, currentEvent_.action);
        yield return new WaitForSeconds(currentEvent_.actionDuration);
        acting_ = false;
        UpdateAnimator((Direction)direction, Action.Idle);
    }

    // Debug only
    private void InputTest() {
        // pick item test
        if (Input.GetKeyUp(KeyCode.N)) {
            TimeData time = EnvManager.Instance.time;
            SceneName current_scene = SceneController.Instance.currentScene;
            int minute = time.hour * 60 + time.minute + 20;
            MoveTo(SceneName.FarmScene, FieldTag.Diggable, minute);
            //MoveTo(SceneName.CabinScene, FieldTag.Dropable,  minute);
            ShowPath(current_scene);
        }
    }

    private void UpdateTime(TimeType time_type, TimeData time, int delta) {
        if (time_type == TimeType.Minute && time.minute % 10 == 0 && UpdateEvent()) {
            Debug.Log("[TMINFO] update event " + nextEvent_ + " @ " + time.hour + ":" + time.minute);
            MoveTo(nextEvent_.scene, nextEvent_.fieldTag, nextEvent_.time);
            ShowPath();
        } else if (time_type == TimeType.Day) {
            MakeSchedule();
        }
    }

    private void ShowPath(SceneName scene = SceneName.CurrentScene) {
        if (scene == SceneName.CurrentScene) {
            scene = SceneController.Instance.currentScene;
        }
        List<Vector3> points = new List<Vector3>();
        foreach (NPCStep step in path_) {
            if (step.scene == scene) {
                points.Add(step.pos);
            }
        }
        FieldManager.Instance.ShowGrids(points);
    }

    public void SetFreeze(bool freeze) {
        freezed_ = freeze;
        if (freezed_) {
            UpdateAnimator(direction_, Action.Idle);
        }
    }
}

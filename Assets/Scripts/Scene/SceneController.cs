using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : Singleton<SceneController> {
    [SerializeField] private CanvasGroup fadeCanvasGroup_ = null;
    [SerializeField] private float fadeDuration_ = 1f;
    [SerializeField] private Image fadeImage_ = null;
    [SerializeField] private SceneName startScene_;
    [SerializeField] private List<SceneName> scenes_;
    private bool loading_;
    private SceneName currentScene_ = SceneName.CurrentScene;
    private Dictionary<string, ScenePortSave> scenePorts_;

    protected override void Awake() {
        base.Awake();
        scenePorts_ = new Dictionary<string, ScenePortSave>();
    }

    private IEnumerator Start() {
        // Load all scenes to save
        foreach (SceneName scene in scenes_) {
            yield return LoadSceneRoutine(scene, Player.Instance.transform.position, false);
        }
        fadeImage_.color = new Color(0f, 0f, 0f, 1f);
        fadeCanvasGroup_.alpha = 1f;
        yield return StartCoroutine(LoadSceneRoutine(startScene_, Player.Instance.transform.position));
    }

    public void LoadScene(SceneName scene_name, Vector3 spawn_pos) {
        if (!loading_) {
            StartCoroutine(LoadSceneRoutine(scene_name, spawn_pos));
        }
    }

    private IEnumerator LoadSceneRoutine(SceneName scene_name, Vector3 spawn_pos, bool fade = true) {
        loading_ = true;
        if (currentScene_ != SceneName.CurrentScene) {
            EventHandler.CallBeforeSceneUnload(currentScene_);
            if (fade) {
                yield return StartCoroutine(FadeRoutine(1f));
            }
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
        Player.Instance.transform.position = spawn_pos;
        yield return SceneManager.LoadSceneAsync(scene_name.ToString(), LoadSceneMode.Additive);
        Scene new_scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(new_scene);
        currentScene_ = scene_name;
        EventHandler.CallAfterSceneLoad(scene_name);
        if (fade) {
            yield return StartCoroutine(FadeRoutine(0f));
        }
        // add ports
        Transform holder = GameObject.FindGameObjectWithTag("ScenePorts").transform;
        foreach (ScenePort port in holder.GetComponentsInChildren<ScenePort>()) {
            string mark = currentScene_ + "_" + port.dstScene;
            if (!scenePorts_.ContainsKey(mark)) {
                scenePorts_[mark] = port.ToSavable();
            }
        }
        loading_ = false;
    }

    private IEnumerator FadeRoutine(float alpha) {
        fadeCanvasGroup_.blocksRaycasts = true;
        float speed = Mathf.Abs(fadeCanvasGroup_.alpha - alpha) / fadeDuration_;
        while (!Mathf.Approximately(fadeCanvasGroup_.alpha, alpha)) {
            fadeCanvasGroup_.alpha = Mathf.MoveTowards(fadeCanvasGroup_.alpha, alpha, speed * Time.deltaTime);
            yield return null;
        }
        fadeCanvasGroup_.blocksRaycasts = false;
    }

    public ScenePortSave FindPort(SceneName src, SceneName dst) {
        string mark = src + "_" + dst;
        if (!scenePorts_.ContainsKey(mark)) {
            return null;
        }
        return scenePorts_[mark];
    }

    public SceneName currentScene { get { return currentScene_; } }
    public SceneName startScene { get { return startScene_; } }
}

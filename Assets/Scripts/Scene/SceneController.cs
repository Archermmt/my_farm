using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : Singleton<SceneController> {
    [SerializeField] private CanvasGroup fadeCanvasGroup_ = null;
    [SerializeField] private float fadeDuration_ = 1f;
    [SerializeField] private Image fadeImage_ = null;
    [SerializeField] private SceneName startScene_;
    private bool loading_;
    private SceneName currentScene_ = SceneName.StartScene;

    private IEnumerator Start() {
        fadeImage_.color = new Color(0f, 0f, 0f, 1f);
        fadeCanvasGroup_.alpha = 1f;
        yield return StartCoroutine(LoadSceneRoutine(startScene_, Player.Instance.transform.position));
    }

    public void LoadScene(SceneName scene_name, Vector3 spawn_pos) {
        if (!loading_) {
            StartCoroutine(LoadSceneRoutine(scene_name, spawn_pos));
        }
    }

    private IEnumerator LoadSceneRoutine(SceneName scene_name, Vector3 spawn_pos) {
        loading_ = true;
        Player.Instance.Freeze();
        if (currentScene_ != SceneName.StartScene) {
            EventHandler.CallBeforeSceneUnload(currentScene_);
            yield return StartCoroutine(FadeRoutine(1f));
            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
        Player.Instance.transform.position = spawn_pos;
        yield return SceneManager.LoadSceneAsync(scene_name.ToString(), LoadSceneMode.Additive);
        Scene new_scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(new_scene);
        currentScene_ = scene_name;
        EventHandler.CallAfterSceneLoad(scene_name);
        yield return StartCoroutine(FadeRoutine(0f));
        Player.Instance.Unfreeze();
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
}

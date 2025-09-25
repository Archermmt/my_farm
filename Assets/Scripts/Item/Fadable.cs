using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fadable : Item {
    [Header("Fade")]
    [SerializeField] private float fadeInSecond_ = 0.25f;
    [SerializeField] private float fadeOutSecond_ = 0.35f;
    [SerializeField] private float fadeOutAlpha_ = 0.45f;
    [SerializeField] private List<string> fadeTargets_;

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (fadeTargets_ != null && fadeTargets_.Contains(collision.tag)) {
            Debug.Log("should handle the fade out for item " + collision);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
        if (fadeTargets_ != null && fadeTargets_.Contains(collision.tag)) {
            Debug.Log("should handle the fade in for item " + collision);
        }
    }

    public void FadeIn() {
        if (gameObject.activeSelf) {
            StartCoroutine(FadeInRoutine());
        }
    }

    public void FadeOut() {
        if (gameObject.activeSelf) {
            StartCoroutine(FadeOutRoutine());
        }
    }

    private IEnumerator FadeInRoutine() {
        float alpha = renderer_.color.a;
        float distance = 1f - alpha;
        while (1f - alpha > 0.01f) {
            alpha = alpha + distance / fadeInSecond_ * Time.deltaTime;
            renderer_.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        renderer_.color = new Color(1f, 1f, 1f, 1f);
    }

    private IEnumerator FadeOutRoutine() {
        float alpha = renderer_.color.a;
        float distance = alpha - fadeOutAlpha_;
        while (alpha - fadeOutAlpha_ > 0.01f) {
            alpha = alpha - distance / fadeOutSecond_ * Time.deltaTime;
            renderer_.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        renderer_.color = new Color(1f, 1f, 1f, fadeOutAlpha_);
    }

}

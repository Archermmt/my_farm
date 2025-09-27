using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triggerable : Item {
    [Header("Trigger.Fade")]
    [SerializeField] private List<string> fadeTargets_;
    [SerializeField] private float fadeInSecond_ = 0.25f;
    [SerializeField] private float fadeOutSecond_ = 0.35f;
    [SerializeField] private float fadeOutAlpha_ = 0.45f;

    [Header("Trigger.Nudge")]
    [SerializeField] private List<string> nudgeTargets_;
    [SerializeField] private float nudgePauseSecs_ = 0.04f;
    private bool rotating_ = false;
    private WaitForSeconds nudgePause_;

    public override void SetItem(ItemData item_data) {
        base.SetItem(item_data);
        nudgePause_ = new WaitForSeconds(nudgePauseSecs_);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision) {
        if (Nudgable(collision)) {
            if (!rotating_) {
                if (transform.position.x < collision.transform.position.x) {
                    StartCoroutine(Rotate(false));
                } else {
                    StartCoroutine(Rotate(true));
                }
            }
        } else if (Fadable(collision)) {
            StartCoroutine(FadeOutRoutine());
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision) {
        if (Nudgable(collision)) {
            if (!rotating_) {
                if (transform.position.x > collision.transform.position.x) {
                    StartCoroutine(Rotate(false));
                } else {
                    StartCoroutine(Rotate(true));
                }
            }
        } else if (Fadable(collision)) {
            StartCoroutine(FadeInRoutine());
        }
    }

    protected virtual bool Nudgable(Collider2D collision) {
        return gameObject.activeSelf && gameObject.activeInHierarchy && nudgeTargets_ != null && nudgeTargets_.Contains(collision.tag);
    }

    private IEnumerator Rotate(bool clock_wise) {
        rotating_ = true;
        if (clock_wise) {
            for (int i = 0; i < 4; i++) {
                transform.Rotate(0f, 0f, -2f);
                yield return nudgePause_;
            }
            for (int i = 0; i < 5; i++) {
                transform.Rotate(0f, 0f, 2f);
                yield return nudgePause_;
            }
            transform.Rotate(0f, 0f, -2f);
        } else {
            for (int i = 0; i < 4; i++) {
                transform.Rotate(0f, 0f, 2f);
                yield return nudgePause_;
            }
            for (int i = 0; i < 5; i++) {
                transform.Rotate(0f, 0f, -2f);
                yield return nudgePause_;
            }
            transform.Rotate(0f, 0f, 2f);
        }
        yield return nudgePause_;
        rotating_ = false;
    }

    protected virtual bool Fadable(Collider2D collision) {
        return gameObject.activeSelf && gameObject.activeInHierarchy && fadeTargets_ != null && fadeTargets_.Contains(collision.tag);
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

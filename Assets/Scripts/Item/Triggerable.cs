using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Triggerable : MonoBehaviour {
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

    private void Awake() {
        nudgePause_ = new WaitForSeconds(nudgePauseSecs_);
    }

    public void TriggerItemEnter(Collider2D collision, Item item) {
        if (Nudgable(collision, item)) {
            if (!rotating_) {
                if (transform.position.x < collision.transform.position.x) {
                    StartCoroutine(Rotate(item, false));
                } else {
                    StartCoroutine(Rotate(item, true));
                }
            }
        } else if (Fadable(collision, item)) {
            StartCoroutine(FadeOutRoutine(item));
        }
    }

    public void TriggerItemExit(Collider2D collision, Item item) {
        if (Nudgable(collision, item)) {
            if (!rotating_) {
                if (transform.position.x > collision.transform.position.x) {
                    StartCoroutine(Rotate(item, false));
                } else {
                    StartCoroutine(Rotate(item, true));
                }
            }
        } else if (Fadable(collision, item)) {
            StartCoroutine(FadeInRoutine(item));
        }
    }

    private bool Nudgable(Collider2D collision, Item item) {
        return item.HasStatus(ItemStatus.Nudgable) && gameObject.activeSelf && gameObject.activeInHierarchy && nudgeTargets_ != null && nudgeTargets_.Contains(collision.tag);
    }

    private IEnumerator Rotate(Item item, bool clock_wise) {
        rotating_ = true;
        if (clock_wise) {
            for (int i = 0; i < 4; i++) {
                item.transform.Rotate(0f, 0f, -2f);
                yield return nudgePause_;
            }
            for (int i = 0; i < 5; i++) {
                item.transform.Rotate(0f, 0f, 2f);
                yield return nudgePause_;
            }
            item.transform.Rotate(0f, 0f, -2f);
        } else {
            for (int i = 0; i < 4; i++) {
                item.transform.Rotate(0f, 0f, 2f);
                yield return nudgePause_;
            }
            for (int i = 0; i < 5; i++) {
                item.transform.Rotate(0f, 0f, -2f);
                yield return nudgePause_;
            }
            item.transform.Rotate(0f, 0f, 2f);
        }
        yield return nudgePause_;
        rotating_ = false;
    }

    private bool Fadable(Collider2D collision, Item item) {
        return item.HasStatus(ItemStatus.Fadable) && gameObject.activeSelf && gameObject.activeInHierarchy && fadeTargets_ != null && fadeTargets_.Contains(collision.tag);
    }

    private IEnumerator FadeInRoutine(Item item) {
        float alpha = item.render.color.a;
        float distance = 1f - alpha;
        while (1f - alpha > 0.01f) {
            alpha = alpha + distance / fadeInSecond_ * Time.deltaTime;
            item.render.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        item.render.color = new Color(1f, 1f, 1f, 1f);
    }

    private IEnumerator FadeOutRoutine(Item item) {
        float alpha = item.render.color.a;
        float distance = alpha - fadeOutAlpha_;
        while (alpha - fadeOutAlpha_ > 0.01f) {
            alpha = alpha - distance / fadeOutSecond_ * Time.deltaTime;
            item.render.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        item.render.color = new Color(1f, 1f, 1f, fadeOutAlpha_);
    }

}

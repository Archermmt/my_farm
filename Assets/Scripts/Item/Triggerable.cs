using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Triggerable : MonoBehaviour {
    [Header("Fade")]
    [SerializeField] private List<string> fadeTargets_;
    [SerializeField] private float fadeInSec_ = 0.25f;
    [SerializeField] private float fadeOutSec_ = 0.35f;
    [SerializeField] private float fadeTarget_ = 0.45f;
    [SerializeField] private string fadeSound_ = "";

    [Header("Nudge")]
    [SerializeField] private List<string> nudgeTargets_;
    [SerializeField] private float nudgeSec_ = 0.04f;
    [SerializeField] private string nudgeSound_ = "";

    private bool rotating_ = false;
    private WaitForSeconds nudgeWait_;

    private void Awake() {
        nudgeWait_ = new WaitForSeconds(nudgeSec_);
        fadeTargets_ ??= new List<string>();
        nudgeTargets_ ??= new List<string>();
    }

    public void TriggerItemEnter(Collider2D collision, Item item) {
        if (Nudgable(collision, item)) {
            if (!rotating_) {
                Rotate(item, transform.position.x >= collision.transform.position.x);
            }
        } else if (Fadable(collision, item)) {
            Fade(item, fadeTarget_, fadeOutSec_);
        }
    }

    public void TriggerItemExit(Collider2D collision, Item item) {
        if (Nudgable(collision, item)) {
            if (!rotating_) {
                Rotate(item, transform.position.x <= collision.transform.position.x);
            }
        } else if (Fadable(collision, item)) {
            Fade(item, 1, fadeInSec_);
        }
    }

    public bool Nudgable(Collider2D collision, Item item) {
        return item.HasStatus(ItemStatus.Nudgable) && BaseUtils.IsActive(this) && nudgeTargets_.Contains(collision.tag);
    }

    public void Rotate(Item item, bool clock_wise, float angle = 2) {
        StartCoroutine(RotateRoutine(item, clock_wise, angle));
    }

    private IEnumerator RotateRoutine(Item item, bool clock_wise, float angle) {
        rotating_ = true;
        if (nudgeSound_.Length > 0) {
            AudioManager.Instance.PlaySound(nudgeSound_);
        }
        if (clock_wise) {
            for (int i = 0; i < 4; i++) {
                item.transform.Rotate(0f, 0f, -angle);
                yield return nudgeWait_;
            }
            for (int i = 0; i < 5; i++) {
                item.transform.Rotate(0f, 0f, angle);
                yield return nudgeWait_;
            }
            item.transform.Rotate(0f, 0f, -angle);
        } else {
            for (int i = 0; i < 4; i++) {
                item.transform.Rotate(0f, 0f, angle);
                yield return nudgeWait_;
            }
            for (int i = 0; i < 5; i++) {
                item.transform.Rotate(0f, 0f, -angle);
                yield return nudgeWait_;
            }
            item.transform.Rotate(0f, 0f, angle);
        }
        yield return nudgeWait_;
        rotating_ = false;
    }

    public bool Fadable(Collider2D collision, Item item) {
        return item.HasStatus(ItemStatus.Fadable) && BaseUtils.IsActive(this) && fadeTargets_.Contains(collision.tag);
    }

    public void Fade(Item item, float target, float duration) {
        StartCoroutine(FadeRoutine(item, target, duration));
    }

    private IEnumerator FadeRoutine(Item item, float target, float duration) {
        if (fadeSound_.Length > 0) {
            AudioManager.Instance.PlaySound(fadeSound_);
        }
        float alpha = item.render.color.a;
        float distance = target - alpha;
        while (Math.Abs(target - alpha) > 0.01f) {
            alpha = alpha + distance / duration * Time.deltaTime;
            item.render.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        item.render.color = new Color(1f, 1f, 1f, target);
    }
}

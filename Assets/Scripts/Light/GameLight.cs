using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class LightingPeriod {
    public int hour;
    public float intensity;
}

[Serializable]
public class LightingSchedule {
    public Season season;
    public List<SceneName> scenes;
    public List<LightingPeriod> periods;
}

[RequireComponent(typeof(Light2D))]
public class GameLight : MonoBehaviour {
    [SerializeField] private List<LightingSchedule> lightSchs_;
    [SerializeField] private float lightSec_ = 20f;
    [SerializeField][Range(0f, 1f)] private float flicker_ = 0f;
    [SerializeField][Range(0f, 0.2f)] private float flickerMin_;
    [SerializeField][Range(0f, 0.2f)] private float flickerMax_;
    private Light2D light_;
    private LightingSchedule currentSch_;
    private float intensity_;
    private float flickIntensity_ = 0f;
    private float flickerTimer_ = 0f;

    private void Awake() {
        light_ = GetComponent<Light2D>();
    }

    private void Start() {
        currentSch_ = GetSchedule();
    }

    private void FixedUpdate() {
        if (flicker_ > 0) {
            flickerTimer_ -= Time.deltaTime;
            if (flickerTimer_ <= 0) {
                LightFlicker(flickIntensity_);
            } else {
                light_.intensity = flickIntensity_;
            }
        }
    }

    private void OnEnable() {
        EventHandler.UpdateTimeEvent += UpdateTime;
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable() {
        EventHandler.UpdateTimeEvent -= UpdateTime;
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    private float GetIntensity(LightingSchedule sch, int hour) {
        float intensity = sch.periods[sch.periods.Count - 1].intensity;
        for (int i = 1; i < sch.periods.Count; i++) {
            if (sch.periods[i - 1].hour < hour && sch.periods[i].hour >= hour) {
                int diff_hour = sch.periods[i].hour - sch.periods[i - 1].hour;
                float diff_intensity = sch.periods[i].intensity - sch.periods[i - 1].intensity;
                intensity = sch.periods[i - 1].intensity + diff_intensity * (hour - sch.periods[i - 1].hour) / diff_hour;
                break;
            }
        }
        return intensity;
    }

    private IEnumerator LightRoutine(Light2D light, float duration, float start, float end) {
        float distance = end - start;
        while (Math.Abs(end - start) > 0.01f) {
            start = start + distance / duration * Time.deltaTime;
            if (flicker_ > 0) {
                flickIntensity_ = start;
            } else {
                light.intensity = start;
            }
            yield return null;
        }
        light.intensity = end;
        flickIntensity_ = end;
    }

    private LightingSchedule GetSchedule() {
        TimeData time = EnvManager.Instance.time;
        SceneName scene_name = SceneController.Instance.currentScene;
        foreach (LightingSchedule schedule in lightSchs_) {
            if (schedule.season == time.season && schedule.scenes.Contains(scene_name)) {
                return schedule;
            }
        }
        return lightSchs_[0];
    }

    public void LightFlicker(float intensity) {
        light_.intensity = UnityEngine.Random.Range(intensity, intensity + (intensity * flicker_));
        flickerTimer_ = UnityEngine.Random.Range(flickerMin_, flickerMax_);
    }

    private void UpdateTime(TimeType time_type, TimeData time, int delta) {
        if (time_type == TimeType.Hour) {
            float start = intensity_;
            intensity_ = GetIntensity(currentSch_, time.hour + delta);
            StartCoroutine(LightRoutine(light_, lightSec_, start, intensity_));
        } else if (time_type == TimeType.Month) {
            currentSch_ = GetSchedule();
        }
    }

    private void AfterSceneLoad(SceneName scene_name) {
        currentSch_ = GetSchedule();
        TimeData time = EnvManager.Instance.time;
        intensity_ = GetIntensity(currentSch_, time.hour);
        light_.intensity = intensity_;
        flickIntensity_ = intensity_;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public class TimeData {
    public int year = 2025;
    public int month = 0;
    public int weekDay = 0;
    public int day = 0;
    public int hour = 6;
    public int minute = 0;
    public int second = 0;

    public Season season {
        get {
            if (month >= 9) return Season.Winter;
            if (month >= 6) return Season.Autumn;
            if (month >= 3) return Season.Summer;
            return Season.Spring;
        }
    }
}

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

public class EnvManager : Singleton<EnvManager> {
    [Header("Time")]
    [SerializeField] private Clock clock_;
    [SerializeField] private TimeData time_;
    [SerializeField] private float speepUp_ = 48f;

    [Header("Light")]
    [SerializeField] private List<LightingSchedule> sunLightSchs_;
    [SerializeField] private float sunLightSec_ = 20f;
    private bool freezed_ = false;
    // time
    private float gameTick_ = 0f;
    private float minuteTick_;
    // light
    private Light2D sunLight_;
    private LightingSchedule sunLightSch_;
    // scene
    private SceneName currentScene_ = SceneName.StartScene;

    protected override void Awake() {
        base.Awake();
        minuteTick_ = 60 / speepUp_;
        sunLightSch_ = GetSunLightSch();
    }

    private void OnEnable() {
        EventHandler.BeforeSceneUnloadEvent += BeforeSceneUnload;
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable() {
        EventHandler.BeforeSceneUnloadEvent -= BeforeSceneUnload;
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    private void Start() {
        clock_.ShowTime(time_);
    }

    private void FixedUpdate() {
        if (!freezed_) {
            gameTick_ += Time.deltaTime;
            if (gameTick_ >= minuteTick_) {
                gameTick_ -= minuteTick_;
                UpdateMinute();
            }
        }
    }

    public void UpdateYear(int delta = 1) {
        EventHandler.CallUpdateTime(TimeType.Year, time_, delta);
        time_.year += delta;
    }

    public void UpdateMonth(int delta = 1) {
        EventHandler.CallUpdateTime(TimeType.Month, time_, delta);
        int left = delta;
        while (time_.month + left > 12) {
            UpdateYear(1);
            left -= 12;
        }
        time_.month += left;
        sunLightSch_ = GetSunLightSch();
    }

    public void UpdateDay(int delta = 1) {
        EventHandler.CallUpdateTime(TimeType.Day, time_, delta);
        int left = delta;
        time_.weekDay = (time_.weekDay + left) % 7;
        while (time_.day + left > 30) {
            UpdateMonth(1);
            left -= 30;
        }
        time_.day += left;
    }

    public void UpdateHour(int delta = 1) {
        EventHandler.CallUpdateTime(TimeType.Hour, time_, delta);
        int left = delta;
        while (time_.hour + left > 24) {
            UpdateDay(1);
            left -= 24;
        }
        time_.hour += left;
        StartCoroutine(LightRoutine(sunLight_, sunLightSch_, sunLightSec_));
    }

    public void UpdateMinute(int delta = 1) {
        int left = delta;
        while (time_.minute + left > 60) {
            UpdateHour(1);
            left -= 60;
        }
        time_.minute += left;
        if (time_.minute % 5 == 0) {
            EventHandler.CallUpdateTime(TimeType.Minute, time_, delta);
            clock_.ShowTime(time_);
        }
    }

    public void UpdateSecond(int delta = 1) {
        int left = delta;
        while (time_.second + left > 60) {
            UpdateMinute(1);
            left -= 60;
        }
        time_.second += left;
    }

    public void SetTime(TimeType time_type, int value) {
        switch (time_type) {
            case TimeType.Year:
                time_.year = value;
                break;
            case TimeType.Month:
                time_.month = value;
                break;
            case TimeType.Day:
                int delta = value - time_.day;
                time_.weekDay = (time_.weekDay + delta) % 7;
                time_.day = value;
                break;
            case TimeType.Hour:
                time_.hour = value;
                break;
            case TimeType.Minute:
                time_.minute = value;
                break;
            case TimeType.Second:
                time_.second = value;
                break;
            default:
                break;
        }
    }

    private LightingSchedule GetSunLightSch() {
        foreach (LightingSchedule schedule in sunLightSchs_) {
            if (schedule.season == time_.season && schedule.scenes.Contains(currentScene_)) {
                return schedule;
            }
        }
        return sunLightSchs_[0];
    }

    private float GetIntensity(LightingSchedule sch) {
        float intensity = sch.periods[sch.periods.Count - 1].intensity;
        for (int i = 1; i < sch.periods.Count; i++) {
            if (sch.periods[i - 1].hour < time_.hour && sch.periods[i].hour >= time_.hour) {
                int diff_hour = sch.periods[i].hour - sch.periods[i - 1].hour;
                float diff_intensity = sch.periods[i].intensity - sch.periods[i - 1].intensity;
                intensity = sch.periods[i - 1].intensity + diff_intensity * (time_.hour - sch.periods[i - 1].hour) / diff_hour;
                break;
            }
        }
        return intensity;
    }

    private IEnumerator LightRoutine(Light2D light, LightingSchedule sch, float duration) {
        float intensity = GetIntensity(sch);
        float distance = intensity - light.intensity;
        while (Math.Abs(intensity - light.intensity) > 0.01f) {
            light.intensity = light.intensity + distance / duration * Time.deltaTime;
            yield return null;
        }
        light.intensity = intensity;
    }

    private void BeforeSceneUnload(SceneName scene_name) {
        SetFreeze(true);
    }

    private void AfterSceneLoad(SceneName scene_name) {
        currentScene_ = scene_name;
        sunLightSch_ = GetSunLightSch();
        sunLight_ = GameObject.FindGameObjectWithTag("SunLight").GetComponent<Light2D>();
        sunLight_.intensity = GetIntensity(sunLightSch_);
        UpdateMinute(0);
        SetFreeze(false);
    }

    public void SetFreeze(bool freeze) {
        freezed_ = freeze;
    }

    public Clock clock { get { return clock_; } }
}

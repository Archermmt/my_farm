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
    [SerializeField] private float speedUp_ = 48f;

    [Header("Light")]
    [SerializeField] private List<LightingSchedule> sunLightSchs_;
    [SerializeField] private float sunLightSec_ = 20f;
    private bool freezed_ = true;
    // time
    private float gameTick_ = 0f;
    private float minuteTick_;
    // light
    private Light2D sunLight_;
    private LightingSchedule sunLightSch_;

    protected override void Awake() {
        base.Awake();
        minuteTick_ = 60 / speedUp_;
    }

    private void OnEnable() {
        EventHandler.AfterSceneLoadEvent += AfterSceneLoad;
    }

    private void OnDisable() {
        EventHandler.AfterSceneLoadEvent -= AfterSceneLoad;
    }

    private void Start() {
        clock_.ShowTime(time_);
        sunLightSch_ = GetSunLightSch();
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

    public TimeData UpdateYear(int delta = 1) {
        EventHandler.CallUpdateTime(TimeType.Year, time_, delta);
        time_.year += delta;
        return time_;
    }

    public TimeData UpdateMonth(int delta = 1) {
        EventHandler.CallUpdateTime(TimeType.Month, time_, delta);
        time_.month += delta;
        while (time_.month >= 12) {
            UpdateYear(1);
            time_.month -= 12;
        }
        sunLightSch_ = GetSunLightSch();
        return time_;
    }

    public TimeData UpdateDay(int delta = 1) {
        EventHandler.CallUpdateTime(TimeType.Day, time_, delta);
        time_.day += delta;
        time_.weekDay = (time_.weekDay + delta) % 7;
        while (time_.day >= 30) {
            UpdateMonth(1);
            time_.day -= 30;
        }
        return time_;
    }

    public TimeData UpdateHour(int delta = 1) {
        EventHandler.CallUpdateTime(TimeType.Hour, time_, delta);
        time_.hour += delta;
        while (time_.hour >= 24) {
            UpdateDay(1);
            time_.hour -= 24;
        }
        StartCoroutine(LightRoutine(sunLight_, sunLightSch_, sunLightSec_));
        return time_;
    }

    public TimeData UpdateMinute(int delta = 1) {
        time_.minute += delta;
        while (time_.minute >= 60) {
            UpdateHour(1);
            time_.minute -= 60;
        }
        if (time_.minute % 5 == 0) {
            EventHandler.CallUpdateTime(TimeType.Minute, time_, delta);
            clock_.ShowTime(time_);
        }
        return time_;
    }

    public TimeData UpdateSecond(int delta = 1) {
        time_.second += delta;
        while (time_.second >= 60) {
            UpdateMinute(1);
            time_.second -= 60;
        }
        return time_;
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
        SceneName scene_name = SceneController.Instance.currentScene;
        foreach (LightingSchedule schedule in sunLightSchs_) {
            if (schedule.season == time_.season && schedule.scenes.Contains(scene_name)) {
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

    private void AfterSceneLoad(SceneName scene_name) {
        sunLightSch_ = GetSunLightSch();
        sunLight_ = GameObject.FindGameObjectWithTag("SunLight").GetComponent<Light2D>();
        sunLight_.intensity = GetIntensity(sunLightSch_);
        if (!freezed_) {
            UpdateMinute(0);
        }
    }

    public void SetFreeze(bool freeze) {
        freezed_ = freeze;
    }

    public TimeData time { get { return time_; } }
    public Clock clock { get { return clock_; } }
    public float minuteTick { get { return minuteTick_; } }
}

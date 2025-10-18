using System;
using System.Collections;
using System.Collections.Generic;
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

public class EnvManager : Singleton<EnvManager> {
    [Header("Time")]
    [SerializeField] private Clock clock_;
    [SerializeField] private TimeData time_;
    [SerializeField] private float speedUp_ = 48f;
    private bool freezed_ = true;
    // time
    private float gameTick_ = 0f;
    private float minuteTick_;

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
    }

    private void Update() {
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

    private void AfterSceneLoad(SceneName scene_name) {
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

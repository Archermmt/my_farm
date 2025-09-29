using System;
using UnityEngine;

[Serializable]
public class TimeData {
    public int year;
    public int month;
    public int week;
    public int day;
    public int hour;
    public int minute;
    public int second;
}

public class Clock : MonoBehaviour {
    [SerializeField] private TimeData time_;
    [SerializeField] private float timeSpeepUp_ = 1 / 0.006f;
    private float gameTick_ = 0f;
    private float tickPerMinute_;
    private bool freezed_ = false;

    private void Awake() {
        tickPerMinute_ = 60 / timeSpeepUp_;
    }

    private void FixedUpdate() {
        if (!freezed_) {
            gameTick_ += Time.deltaTime;
            if (gameTick_ >= tickPerMinute_) {
                gameTick_ -= tickPerMinute_;
                UpdateMinute();
            }
        }
    }

    public void UpdateYear(int delta = 1) {
        time_.year += delta;
    }

    public void UpdateMonth(int delta = 1) {
        int left = delta;
        while (time_.month + left > 12) {
            UpdateYear(1);
            left -= 12;
        }
        time_.month += left;
    }

    public void UpdateDay(int delta = 1) {
        int left = delta;
        time_.week = (time_.week + left) % 7;
        while (time_.day + left > 30) {
            UpdateMonth(1);
            left -= 30;
        }
        time_.day += left;
    }

    public void UpdateHour(int delta = 1) {
        int left = delta;
        while (time_.hour + left > 24) {
            UpdateDay(1);
            left -= 24;
        }
        time_.hour += left;
    }

    public void UpdateMinute(int delta = 1) {
        int left = delta;
        while (time_.minute + left > 60) {
            UpdateHour(1);
            left -= 60;
        }
        time_.minute += left;
    }

    public void UpdateSecond(int delta = 1) {
        int left = delta;
        while (time_.second + left > 60) {
            UpdateMinute(1);
            left -= 60;
        }
        time_.second += left;
    }

    public void Freeze() {
        freezed_ = true;
    }

    public void Unfreeze() {
        freezed_ = false;
    }

    public TimeData time { get { return time_; } }

    public Season season {
        get {
            if (time_.month >= 9) return Season.Winter;
            if (time_.month >= 6) return Season.Autumn;
            if (time_.month >= 3) return Season.Summer;
            return Season.Spring;
        }
    }
}

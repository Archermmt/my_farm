using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class TimeData {
    public int year;
    public int month;
    public int weekDay;
    public int day;
    public int hour;
    public int minute;
    public int second;
}

public class Clock : MonoBehaviour {
    [SerializeField] private TimeData time_;
    [SerializeField] private TextMeshProUGUI year_;
    [SerializeField] private TextMeshProUGUI month_;
    [SerializeField] private TextMeshProUGUI day_;
    [SerializeField] private TextMeshProUGUI hour_;
    [SerializeField] private float timeSpeepUp_ = 1 / 0.006f;
    private List<string> monthNames_;
    private List<string> weekDayNames_;
    private float gameTick_ = 0f;
    private float tickPerMinute_;
    private bool freezed_ = false;

    private void Awake() {
        tickPerMinute_ = 60 / timeSpeepUp_;
        monthNames_ = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        weekDayNames_ = new List<string> { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
    }

    private void FixedUpdate() {
        if (!freezed_) {
            gameTick_ += Time.deltaTime;
            if (gameTick_ >= tickPerMinute_) {
                gameTick_ -= tickPerMinute_;
                UpdateMinute();
                year_.text = time_.year.ToString();
                month_.text = monthNames_[time.month] + "." + season.ToString();
                if (time_.day == 0) {
                    day_.text = "1st";
                } else if (time_.day == 1) {
                    day_.text = "2nd";
                } else if (time_.day == 3) {
                    day_.text = "3rd";
                } else {
                    day_.text = time_.day + "th";
                }
                day_.text += "." + weekDayNames_[time_.weekDay];
                if (time_.minute < 10) {
                    hour_.text = time_.hour + ":0" + time_.minute;
                } else {
                    hour_.text = time_.hour + ":" + time_.minute;
                }
            }
        }
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
    }

    public void UpdateMinute(int delta = 1) {
        EventHandler.CallUpdateTime(TimeType.Minute, time_, delta);
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

    public void SetFreeze(bool freeze) {
        freezed_ = freeze;
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

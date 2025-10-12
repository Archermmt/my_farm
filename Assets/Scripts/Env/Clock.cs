using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Clock : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI year_;
    [SerializeField] private TextMeshProUGUI month_;
    [SerializeField] private TextMeshProUGUI day_;
    [SerializeField] private TextMeshProUGUI hour_;
    private List<string> monthNames_;
    private List<string> weekDayNames_;

    private void Awake() {
        monthNames_ = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        weekDayNames_ = new List<string> { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
    }

    public void ShowTime(TimeData time) {
        year_.text = time.year.ToString();
        month_.text = monthNames_[time.month] + "." + time.season.ToString();
        if (time.day == 0) {
            day_.text = "1st";
        } else if (time.day == 1) {
            day_.text = "2nd";
        } else if (time.day == 3) {
            day_.text = "3rd";
        } else {
            day_.text = time.day + "th";
        }
        day_.text += "." + weekDayNames_[time.weekDay];
        if (time.minute < 10) {
            hour_.text = time.hour + ":0" + time.minute;
        } else {
            hour_.text = time.hour + ":" + time.minute;
        }
    }
}

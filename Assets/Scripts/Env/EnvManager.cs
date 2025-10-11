using UnityEngine;

public class EnvManager : Singleton<EnvManager> {
    [SerializeField] private Clock clock_;

    public void UpdateTime(TimeType time_type, int delta) {
        EventHandler.CallUpdateTime(time_type, clock_.time, delta);
        switch (time_type) {
            case TimeType.Year:
                clock_.UpdateYear(delta);
                break;
            case TimeType.Month:
                clock_.UpdateMonth(delta);
                break;
            case TimeType.Day:
                clock_.UpdateDay(delta);
                break;
            case TimeType.Hour:
                clock_.UpdateHour(delta);
                break;
            case TimeType.Minute:
                clock_.UpdateMinute(delta);
                break;
            case TimeType.Second:
                clock_.UpdateSecond(delta);
                break;
            default:
                break;
        }
    }
}

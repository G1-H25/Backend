
public static class TrackingTimeOutsideRange
{
    public static (int timeOutside, DateTime? timerStart) TrackTimeOutsideRange(
        float currentValue,
        float expectedMin,
        float expectedMax,
        DateTime currentTimestamp,
        DateTime? lastTimerStart)
    {
        bool isOutOfRange = currentValue < expectedMin || currentValue > expectedMax;

        if (isOutOfRange)
        {
            DateTime start = lastTimerStart ?? currentTimestamp;
            int seconds = (int)(currentTimestamp - start).TotalSeconds;
            return (seconds, start);
        }

        return (0, null); // back in range
    }
}

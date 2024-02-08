namespace ElAd2024.Helpers;
public static class TimerHelper
{
    // Start the timer
    public static void ResumeTimer(Timer timer, int dueTime = 0, int period = 100)
        => timer.Change(dueTime, period);

    // Stop the timer
    public static void PauseTimer(Timer timer)
        => timer?.Change(Timeout.Infinite, 0);

}

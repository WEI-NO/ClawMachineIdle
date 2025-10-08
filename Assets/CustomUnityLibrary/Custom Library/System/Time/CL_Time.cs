using UnityEngine;

namespace CustomLibrary.Time
{
    public struct TimeFormatter
    {

        public static string TimeToDisplay(float seconds)
        {
            int totalSeconds = Mathf.FloorToInt(seconds); // remove decimals
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int secs = totalSeconds % 60;

            return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, secs);
        }

    }
}
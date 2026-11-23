using System;

namespace TAU.President.Core
{
    /// <summary>
    /// Pure, zero-based five-day calendar rules for C0-v2. Does not mutate GameState
    /// or advance the campaign; only CommandProcessor may commit a day transition.
    /// </summary>
    public static class GameClock
    {
        public const int DaysPerWeek = 5;

        public static int DayNumber(int dayIndex)
        {
            Validate(dayIndex);
            return checked(dayIndex + 1);
        }

        public static int WeekIndex(int dayIndex)
        {
            Validate(dayIndex);
            return dayIndex / DaysPerWeek;
        }

        public static int WeekDayIndex(int dayIndex)
        {
            Validate(dayIndex);
            return dayIndex % DaysPerWeek;
        }

        public static bool IsWeekClosingDay(int dayIndex)
        {
            return WeekDayIndex(dayIndex) == DaysPerWeek - 1;
        }

        /// <summary>
        /// Index of the next weekly settlement day, including today if it closes a week.
        /// Throws rather than overflowing the Int32 day-index domain.
        /// </summary>
        public static int NextWeekSettlementDayIndex(int dayIndex)
        {
            Validate(dayIndex);
            int remaining = (DaysPerWeek - 1) - WeekDayIndex(dayIndex);
            return checked(dayIndex + remaining);
        }

        private static void Validate(int dayIndex)
        {
            if (dayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(dayIndex), "DayIndex must be zero-based.");
        }
    }
}

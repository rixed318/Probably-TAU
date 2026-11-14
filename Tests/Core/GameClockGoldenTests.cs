using System;
using TAU.President.Core;

namespace TAU.President.Tests
{
    /// <summary>Standalone C0-v2 calendar tests; invoke RunAll from a future EditMode wrapper.</summary>
    public static class GameClockGoldenTests
    {
        public static void RunAll()
        {
            for (int day = 0; day < 1000; day++)
            {
                int week = day / 5;
                int weekday = day % 5;
                Require(GameClock.DayNumber(day) == day + 1, "DayNumber " + day);
                Require(GameClock.WeekIndex(day) == week, "WeekIndex " + day);
                Require(GameClock.WeekDayIndex(day) == weekday, "WeekDayIndex " + day);
                Require(GameClock.IsWeekClosingDay(day) == (weekday == 4), "Closing " + day);
                Require(GameClock.NextWeekSettlementDayIndex(day) == week * 5 + 4, "Next settlement " + day);
            }

            Require(GameClock.DayNumber(0) == 1, "First UI day");
            Require(GameClock.IsWeekClosingDay(4), "First settlement closes DayIndex 4");
            Require(GameClock.IsWeekClosingDay(9), "Second settlement closes DayIndex 9");
            Require(!GameClock.IsWeekClosingDay(5), "DayIndex 5 is not settlement");
            ExpectArgument(() => GameClock.DayNumber(-1));
            ExpectArgument(() => GameClock.WeekIndex(-1));
            ExpectArgument(() => GameClock.WeekDayIndex(-1));
            ExpectArgument(() => GameClock.IsWeekClosingDay(-1));
            ExpectArgument(() => GameClock.NextWeekSettlementDayIndex(-1));
            ExpectOverflow(() => GameClock.DayNumber(int.MaxValue));
            ExpectOverflow(() => GameClock.NextWeekSettlementDayIndex(int.MaxValue));
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void ExpectArgument(Action action)
        {
            try { action(); }
            catch (ArgumentOutOfRangeException) { return; }
            throw new InvalidOperationException("Expected ArgumentOutOfRangeException.");
        }

        private static void ExpectOverflow(Action action)
        {
            try { action(); }
            catch (OverflowException) { return; }
            throw new InvalidOperationException("Expected OverflowException.");
        }
    }
}

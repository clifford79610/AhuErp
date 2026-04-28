using System;
using AhuErp.Core.Services;
using Xunit;

namespace AhuErp.Tests
{
    /// <summary>
    /// Edge cases для <see cref="WorkingDaysCalendar"/>: пятидневка,
    /// перенесённые выходные, праздники, нулевой/отрицательный срок.
    /// </summary>
    public class WorkingDaysCalendarTests
    {
        [Fact]
        public void AddWorkingDays_skips_weekend()
        {
            var cal = WorkingDaysCalendar.FiveDayWeek();
            // Пятница 2026-04-24 + 1 рабочий день -> понедельник 2026-04-27.
            var friday = new DateTime(2026, 4, 24);
            Assert.Equal(new DateTime(2026, 4, 27), cal.AddWorkingDays(friday, 1).Date);
        }

        [Fact]
        public void AddWorkingDays_skips_holiday_inside_week()
        {
            var cal = new WorkingDaysCalendar(
                weekend: new[] { DayOfWeek.Saturday, DayOfWeek.Sunday },
                holidays: new[] { new DateTime(2026, 5, 1) }, // праздник в пятницу
                workingWeekends: null);
            // Четверг 2026-04-30 + 2 раб.дня: пятница 1 мая выходная (праздник),
            // суббота-воскресенье — выходные, понедельник 4 мая первый рабочий,
            // вторник 5 мая — второй рабочий.
            var thursday = new DateTime(2026, 4, 30);
            Assert.Equal(new DateTime(2026, 5, 5), cal.AddWorkingDays(thursday, 2).Date);
        }

        [Fact]
        public void AddWorkingDays_uses_workingWeekends_override()
        {
            var cal = new WorkingDaysCalendar(
                weekend: new[] { DayOfWeek.Saturday, DayOfWeek.Sunday },
                holidays: null,
                workingWeekends: new[] { new DateTime(2026, 4, 25) }); // суббота — рабочая
            // Пятница 2026-04-24 + 1 раб.день -> суббота 2026-04-25 (subbotnik).
            var friday = new DateTime(2026, 4, 24);
            Assert.Equal(new DateTime(2026, 4, 25), cal.AddWorkingDays(friday, 1).Date);
        }

        [Fact]
        public void AddWorkingDays_zero_or_negative_returns_start_unchanged()
        {
            var cal = WorkingDaysCalendar.FiveDayWeek();
            var d = new DateTime(2026, 4, 24, 13, 0, 0);
            Assert.Equal(d, cal.AddWorkingDays(d, 0));
            Assert.Equal(d, cal.AddWorkingDays(d, -3));
        }

        [Fact]
        public void AddWorkingDays_preserves_time_of_day()
        {
            var cal = WorkingDaysCalendar.FiveDayWeek();
            var friday = new DateTime(2026, 4, 24, 17, 30, 0);
            var result = cal.AddWorkingDays(friday, 1);
            Assert.Equal(new TimeSpan(17, 30, 0), result.TimeOfDay);
        }

        [Fact]
        public void IsWorkingDay_identifies_weekend_and_holiday()
        {
            var cal = new WorkingDaysCalendar(
                weekend: new[] { DayOfWeek.Saturday, DayOfWeek.Sunday },
                holidays: new[] { new DateTime(2026, 1, 1) },
                workingWeekends: null);
            Assert.False(cal.IsWorkingDay(new DateTime(2026, 4, 25))); // суббота
            Assert.False(cal.IsWorkingDay(new DateTime(2026, 1, 1)));  // праздник
            Assert.True(cal.IsWorkingDay(new DateTime(2026, 4, 27)));  // понедельник
        }

        [Fact]
        public void CountWorkingDays_returns_zero_when_to_le_from()
        {
            var cal = WorkingDaysCalendar.FiveDayWeek();
            Assert.Equal(0, cal.CountWorkingDays(
                new DateTime(2026, 4, 27),
                new DateTime(2026, 4, 27)));
            Assert.Equal(0, cal.CountWorkingDays(
                new DateTime(2026, 4, 28),
                new DateTime(2026, 4, 27)));
        }

        [Fact]
        public void CountWorkingDays_full_week()
        {
            var cal = WorkingDaysCalendar.FiveDayWeek();
            // [Понедельник 27 апреля .. Понедельник 4 мая) — 5 рабочих дней
            Assert.Equal(5, cal.CountWorkingDays(
                new DateTime(2026, 4, 27),
                new DateTime(2026, 5, 4)));
        }
    }
}

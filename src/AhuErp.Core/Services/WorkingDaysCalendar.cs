using System;
using System.Collections.Generic;
using System.Linq;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Календарь рабочих дней. Используется при расчёте срока поручений
    /// и этапов согласования: <c>DueAt = AddWorkingDays(start, step.DurationDays)</c>.
    /// «По умолчанию» суббота и воскресенье — выходные; список государственных
    /// праздников передаётся явно (или загружается из НСИ — расширим в Phase 13).
    /// Реализация — чистая, без обращения к репозиториям, чтобы быть
    /// тестируемой и пригодной для серверного и клиентского кода.
    /// </summary>
    public sealed class WorkingDaysCalendar
    {
        private readonly HashSet<DateTime> _holidays;
        private readonly HashSet<DateTime> _workingWeekends;
        private readonly DayOfWeek[] _weekend;

        /// <summary>
        /// Дефолтная пятидневка с пустым списком праздников. Удобно для тестов.
        /// </summary>
        public static WorkingDaysCalendar FiveDayWeek() =>
            new WorkingDaysCalendar(
                weekend: new[] { DayOfWeek.Saturday, DayOfWeek.Sunday },
                holidays: null,
                workingWeekends: null);

        public WorkingDaysCalendar(
            IEnumerable<DayOfWeek> weekend,
            IEnumerable<DateTime> holidays,
            IEnumerable<DateTime> workingWeekends)
        {
            _weekend = weekend != null
                ? new HashSet<DayOfWeek>(weekend).ToArray()
                : new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
            _holidays = holidays != null
                ? new HashSet<DateTime>(NormalizeAll(holidays))
                : new HashSet<DateTime>();
            _workingWeekends = workingWeekends != null
                ? new HashSet<DateTime>(NormalizeAll(workingWeekends))
                : new HashSet<DateTime>();
        }

        /// <summary>
        /// True, если <paramref name="date"/> — рабочий день. Учитывает
        /// перенесённые выходные (<see cref="_workingWeekends"/>).
        /// </summary>
        public bool IsWorkingDay(DateTime date)
        {
            var d = date.Date;
            if (_workingWeekends.Contains(d)) return true;
            if (_holidays.Contains(d)) return false;
            foreach (var w in _weekend) if (d.DayOfWeek == w) return false;
            return true;
        }

        /// <summary>
        /// Прибавляет <paramref name="workingDays"/> рабочих дней к
        /// <paramref name="start"/>. Если <paramref name="workingDays"/>
        /// неположительно — возвращает <paramref name="start"/> как есть
        /// (договорённость: «срок 0 дней» = «к концу того же дня»).
        /// </summary>
        public DateTime AddWorkingDays(DateTime start, int workingDays)
        {
            if (workingDays <= 0) return start;
            var cursor = start;
            int added = 0;
            while (added < workingDays)
            {
                cursor = cursor.AddDays(1);
                if (IsWorkingDay(cursor)) added++;
            }
            // Сохраняем время суток исходной даты, но возвращаем уже новый день.
            return new DateTime(cursor.Year, cursor.Month, cursor.Day,
                start.Hour, start.Minute, start.Second, start.Kind);
        }

        /// <summary>
        /// Считает количество полных рабочих дней между двумя датами
        /// (полуинтервал [from..to)). Если <paramref name="to"/> &lt;=
        /// <paramref name="from"/> — возвращает 0.
        /// </summary>
        public int CountWorkingDays(DateTime from, DateTime to)
        {
            if (to <= from) return 0;
            var cursor = from.Date;
            var end = to.Date;
            int count = 0;
            while (cursor < end)
            {
                if (IsWorkingDay(cursor)) count++;
                cursor = cursor.AddDays(1);
            }
            return count;
        }

        private static IEnumerable<DateTime> NormalizeAll(IEnumerable<DateTime> source)
        {
            foreach (var d in source) yield return d.Date;
        }
    }
}

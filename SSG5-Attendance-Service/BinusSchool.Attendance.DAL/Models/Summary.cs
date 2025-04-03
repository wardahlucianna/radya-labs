using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.AttendanceDb.Entities;

namespace BinusSchool.Persistence.AttendanceDb.Models
{
    public class Summary
    {
        public Summary()
        {
            EntryStatus = AttendanceEntryStatus.Unsubmitted;
            Workhabits = new List<WorkHabit>();
        }

        public string Id { get; set; }
        public string IdGrade { get; set; }
        public string IdLesson { get; set; }
        public string IdStudent { get; set; }
        public string IdHomeroom { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdSubject { get; set; }
        public string IdSession { get; set; }
        public DateTime ScheduleDt { get; set; }
        public DateTime StartPeriod { get; set; }
        public DateTime EndPeriod { get; set; }
        public AttendanceEntryStatus EntryStatus { get; set; }
        public string IdAttendanceMappingAttendance { get; set; }
        public List<WorkHabit> Workhabits { get; }

        #region Helper fields

        /// <summary>
        /// As IdTerm
        /// </summary>
        public string IdPeriod { get; set; }

        public int Term { get; set; }
        public int Semester { get; set; }
        public string IdSchool { get; set; }

        #endregion
    }

    public static class SummaryExtensions
    {
        public static void Mapping(this Summary data, List<PeriodDto> periods)
        {
            var period =
                periods.FirstOrDefault(e => e.PeriodStartDt <= data.ScheduleDt && data.ScheduleDt <= e.PeriodEndDt);
            if (period == null) return;
            //throw new Exception($"Data period not found by IdGenLesson {data.Id}, IdGrade {data.IdGrade}");

            data.IdPeriod = period.IdPeriod;
            data.Term = Convert.ToInt32(Regex.Match(period.PeriodCode, @"\d+").Value);
            data.Semester = period.PeriodSemester;
        }
    }

    public class WorkHabit
    {
        /// <summary>
        /// only used when get list
        /// </summary>
        public string IdEntry { get; set; }
        public string IdEntryWorkHabit { get; set; }
        public string IdWorkHabit { get; set; }
        public string IdMappingAttendanceWorkHabit { get; set; }
        public string IdMappingAttendance { get; set; }
        public string IdLevel { get; set; }
        public AbsentTerm? AbsentTerm { get; set; }
        public bool IsNeedValidation { get; set; }
        public bool IsUseWorkHabit { get; set; }
        public bool IsUseDueToLateness { get; set; }
        public bool IsUsingCheckboxAttendance { get; set; }
    }

    public static class WorkHabitExtensions
    {
        public static void Mapping(this WorkHabit data, IEnumerable<MsMappingAttendance> listMappingAttendance,
            IEnumerable<MsMappingAttendanceWorkhabit> listMappingAttendanceWorkhabit)
        {
            var mappingAttendanceWorkhabit =
                listMappingAttendanceWorkhabit.FirstOrDefault(e => e.Id == data.IdMappingAttendanceWorkHabit);
            if (mappingAttendanceWorkhabit == null) return;

            data.IdMappingAttendance = mappingAttendanceWorkhabit.IdMappingAttendance;
            data.IdWorkHabit = mappingAttendanceWorkhabit.IdWorkhabit;

            var mappingAttendance = listMappingAttendance.FirstOrDefault(e => e.Id == data.IdMappingAttendance);
            if (mappingAttendance == null) return;

            data.IdLevel = mappingAttendance.IdLevel;
            data.AbsentTerm = mappingAttendance.AbsentTerms;
            data.IsNeedValidation = mappingAttendance.IsNeedValidation;
            data.IsUseWorkHabit = mappingAttendance.IsUseWorkhabit;
            data.IsUseDueToLateness = mappingAttendance.IsUseDueToLateness;
            data.IsUsingCheckboxAttendance = mappingAttendance.UsingCheckboxAttendance;
        }
    }
}

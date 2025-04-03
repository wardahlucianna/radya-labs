namespace BinusSchool.Attendance.FnAttendance.Constants
{
    public static class RedisKeyConstants
    {
        public static string GetPeriodKey(string idAcademicYear, string idLevel)
            => $"PERIOD_{idAcademicYear}_{idLevel}";

        public static string GetHomeroomStudentEnrollmentKey(string idAcademicYear, string idLevel)
            => $"HOMEROOMSTUDENTENROLLMENT_{idAcademicYear}_{idLevel}";

        public static string GetHomeroomStudentEnrollmentTransactionKey(string idAcademicYear, string idLevel)
            => $"HOMEROOMSTUDENTENROLLMENTTRANSACTIONKEY_{idAcademicYear}_{idLevel}";

        public static string GetScheduleKey(string idAcademicYear, string idLevel)
            => $"SCHEDULE_{idAcademicYear}_{idLevel}";

        public static string GetScheduleLessonKey(string idAcademicYear, string idLevel)
            => $"SCHEDULELESSON_{idAcademicYear}_{idLevel}";

        public static string GetHomeroomTeacherKey(string idAcademicYear, string idLevel)
            => $"HOMEROOMTEACHER_{idAcademicYear}_{idLevel}";

        public static string GetNonTeachingLoadKey(string idAcademicYear)
            => $"NONTEACHINGLOAD_{idAcademicYear}";

        public static string GetDepartmentLevelKey(string idAcademicYear)
            => $"DEPARTMENTLEVEL_{idAcademicYear}";
    }
}

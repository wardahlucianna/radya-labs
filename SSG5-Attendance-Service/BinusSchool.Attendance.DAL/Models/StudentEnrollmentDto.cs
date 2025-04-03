using System;
using System.Collections.Generic;
using BinusSchool.Common.Utils;

namespace BinusSchool.Persistence.AttendanceDb.Models
{
    public class StudentEnrollmentDto
    {
        public string IdHomeroomStudent { get; set; }
        public string IdStudent { get; set; }
        public List<StudentEnrollmentItemDto> Items { get; set; } = new List<StudentEnrollmentItemDto>();
    }

    public class StudentEnrollmentDto2
    {
        public string IdHomeroomStudent { get; set; }
        public string IdStudent { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string FullName => NameUtil.GenerateFullName(FirstName, MiddleName, LastName);
        public string ClassroomCode { get; set; }
        public string GraceCode { get; set; }
        public string GraceId { get; set; }
        public string IdHomeroom { get; set; }
        public List<StudentEnrollmentItemDto> Items { get; set; } = new List<StudentEnrollmentItemDto>();
    }

    public class StudentEnrollmentItemDto
    {
        public string IdLesson { get; set; }
        public DateTime StartDt { get; set; }
        public DateTime EndDt { get; set; } = DateTime.MaxValue;
        public bool Ignored { get; set; }
    }

    public class HomeroomStudentEnrollDto
    {
        public string IdHomeroomStudentEnrollment { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdLessonOld { get; set; }
        public string IdLesson { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateIn { get; set; }
        public bool IsDeleted { get; set; }
        public int Flag { get; set; }
        public bool IsFromHistory { get; set; }
    }
}

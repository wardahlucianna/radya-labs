using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.Achievement
{
    public class GetAchievementRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public string Status { get; set; }
        public string IdUser { get; set; }
        /// <summary>
        /// role di tulis dengan uppercase
        /// </summary>
        public string Role { get; set; }

        public EntryMeritStudentType? Type { get; set; }
    }
}

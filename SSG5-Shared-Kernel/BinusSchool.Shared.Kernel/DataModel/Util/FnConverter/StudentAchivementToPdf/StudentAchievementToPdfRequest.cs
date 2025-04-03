using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Util.FnConverter.StudentAchivementToPdf
{
    public class StudentAchievementToPdfRequest
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
        public string IdSchool { get; set; }
        public string Search { get; set; }
        public string OrderBy { get; set; }
        public OrderType OrderType { get; set; }
    }
}

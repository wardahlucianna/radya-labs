namespace BinusSchool.Data.Model.School.FnSchool.Grade
{
    public class GetGradeRequest : CollectionSchoolRequest
    {
        public string IdAcadyear { get; set; }
        public string IdLevel { get; set; }
        public bool IsRemoveLastGrade { get; set; } = false;
    }
}

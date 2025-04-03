using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class GetStudentEnrollmentScoreResult
    {
        public string IdAcademicYear { set; get; }
        public string AcademicYearName { set; get; }
     
        public List<GetStudentEnrollmentScore_SemesterVm> SemesterList { set; get; }
    }

    public class GetStudentEnrollmentScore_SemesterVm
    {
        public int Semester { set; get; }
        public string IdLevel { set; get; }
        public string LevelName { set; get; }
        public string IdGrade { set; get; }
        public string GradeName { set; get; }
        public bool ViewAsCambridge { set; get; }

    }

    public class GetStudentEnrollmentScore_AYVm
    {
        public string IdAcademicYear { set; get; }
        public string AcademicYearName { set; get; }
        public int Semester { set; get; }
        public string IdLevel { set; get; }
        public string LevelName { set; get; }
        public string IdGrade { set; get; }
        public string GradeName { set; get; }
        public string ViewAsCambridge { set; get; }

    }
}

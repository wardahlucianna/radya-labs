using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByHomeroom
{
    public class GetStudentScoreByHomeroomRequest
    {        
        public string IdSchool { set; get; }
        public string IdAcademicYear {set; get;}
        public int Semester {set; get;}
        public string IdGrade {set; get;}
        public string IdHomeroom {set; get;}
        public string IdSubjectType { set; get; }
        public decimal? MinScore { set; get; }
        public decimal? MaxScore { set; get; }
        public PrivilageStudentScoreByHomeroom AuthPrivilage { set; get; }
    }
}
